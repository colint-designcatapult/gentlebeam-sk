using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Heracles.Ucsi.Storage;
using Parquet.Schema;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Ucsi.Models;

public abstract class TelemetryParameterDescriptor
{
    protected TelemetryParameterDescriptor(
        string id,
        string displayName,
        string group,
        string unit,
        string axisKey,
        string parquetColumnName,
        Type valueType,
        TelemetryValueKind valueKind,
        bool isMock)
    {
        Id = id;
        DisplayName = displayName;
        Group = group;
        Unit = unit;
        AxisKey = axisKey;
        ParquetColumnName = parquetColumnName;
        ValueType = valueType;
        ValueKind = valueKind;
        IsMock = isMock;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public string Group { get; }
    public string Unit { get; }
    public string AxisKey { get; }
    public string ParquetColumnName { get; }
    public Type ValueType { get; }
    public TelemetryValueKind ValueKind { get; }
    public bool IsMock { get; }

    public abstract object? GetValue(UcsiTelemetrySample sample);
    public abstract string Format(UcsiTelemetrySample sample);
    public abstract double? Project(
        UcsiTelemetrySample sample,
        IDictionary<string, double> categories);
    public abstract double? ProjectValue(
        object? value,
        IDictionary<string, double> categories);

    internal abstract DataField CreateParquetField();
    internal abstract IParquetColumnBuffer CreateParquetBuffer(int capacity);
}

public sealed class TelemetryParameterDescriptor<T> : TelemetryParameterDescriptor
{
    private readonly Func<UcsiTelemetrySample, T> _accessor;
    private readonly Func<T, string> _formatter;
    private readonly Func<T, double?>? _numericProjector;
    private readonly Func<T, string> _categoryKey;

    internal TelemetryParameterDescriptor(
        string id,
        string displayName,
        string group,
        string unit,
        string axisKey,
        string parquetColumnName,
        TelemetryValueKind valueKind,
        bool isMock,
        Func<UcsiTelemetrySample, T> accessor,
        Func<T, string> formatter,
        Func<T, double?>? numericProjector,
        Func<T, string> categoryKey)
        : base(
            id,
            displayName,
            group,
            unit,
            axisKey,
            parquetColumnName,
            typeof(T),
            valueKind,
            isMock)
    {
        _accessor = accessor;
        _formatter = formatter;
        _numericProjector = numericProjector;
        _categoryKey = categoryKey;
    }

    public override object? GetValue(UcsiTelemetrySample sample) => _accessor(sample);

    public override string Format(UcsiTelemetrySample sample) => _formatter(_accessor(sample));

    public override double? Project(
        UcsiTelemetrySample sample,
        IDictionary<string, double> categories) =>
        ProjectValue(_accessor(sample), categories);

    public override double? ProjectValue(
        object? value,
        IDictionary<string, double> categories)
    {
        if (value is null)
            return null;
        if (value is not T typedValue)
            throw new ArgumentException(
                $"Value for '{Id}' must be {typeof(T).Name}.",
                nameof(value));

        if (_numericProjector is not null)
            return _numericProjector(typedValue);

        string key = _categoryKey(typedValue);
        if (!categories.TryGetValue(key, out double category))
        {
            category = categories.Count;
            categories.Add(key, category);
        }
        return category;
    }

    internal override DataField CreateParquetField() =>
        ParquetColumnBufferFactory.CreateField<T>(ParquetColumnName);

    internal override IParquetColumnBuffer CreateParquetBuffer(int capacity) =>
        ParquetColumnBufferFactory.Create(ParquetColumnName, _accessor, capacity);
}

public sealed class TelemetryParameterCatalog
{
    private static readonly HashSet<string> RootContainers =
    [
        nameof(ISystemTelemetry.Faults),
        nameof(ISystemTelemetry.Interlocks),
        nameof(ISystemTelemetry.Hvps),
        nameof(ISystemTelemetry.Mag1),
        nameof(ISystemTelemetry.Mag2),
    ];

