using System.Linq.Expressions;
using System.Reflection;
using Heracles.Ucsi.Models;
using Parquet;
using Parquet.Schema;

namespace Heracles.Ucsi.Storage;

internal interface IParquetColumnBuffer
{
    DataField Field { get; }
    int Count { get; }
    void Append(UcsiTelemetrySample sample);
    Task WriteAsync(ParquetRowGroupWriter writer, CancellationToken cancellationToken);
    void Clear();
}

internal static class ParquetColumnBufferFactory
{
    private static readonly MethodInfo CreateValueMethod = GetFactoryMethod(nameof(CreateValue));
    private static readonly MethodInfo CreateNullableValueMethod = GetFactoryMethod(nameof(CreateNullableValue));
    private static readonly MethodInfo CreateEnumMethod = GetFactoryMethod(nameof(CreateEnum));
    private static readonly MethodInfo CreateNullableEnumMethod = GetFactoryMethod(nameof(CreateNullableEnum));

    public static DataField CreateField<T>(string name)
    {
        Type type = typeof(T);
        Type? nullableType = Nullable.GetUnderlyingType(type);
        Type actualType = nullableType ?? type;
        Type storageType = actualType.IsEnum ? typeof(int) : actualType;
        return new DataField(name, storageType, nullableType is not null || !type.IsValueType);
    }

    public static IParquetColumnBuffer Create<T>(
        string name,
        Func<UcsiTelemetrySample, T> accessor,
        int capacity)
    {
        Type type = typeof(T);
        if (type == typeof(string))
        {
            return new StringParquetColumnBuffer(
                new DataField<string>(name, true),
                (Func<UcsiTelemetrySample, string?>)(object)accessor,
                capacity);
        }

        Type? nullableType = Nullable.GetUnderlyingType(type);
        Type actualType = nullableType ?? type;
        MethodInfo method = actualType.IsEnum
            ? nullableType is null ? CreateEnumMethod : CreateNullableEnumMethod
            : nullableType is null ? CreateValueMethod : CreateNullableValueMethod;
        return (IParquetColumnBuffer)method.MakeGenericMethod(actualType).Invoke(
            null,
            [name, accessor, capacity])!;
    }

    private static MethodInfo GetFactoryMethod(string name) => typeof(ParquetColumnBufferFactory)
        .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;

    private static IParquetColumnBuffer CreateValue<TValue>(
        string name,
        Delegate accessor,
        int capacity)
        where TValue : struct =>
        new ValueParquetColumnBuffer<TValue>(
            new DataField<TValue>(name, false),
            (Func<UcsiTelemetrySample, TValue>)accessor,
            capacity);

    private static IParquetColumnBuffer CreateNullableValue<TValue>(
        string name,
        Delegate accessor,
        int capacity)
        where TValue : struct =>
        new NullableValueParquetColumnBuffer<TValue>(
            new DataField<TValue>(name, true),
            (Func<UcsiTelemetrySample, TValue?>)accessor,
            capacity);

    private static IParquetColumnBuffer CreateEnum<TEnum>(
        string name,
        Delegate accessor,
        int capacity)
        where TEnum : struct, Enum
    {
        var typedAccessor = (Func<UcsiTelemetrySample, TEnum>)accessor;
        ParameterExpression sample = Expression.Parameter(typeof(UcsiTelemetrySample), "sample");
        Expression converted = Expression.Convert(
            Expression.Convert(Expression.Invoke(Expression.Constant(typedAccessor), sample), Enum.GetUnderlyingType(typeof(TEnum))),
            typeof(int));
        var integerAccessor = Expression.Lambda<Func<UcsiTelemetrySample, int>>(converted, sample).Compile();
        return new ValueParquetColumnBuffer<int>(new DataField<int>(name, false), integerAccessor, capacity);
    }

    private static IParquetColumnBuffer CreateNullableEnum<TEnum>(
        string name,
        Delegate accessor,
        int capacity)
        where TEnum : struct, Enum
    {
        var typedAccessor = (Func<UcsiTelemetrySample, TEnum?>)accessor;
        ParameterExpression sample = Expression.Parameter(typeof(UcsiTelemetrySample), "sample");
        Expression value = Expression.Invoke(Expression.Constant(typedAccessor), sample);
        Expression converted = Expression.Condition(
            Expression.Property(value, nameof(Nullable<int>.HasValue)),
            Expression.Convert(
                Expression.Convert(
                    Expression.Property(value, nameof(Nullable<int>.Value)),
                    Enum.GetUnderlyingType(typeof(TEnum))),
                typeof(int?)),
            Expression.Default(typeof(int?)));
        var integerAccessor = Expression.Lambda<Func<UcsiTelemetrySample, int?>>(converted, sample).Compile();
        return new NullableValueParquetColumnBuffer<int>(new DataField<int>(name, true), integerAccessor, capacity);
    }
}

internal sealed class ValueParquetColumnBuffer<T>(
    DataField<T> field,
    Func<UcsiTelemetrySample, T> accessor,
    int capacity) : IParquetColumnBuffer
    where T : struct
{
    private readonly T[] _values = new T[capacity];

    public DataField Field => field;
    public int Count { get; private set; }

    public void Append(UcsiTelemetrySample sample)
    {
        if (Count == _values.Length)
            throw new InvalidOperationException("Parquet column buffer is full.");
        _values[Count++] = accessor(sample);
    }

    public Task WriteAsync(ParquetRowGroupWriter writer, CancellationToken cancellationToken) =>
        writer.WriteAsync<T>(
            field,
            new ReadOnlyMemory<T>(_values, 0, Count),
            cancellationToken: cancellationToken);

    public void Clear() => Count = 0;
}

internal sealed class NullableValueParquetColumnBuffer<T>(
    DataField<T> field,
    Func<UcsiTelemetrySample, T?> accessor,
    int capacity) : IParquetColumnBuffer
    where T : struct
{
    private readonly T?[] _values = new T?[capacity];

    public DataField Field => field;
    public int Count { get; private set; }

    public void Append(UcsiTelemetrySample sample)
    {
        if (Count == _values.Length)
            throw new InvalidOperationException("Parquet column buffer is full.");
        _values[Count++] = accessor(sample);
    }

    public Task WriteAsync(ParquetRowGroupWriter writer, CancellationToken cancellationToken) =>
        writer.WriteAsync<T>(
            field,
            new ReadOnlyMemory<T?>(_values, 0, Count),
            cancellationToken: cancellationToken);

    public void Clear() => Count = 0;
}

internal sealed class StringParquetColumnBuffer(
    DataField<string> field,
    Func<UcsiTelemetrySample, string?> accessor,
    int capacity) : IParquetColumnBuffer
{
    private readonly string?[] _values = new string?[capacity];

    public DataField Field => field;
    public int Count { get; private set; }

    public void Append(UcsiTelemetrySample sample)
    {
        if (Count == _values.Length)
            throw new InvalidOperationException("Parquet column buffer is full.");
        _values[Count++] = accessor(sample);
    }

    public Task WriteAsync(ParquetRowGroupWriter writer, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<string?> values = Count == _values.Length
            ? _values
            : new ArraySegment<string?>(_values, 0, Count);
        return writer.WriteAsync(field, values);
    }

    public void Clear()
    {
        Array.Clear(_values, 0, Count);
        Count = 0;
    }
}