    private static readonly MethodInfo CreateGenericDescriptorMethod = typeof(TelemetryParameterCatalog)
        .GetMethod(nameof(CreateGenericDescriptor), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly ReadOnlyCollection<TelemetryParameterDescriptor> _all;
    private readonly IReadOnlyDictionary<string, TelemetryParameterDescriptor> _byId;

    public TelemetryParameterCatalog()
    {
        var descriptors = new List<TelemetryParameterDescriptor>();
        BuildSystemDescriptors(descriptors);
        BuildFaultSlotDescriptors(descriptors);
        BuildMockDescriptors(descriptors);

        var byId = new Dictionary<string, TelemetryParameterDescriptor>(StringComparer.Ordinal);
        var parquetNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (TelemetryParameterDescriptor descriptor in descriptors)
        {
            if (!byId.TryAdd(descriptor.Id, descriptor))
                throw new InvalidOperationException($"Duplicate telemetry descriptor ID '{descriptor.Id}'.");
            if (!parquetNames.Add(descriptor.ParquetColumnName))
                throw new InvalidOperationException($"Duplicate Parquet column '{descriptor.ParquetColumnName}'.");
        }

        _all = descriptors.AsReadOnly();
        _byId = byId;
    }

    public IReadOnlyList<TelemetryParameterDescriptor> All => _all;
    public IReadOnlyDictionary<string, TelemetryParameterDescriptor> ById => _byId;

    public TelemetryParameterDescriptor GetRequired(string id) =>
        _byId.TryGetValue(id, out TelemetryParameterDescriptor? descriptor)
            ? descriptor
            : throw new KeyNotFoundException($"Unknown telemetry parameter '{id}'.");

    private static void BuildSystemDescriptors(List<TelemetryParameterDescriptor> descriptors)
    {
        ParameterExpression sample = Expression.Parameter(typeof(UcsiTelemetrySample), "sample");
        Expression telemetry = Expression.Property(sample, nameof(UcsiTelemetrySample.Telemetry));

        foreach (PropertyInfo property in typeof(ISystemTelemetry).GetProperties())
        {
            if (RootContainers.Contains(property.Name))
                continue;

            AddPropertyDescriptor(
                descriptors,
                sample,
                Expression.Property(telemetry, property),
                $"system.{property.Name}",
                Humanize(property.Name),
                "System",
                UnitFor(property.Name),
                IsIdentifier(property.Name));
        }

        Add(
            descriptors,
            "system.IsFaultState",
            "Fault State",
            "System",
            string.Empty,
            sampleValue => sampleValue.Telemetry.IsFaultState());
        Add(
            descriptors,
            "system.IsEmissionState",
            "Emission State",
            "System",
            string.Empty,
            sampleValue => sampleValue.Telemetry.IsEmissionState());
        
        // Power Feedback: computed from KvFeedback × EmissionCurrent (same as HVPS tab FB display)
        Add(
            descriptors,
            "system.PowerFeedback",
            "Power Feedback",
            "System",
            "W",
            sampleValue => (double)sampleValue.Telemetry.KvFeedback * (double)sampleValue.Telemetry.EmissionCurrent);

        Expression faults = Expression.Property(telemetry, nameof(ISystemTelemetry.Faults));
        AddContainerProperties(
            descriptors,
            sample,
            faults,
            typeof(SystemFaults),
            "system.Faults",
            "Faults",
            [nameof(SystemFaults.RawFlags), nameof(SystemFaults.RawCommunicationFlags), nameof(SystemFaults.AnyActive)]);
        foreach (SystemFault fault in Enum.GetValues<SystemFault>())
        {
            SystemFault captured = fault;
            Add(
                descriptors,
                $"system.Faults.{captured}",
                GetEnumDisplayName(captured),
                "Faults",
                string.Empty,
                sampleValue => sampleValue.Telemetry.Faults.GetState(captured));
        }

        Expression interlocks = Expression.Property(telemetry, nameof(ISystemTelemetry.Interlocks));
        AddContainerProperties(
            descriptors,
            sample,
            interlocks,
            typeof(SystemInterlocks),
            "system.Interlocks",
            "Interlocks",
            typeof(SystemInterlocks).GetProperties().Select(property => property.Name));
        foreach (SystemInterlock interlock in Enum.GetValues<SystemInterlock>())
        {
            SystemInterlock captured = interlock;
            Add(
                descriptors,
                $"system.Interlocks.{captured}.Required",
                $"{GetEnumDisplayName(captured)} Required",
                "Interlocks",
                string.Empty,
                sampleValue => sampleValue.Telemetry.Interlocks.IsRequired(captured));
        }

        Expression hvps = Expression.Property(telemetry, nameof(ISystemTelemetry.Hvps));
        AddContainerProperties(
            descriptors,
            sample,
            hvps,
            typeof(HvpsTelemetryStatus),
            "system.Hvps",
            "HVPS",
            typeof(HvpsTelemetryStatus).GetProperties().Select(property => property.Name));

        AddNullableVector(descriptors, sample, telemetry, nameof(ISystemTelemetry.Mag1));
        AddNullableVector(descriptors, sample, telemetry, nameof(ISystemTelemetry.Mag2));
    }

    private static void AddNullableVector(
        List<TelemetryParameterDescriptor> descriptors,
        ParameterExpression sample,
        Expression telemetry,
        string propertyName)
    {
        Expression nullableVector = Expression.Property(telemetry, propertyName);
        Expression hasValue = Expression.Property(nullableVector, nameof(Nullable<int>.HasValue));
        Expression vector = Expression.Property(nullableVector, nameof(Nullable<int>.Value));
        foreach (string component in new[] { nameof(TelemetryVector3.X), nameof(TelemetryVector3.Y), nameof(TelemetryVector3.Z) })
        {
            Expression value = Expression.Condition(
                hasValue,
                Expression.Convert(Expression.Property(vector, component), typeof(float?)),
                Expression.Default(typeof(float?)));
            AddPropertyDescriptor(
                descriptors,
                sample,
                value,
                $"system.{propertyName}.{component}",
                $"{propertyName} {component}",
                "Magnetometers",
                "µT",
                false);
        }
    }

    private static void AddContainerProperties(
        List<TelemetryParameterDescriptor> descriptors,
        ParameterExpression sample,
        Expression container,
        Type containerType,
        string idPrefix,
        string group,
        IEnumerable<string> propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            PropertyInfo property = containerType.GetProperty(propertyName)
                ?? throw new InvalidOperationException($"Missing {containerType.Name}.{propertyName}.");
            AddPropertyDescriptor(
                descriptors,
                sample,
                Expression.Property(container, property),
                $"{idPrefix}.{property.Name}",
                Humanize(property.Name),
                group,
                UnitFor(property.Name),
                IsIdentifier(property.Name));
        }
    }

    private static void BuildFaultSlotDescriptors(List<TelemetryParameterDescriptor> descriptors)
    {
        Add(
            descriptors,
            "faults.Count",
            "Active Fault Count",
            "Detailed Faults",
            string.Empty,
            sample => sample.ActiveFaults.Count,
            parquetColumnName: "FaultCount");

        for (int slot = 0; slot < 4; slot++)
        {
            int captured = slot;
            string prefix = $"faults.{captured}";
            string columnPrefix = $"Fault{captured}_";
            Add(descriptors, $"{prefix}.FaultType", $"Fault {captured + 1} Type", "Detailed Faults", string.Empty,
                sample => captured < sample.ActiveFaults.Count ? sample.ActiveFaults[captured].FaultType : (SystemFault?)null,
                parquetColumnName: $"{columnPrefix}FaultType");
            Add(descriptors, $"{prefix}.FormatHash", $"Fault {captured + 1} Format Hash", "Detailed Faults", string.Empty,
                sample => captured < sample.ActiveFaults.Count ? sample.ActiveFaults[captured].FormatHash : (uint?)null,
                isIdentifier: true,
                parquetColumnName: $"{columnPrefix}FormatHash");
            Add(descriptors, $"{prefix}.CapturedState", $"Fault {captured + 1} Captured State", "Detailed Faults", string.Empty,
                sample => captured < sample.ActiveFaults.Count ? sample.ActiveFaults[captured].CapturedState : (GcbStateNew?)null,
                parquetColumnName: $"{columnPrefix}CapturedState");
            Add(descriptors, $"{prefix}.CapturedRuntime", $"Fault {captured + 1} Captured Runtime", "Detailed Faults", "ms",
                sample => captured < sample.ActiveFaults.Count ? sample.ActiveFaults[captured].CapturedRuntime : (uint?)null,
                parquetColumnName: $"{columnPrefix}CapturedRuntime");
            Add(descriptors, $"{prefix}.Format", $"Fault {captured + 1} Format", "Detailed Faults", string.Empty,
                sample => captured < sample.ActiveFaults.Count ? sample.ActiveFaults[captured].Format : null,
                parquetColumnName: $"{columnPrefix}Format");
            Add(descriptors, $"{prefix}.Message", $"Fault {captured + 1} Message", "Detailed Faults", string.Empty,
                sample => captured < sample.ActiveFaults.Count ? sample.ActiveFaults[captured].Message : null,
                parquetColumnName: $"{columnPrefix}Message");
        }
    }

    private static void BuildMockDescriptors(List<TelemetryParameterDescriptor> descriptors)
    {
        Add(descriptors, "mock.GcbFirmware", "GCB Firmware", "Mock", string.Empty, _ => "v3.0.0 abc123", isMock: true);
        Add(descriptors, "mock.HvpsFirmware", "HVPS Firmware", "Mock", string.Empty, _ => "v3.0.0 def456", isMock: true);
        foreach (string component in new[] { "InternalPc", "ControlBoard", "HvpsBoard", "Hvps", "HeadBoard" })
        {
            Add(descriptors, $"mock.Comms.{component}", $"{Humanize(component)} Communications", "Mock", string.Empty, _ => true, isMock: true);
        }
        Add(descriptors, "mock.DalsaAvailable", "DALSA Available", "Mock", string.Empty, _ => false, isMock: true);
        foreach (string command in new[] { "Kv", "Emission", "Grid", "Heater" })
        {
            Add(descriptors, $"mock.Command.{command}", $"{command} Command", "Mock", UnitFor(command), _ => 0.0, isMock: true);
        }
        // Emission Setpoint is derived (Power / HV) and not useful for monitoring, kept as mock
        Add<double?>(descriptors, "mock.Setpoint.Emission", "Emission Setpoint", "Mock", "mA", _ => null, isMock: true);
    }

    private static void AddPropertyDescriptor(
        List<TelemetryParameterDescriptor> descriptors,
        ParameterExpression sample,
        Expression value,
        string id,
        string displayName,
        string group,
        string unit,
        bool isIdentifier)
    {
        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(UcsiTelemetrySample), value.Type);
        Delegate accessor = Expression.Lambda(delegateType, value, sample).Compile();
        MethodInfo factory = CreateGenericDescriptorMethod.MakeGenericMethod(value.Type);
        var descriptor = (TelemetryParameterDescriptor)factory.Invoke(
            null,
            [id, displayName, group, unit, isIdentifier, null, false, accessor])!;
        descriptors.Add(descriptor);
    }

    private static TelemetryParameterDescriptor CreateGenericDescriptor<T>(
        string id,
        string displayName,
        string group,
        string unit,
        bool isIdentifier,
        string? parquetColumnName,
        bool isMock,
        Delegate accessor) =>
        CreateDescriptor(
            id,
            displayName,
            group,
            unit,
            (Func<UcsiTelemetrySample, T>)accessor,
            isIdentifier,
            parquetColumnName,
            isMock);

    private static void Add<T>(
        List<TelemetryParameterDescriptor> descriptors,
        string id,
        string displayName,
        string group,
        string unit,
        Func<UcsiTelemetrySample, T> accessor,
        bool isIdentifier = false,
        string? parquetColumnName = null,
        bool isMock = false) =>
        descriptors.Add(CreateDescriptor(
            id,
            displayName,
            group,
            unit,
            accessor,
            isIdentifier,
            parquetColumnName,
            isMock));

    private static TelemetryParameterDescriptor<T> CreateDescriptor<T>(
        string id,
        string displayName,
        string group,
        string unit,
        Func<UcsiTelemetrySample, T> accessor,
        bool isIdentifier,
        string? parquetColumnName,
        bool isMock)
    {
        TelemetryValueKind kind = GetValueKind(typeof(T), isIdentifier);
        string axisKey = kind switch
        {
            TelemetryValueKind.Boolean => "Boolean",
            TelemetryValueKind.Enum => Nullable.GetUnderlyingType(typeof(T))?.FullName ?? typeof(T).FullName ?? id,
            TelemetryValueKind.String or TelemetryValueKind.Identifier => id,
            _ => string.IsNullOrWhiteSpace(unit) ? "Numeric" : unit,
        };

        return new TelemetryParameterDescriptor<T>(
            id,
            displayName,
            group,
            unit,
            axisKey,
            parquetColumnName ?? id.Replace('.', '_'),
            kind,
            isMock,
            accessor,
            value => FormatValue(value, unit, kind),
            CreateNumericProjector<T>(kind),
            value => CategoryKey(value, kind));
    }

    private static TelemetryValueKind GetValueKind(Type type, bool isIdentifier)
    {
        if (isIdentifier)
            return TelemetryValueKind.Identifier;

        Type actual = Nullable.GetUnderlyingType(type) ?? type;
        if (actual == typeof(bool))
            return TelemetryValueKind.Boolean;
        if (actual.IsEnum)
            return TelemetryValueKind.Enum;
        if (actual == typeof(string))
            return TelemetryValueKind.String;
        return TelemetryValueKind.Numeric;
    }

    private static Func<T, double?>? CreateNumericProjector<T>(TelemetryValueKind kind)
    {
        if (kind is TelemetryValueKind.String or TelemetryValueKind.Identifier)
            return null;

        Type type = typeof(T);
        Type actual = Nullable.GetUnderlyingType(type) ?? type;
        ParameterExpression value = Expression.Parameter(type, "value");
        Expression raw = Nullable.GetUnderlyingType(type) is not null
            ? Expression.Property(value, nameof(Nullable<int>.Value))
            : value;
        Expression numeric = actual == typeof(bool)
            ? Expression.Condition(raw, Expression.Constant(1.0), Expression.Constant(0.0))
            : Expression.Convert(actual.IsEnum ? Expression.Convert(raw, Enum.GetUnderlyingType(actual)) : raw, typeof(double));
        Expression projected = Expression.Convert(numeric, typeof(double?));

        if (Nullable.GetUnderlyingType(type) is not null)
        {
            projected = Expression.Condition(
                Expression.Property(value, nameof(Nullable<int>.HasValue)),
                projected,
                Expression.Default(typeof(double?)));
        }

        return Expression.Lambda<Func<T, double?>>(projected, value).Compile();
    }

    private static string FormatValue<T>(T value, string unit, TelemetryValueKind kind)
    {
        if (value is null)
            return "N/A";

        object boxed = value;
        string formatted = kind switch
        {
            TelemetryValueKind.Boolean => (bool)boxed ? "True" : "False",
            TelemetryValueKind.Enum => boxed is Enum enumValue ? GetEnumDisplayName(enumValue) : boxed.ToString()!,
            TelemetryValueKind.Identifier => FormatIdentifier(boxed),
            TelemetryValueKind.Numeric when boxed is float single => single.ToString("0.###", CultureInfo.InvariantCulture),
            TelemetryValueKind.Numeric when boxed is double number => number.ToString("0.###", CultureInfo.InvariantCulture),
            TelemetryValueKind.Numeric when boxed is decimal number => number.ToString("0.###", CultureInfo.InvariantCulture),
            _ => Convert.ToString(boxed, CultureInfo.InvariantCulture) ?? "N/A",
        };
        return string.IsNullOrWhiteSpace(unit) || formatted == "N/A"
            ? formatted
            : $"{formatted} {unit}";
    }

    private static string CategoryKey<T>(T value, TelemetryValueKind kind)
    {
        if (value is null)
            return "N/A";
        object boxed = value;
        return kind == TelemetryValueKind.Identifier
            ? FormatIdentifier(boxed)
            : Convert.ToString(boxed, CultureInfo.InvariantCulture) ?? "N/A";
    }

    private static string FormatIdentifier(object value) => value switch
    {
        byte number => $"0x{number:X2}",
        ushort number => $"0x{number:X4}",
        uint number => $"0x{number:X8}",
        ulong number => $"0x{number:X16}",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "N/A",
    };

    private static string UnitFor(string propertyName)
    {
        if (propertyName.Contains("Kv", StringComparison.OrdinalIgnoreCase)) return "kV";
        if (propertyName.Contains("Emission", StringComparison.OrdinalIgnoreCase)) return "mA";
        if (propertyName.Contains("HeaterCurrent", StringComparison.OrdinalIgnoreCase)) return "mA";
        if (propertyName.Contains("Power", StringComparison.OrdinalIgnoreCase)) return "W";
        if (propertyName.Contains("Grid", StringComparison.OrdinalIgnoreCase)) return "V";
        if (propertyName.Contains("CoilCurrent", StringComparison.OrdinalIgnoreCase) || propertyName == nameof(ISystemTelemetry.FocusCurrent)) return "A";
        if (propertyName.Contains("Temperature", StringComparison.OrdinalIgnoreCase)) return "°C";
        if (propertyName.Contains("Pressure", StringComparison.OrdinalIgnoreCase)) return "psi";
        if (propertyName.Contains("FlowRate", StringComparison.OrdinalIgnoreCase)) return "L/min";
        if (propertyName.Contains("Runtime", StringComparison.OrdinalIgnoreCase)) return "ms";
        return string.Empty;
    }

    private static bool IsIdentifier(string propertyName) =>
        propertyName.Contains("Collimator", StringComparison.OrdinalIgnoreCase)
        || propertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
        || propertyName.EndsWith("Flags", StringComparison.OrdinalIgnoreCase);

    private static string Humanize(string value)
    {
        var builder = new StringBuilder(value.Length + 8);
        for (int index = 0; index < value.Length; index++)
        {
            char current = value[index];
            if (index > 0 && char.IsUpper(current) && !char.IsUpper(value[index - 1]))
                builder.Append(' ');
            builder.Append(current);
        }
        return builder.ToString();
    }

    private static string GetEnumDisplayName(Enum value)
    {
        MemberInfo? member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
    }
}
