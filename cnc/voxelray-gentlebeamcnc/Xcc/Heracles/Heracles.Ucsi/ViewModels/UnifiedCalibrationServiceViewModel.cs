using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Prism.Commands;
using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Ucsi.ViewModels;

public interface IUcsiHostCommands
{
    bool CanClearFaults { get; }
    string ClearFaultsUnavailableReason { get; }
    Task ClearFaultsAsync();
}

/// <summary>
/// Default stub implementation - disables fault clearing for safety.
/// Used when UCSI is embedded in the main application to prevent accidental emission start.
/// </summary>
public sealed class UnavailableUcsiHostCommands : IUcsiHostCommands
{
    public bool CanClearFaults => false;
    public string ClearFaultsUnavailableReason => "Clear Faults is unavailable in this host.";
    public Task ClearFaultsAsync() => Task.CompletedTask;
}

/// <summary>
/// Standalone UCSI implementation of IUcsiHostCommands.
/// Enables fault clearing for bench/standalone use.
/// </summary>
public sealed class StandaloneUcsiHostCommands(
    IGcbCommandInterface gcbCommandInterface) : IUcsiHostCommands
{
    public bool CanClearFaults => true;
    public string ClearFaultsUnavailableReason => string.Empty;
    public Task ClearFaultsAsync() => gcbCommandInterface.ClearFaults();
}

public sealed class CheckableParameterViewModel : BindableBase
{
    private bool _isSelected;

    public CheckableParameterViewModel(TelemetryParameterDescriptor descriptor)
    {
        Descriptor = descriptor;
    }

    public TelemetryParameterDescriptor Descriptor { get; }
    public string Id => Descriptor.Id;
    public string DisplayName => Descriptor.DisplayName;
    public string Group => Descriptor.Group;
    public string Unit => Descriptor.Unit;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class MonitoredParameterViewModel(
    TelemetryParameterDescriptor descriptor) : BindableBase
{
    private string _value = "N/A";

    public TelemetryParameterDescriptor Descriptor { get; } = descriptor;
    public string DisplayName => Descriptor.DisplayName;
    public string Group => Descriptor.Group;
    public string Unit => Descriptor.Unit;

    public string Value
    {
        get => _value;
        private set => SetProperty(ref _value, value);
    }

    public void Update(UcsiTelemetrySample? sample) =>
        Value = sample is null ? "N/A" : Descriptor.Format(sample.Value);
}

public sealed class TelemetryStateItemViewModel(string name) : BindableBase
{
    private string _value = "N/A";
    private bool _isActive;
    private bool _isAvailable = true;

    public string Name { get; } = name;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
    public bool IsAvailable
    {
        get => _isAvailable;
        set => SetProperty(ref _isAvailable, value);
    }
}

public sealed class SystemConfigItem : BindableBase
{
    private double _currentValue;
    private string _inputValue = string.Empty;

    public SystemConfigItem(string name, double currentValue, int firmwareIndex, DelegateCommand setCommand)
    {
        Name = name;
        CurrentValue = currentValue;
        FirmwareIndex = firmwareIndex;
        SetCommand = setCommand;
    }

    public string Name { get; }
    public int FirmwareIndex { get; }
    public double CurrentValue
    {
        get => _currentValue;
        set => SetProperty(ref _currentValue, value);
    }
    public string InputValue
    {
        get => _inputValue;
        set => SetProperty(ref _inputValue, value);
    }
    public DelegateCommand SetCommand { get; }
}

public sealed class GraphPaneViewModel : BindableBase
{
    private readonly Action<GraphPaneViewModel> _remove;
    private string _title;
    private string _filterText = string.Empty;

    public GraphPaneViewModel(
        int number,
        TelemetryParameterCatalog catalog,
        Action<GraphPaneViewModel> remove,
        params string[] selectedIds)
    {
        _remove = remove;
        _title = $"Graph {number}";
        ParameterOptions = new ObservableCollection<CheckableParameterViewModel>(
            catalog.All.Select(descriptor => new CheckableParameterViewModel(descriptor)));
        foreach (CheckableParameterViewModel option in ParameterOptions)
        {
            option.IsSelected = selectedIds.Contains(option.Id, StringComparer.Ordinal);
            option.PropertyChanged += OnOptionChanged;
        }
        ParameterView = CollectionViewSource.GetDefaultView(ParameterOptions);
        ParameterView.Filter = FilterParameter;
        ParameterView.SortDescriptions.Add(new SortDescription(nameof(CheckableParameterViewModel.IsSelected), ListSortDirection.Descending));
        ParameterView.SortDescriptions.Add(new SortDescription(nameof(CheckableParameterViewModel.DisplayName), ListSortDirection.Ascending));
        RemoveCommand = new DelegateCommand(() => _remove(this));
    }

    public event EventHandler? SeriesSelectionChanged;
    public ObservableCollection<CheckableParameterViewModel> ParameterOptions { get; }
    public ICollectionView ParameterView { get; }
    public DelegateCommand RemoveCommand { get; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
                ParameterView.Refresh();
        }
    }

    public IReadOnlyList<string> SelectedParameterIds => ParameterOptions
        .Where(option => option.IsSelected)
        .Select(option => option.Id)
        .ToArray();

    public string SelectionSummary
    {
        get
        {
            string[] names = ParameterOptions
                .Where(option => option.IsSelected)
                .Select(option => option.DisplayName)
                .ToArray();
            return names.Length switch
            {
                0 => "Select series",
                1 => names[0],
                _ => $"{names.Length} series",
            };
        }
    }

    private bool FilterParameter(object item)
    {
        if (item is not CheckableParameterViewModel parameter || string.IsNullOrWhiteSpace(FilterText))
            return true;
        return parameter.DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
            || parameter.Group.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
            || parameter.Id.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
    }

    private void OnOptionChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName != nameof(CheckableParameterViewModel.IsSelected))
            return;
        RaisePropertyChanged(nameof(SelectedParameterIds));
        RaisePropertyChanged(nameof(SelectionSummary));
        if (ParameterOptions.Count(option => option.IsSelected) == 1)
            Title = ParameterOptions.First(option => option.IsSelected).DisplayName;
        ParameterView.Refresh();
        SeriesSelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class UnifiedCalibrationServiceViewModel : BindableBase
{
    private static readonly string[] DefaultMonitoredParameters =
    [
        "system.KvFeedback",
        "system.EmissionCurrent",
        "system.HeaterCurrentFeedback",
    ];

    private readonly ITelemetrySessionCoordinator _coordinator;
    private readonly TelemetryParameterCatalog _catalog;
    private readonly IUcsiHostCommands _hostCommands;
    private readonly UcsiLogBuffer _logBuffer;
    private readonly IGcbCommandInterface _commandInterface;
    private readonly ISystemTelemetryProcessor _telemetryProcessor;
    private readonly IUcsiHvpsUartCommandInterface _hvpsUartInterface;
    private readonly SessionDataExportService _exportService;
    private bool _tickInProgress;
    private bool _updatingTimeline;
    private int _nextGraphNumber = 3;
    private string _parameterFilterText = string.Empty;
    private string _modeText = "LIVE";
    private string _transportText = "Idle";
    private string _connectionText = "Waiting for telemetry";
    private string _systemStateText = "N/A";
    private string _runtimeText = "00:00:00";
    private string _sampleRateText = "0 samples/s";
    private string _recordingCountText = "0 samples";
    private string _errorText = string.Empty;
    private string _gcbFirmwareVersion = "Unknown";
    private string _hvpsFirmwareVersion = "Unknown";
    private string _cncSoftwareVersion = "Unknown";
    private double _timelineSeconds;
    private double _timelineMaximumSeconds;
    private CancellationTokenSource? _seekCancellation;
    private double _hvpsCommandHV;
    private double _hvpsCommandPower;
    private double _hvpsCommandGrid;
    private double _hvpsCommandHeat;
    private float _maLimitValue = 4.0f;
    private bool _emissionOn = false;
    private bool _pidEnabled;
    private bool _coolingWaterPumpEnabled;
    private bool _coolingRadiatorFanEnabled;
    private bool _setpointPollingActive;
    private DateTimeOffset _setpointPollingStartUtc = DateTimeOffset.MinValue;
    private double _expectedKvSetpoint;
    private double _expectedPowerSetpoint;
    private double _expectedGridSetpoint;
    private DateTimeOffset _lastSetpointPollRequestUtc = DateTimeOffset.MinValue;
    private bool _refreshEnabled = true;
    private DateTimeOffset _refreshDisabledUntilUtc = DateTimeOffset.MinValue;
    private bool _configPollingActive;
    private bool _configPollingSuccessful;  // Track if we successfully got a response
    private bool _configPollingGate;  // Gate to prevent overlapping ACFGS requests during polling window
    private DateTimeOffset _configPollingStartUtc = DateTimeOffset.MinValue;
    private DateTimeOffset _lastConfigPollRequestUtc = DateTimeOffset.MinValue;
    private double _coilsCommandXCoil;
    private double _coilsCommandYCoil;
    private double _coilsCommandFocus;
    private bool _hvpsConnected = false;  // Backing field for HvpsConnected property
    private bool _versionInfoFetched = false;  // Track whether we've fetched firmware versions

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        private set => SetProperty(ref _refreshEnabled, value);
    }

    /// <summary>
    /// Disables all config editing (Refresh button + all Set buttons) during polling windows.
    /// This prevents concurrent requests and ensures clean polling cycles.
    /// </summary>
    public bool ConfigEditingEnabled
    {
        get => !_configPollingActive;
    }

    /// <summary>
    /// True when USB UART connection is established and ready for commands.
    /// System Config Refresh and Set buttons are disabled when false.
    /// Updates whenever the service detects a connection state change.
    /// </summary>
    public bool HvpsConnected
    {
        get => _hvpsConnected;
        private set => SetProperty(ref _hvpsConnected, value);
    }

    /// <summary>
    /// True when NO USB UART connection is established (inverse of HvpsConnected).
    /// Used for displaying "No USB UART Connection" message in UI.
    /// </summary>
    public bool HvpsNotConnected
    {
        get => !_hvpsConnected;
    }

    public bool PidEnabled
    {
        get => _pidEnabled;
        set
        {
            if (SetProperty(ref _pidEnabled, value))
            {
                _ = SendHvpsPidControlAsync(value);
            }
        }
    }

    public UnifiedCalibrationServiceViewModel(
        ITelemetrySessionCoordinator coordinator,
        TelemetryParameterCatalog catalog,
        IUcsiHostCommands hostCommands,
        UcsiLogBuffer logBuffer,
        IGcbCommandInterface commandInterface,
        ISystemTelemetryProcessor telemetryProcessor,
        IUcsiHvpsUartCommandInterface hvpsUartInterface,
        SessionDataExportService exportService)
    {
        _coordinator = coordinator;
        _catalog = catalog;
        _hostCommands = hostCommands;
        _logBuffer = logBuffer;
        _commandInterface = commandInterface;
        _telemetryProcessor = telemetryProcessor;
        _hvpsUartInterface = hvpsUartInterface;
        _exportService = exportService;

        ParameterOptions = new ObservableCollection<CheckableParameterViewModel>(
            catalog.All.Select(descriptor => new CheckableParameterViewModel(descriptor)));
        ParameterView = CollectionViewSource.GetDefaultView(ParameterOptions);
        ParameterView.Filter = FilterParameter;
        ParameterView.SortDescriptions.Add(new SortDescription(nameof(CheckableParameterViewModel.IsSelected), ListSortDirection.Descending));
        ParameterView.SortDescriptions.Add(new SortDescription(nameof(CheckableParameterViewModel.DisplayName), ListSortDirection.Ascending));
        foreach (CheckableParameterViewModel option in ParameterOptions)
            option.IsSelected = DefaultMonitoredParameters.Contains(option.Id, StringComparer.Ordinal);

        MonitoredParameters = [];
        ApplyMonitoredSelection();
        Graphs =
        [
            new GraphPaneViewModel(1, catalog, RemoveGraph, "system.KvFeedback"),
            new GraphPaneViewModel(2, catalog, RemoveGraph, "system.EmissionCurrent"),
        ];
        Interlocks = new ObservableCollection<TelemetryStateItemViewModel>(
            Enum.GetValues<SystemInterlock>().Select(value => new TelemetryStateItemViewModel(GetDisplayName(value))));
        HvpsStates = new ObservableCollection<TelemetryStateItemViewModel>(
        [
            new("HV Control Enabled"), new("Grid Control Enabled"), new("Warming"),
            new("Kilovoltage Ramping"), new("Emission On"), new("PID Enabled"),
            new("High Voltage Interlock"), new("High Voltage Status"),
            new("Filament Clock Fault"), new("Cathode Arc"), new("Fan Fault"),
            new("24V Overcurrent Fault"), new("Master Fault"), new("HV Overcurrent Fault"),
            new("Temperature 1 Fault"), new("Cathode Overcurrent Fault"), new("Temperature 3 Fault"),
            new("Temperature 2 Fault"),
        ]);
        InterlockIndicators = new ObservableCollection<TelemetryStateItemViewModel>(
        [
            new("HV Interlock"),
            new("Grid Interlock"),
        ]);
        WarmingIndicators = new ObservableCollection<TelemetryStateItemViewModel>(
        [
            new("Warming"),
            new("HV Ramping"),
        ]);
        ActiveFaults = [];
        Logs = [];

        // Initialize Config Items with proper lambda capture to avoid index mismatch
        ConfigItems = new ObservableCollection<SystemConfigItem>();
        var configItemDefinitions = new (string name, double initial, int fwIndex)[] {
            ("Max Power Limit", 0.0, 0),
            ("Min KV", 0.0, 1),
            ("KV Boundary Threshold", 0.0, 2),
            ("Fast kV Ramp Rate", 0.0, 3),
            ("Slow kV ramp rate", 0.0, 4),
            ("Max KV", 0.0, 5),
            ("Initial Filament", 0.0, 6),
            ("Filament Limit", 0.0, 7),
            ("Grid Proportional Gain 50%", 0.0, 8),
            ("Grid Proportional Gain 70%", 0.0, 9),
            ("Grid Proportional Gain 100%", 0.0, 10),
            ("Grid Integral Time", 0.0, 11),
            ("Grid Slew-Down Rate", 0.0, 12),
            ("Grid Slew-Up Rate", 0.0, 13),
            ("PID Control Parameter", 0.0, 24),
            ("Min Grid", 0.0, 28),
            ("Max Grid", 0.0, 29),
            ("Max Current/mA", 0.0, 30),
            ("Low Filament Threshold", 0.0, 31),
        };
        for (int i = 0; i < configItemDefinitions.Length; i++)
        {
            var def = configItemDefinitions[i];
            int collectionIndex = i;  // Capture the correct collection index
            // Set button is enabled only when NOT polling AND USB connection is active
            var setCommand = new DelegateCommand(
                () => SetConfigValueFromUI(collectionIndex),
                () => ConfigEditingEnabled && HvpsConnected);
            ConfigItems.Add(new SystemConfigItem(
                def.name,
                def.initial,
                def.fwIndex,
                setCommand));
        }

        AddGraphCommand = new DelegateCommand(() => Graphs.Add(new GraphPaneViewModel(_nextGraphNumber++, catalog, RemoveGraph)));
        ApplyMonitoredSelectionCommand = new DelegateCommand(ApplyMonitoredSelection);
        RecordCommand = new DelegateCommand(async () => await ToggleRecordingAsync(), () => CanRecord);
        LoadCommand = new DelegateCommand(async () => await LoadAsync(), () => CanLoad);
        PlayPauseCommand = new DelegateCommand(async () => await RunCommandAsync(() => _coordinator.TogglePlaybackAsync()), () => CanPlayPause);
        ReturnToLiveCommand = new DelegateCommand(async () => await RunCommandAsync(_coordinator.ReturnToLiveAsync), () => IsReplay);
        ClearFaultsCommand = new DelegateCommand(async () => await RunCommandAsync(_hostCommands.ClearFaultsAsync), () => CanClearFaults);
        RefreshSetpointsCommand = new DelegateCommand(RefreshSetpoints, () => RefreshEnabled);
        // Refresh button disabled during polling windows AND when no USB connection
        RefreshSystemConfigCommand = new DelegateCommand(RefreshSystemConfig, () => ConfigEditingEnabled && HvpsConnected);
        ExportSessionDataCommand = new DelegateCommand(async () => await ExportSessionDataAsync(), () => Mode == UcsiMode.Live);
        SaveLogsCommand = new DelegateCommand(async () => await SaveLogsAsync());

        // Subscribe to HVPS connection state changes
        // The service event fires on the message loop thread, so we need to marshal to UI thread
        _hvpsUartInterface.ConnectionStateChanged += (sender, args) =>
        {
            // Use Dispatcher to ensure UI updates happen on the UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                HvpsConnected = args.IsConnected;
                RaisePropertyChanged(nameof(HvpsNotConnected));  // Also notify HvpsNotConnected changed
                
                // Commands that depend on HvpsConnected need to re-evaluate their CanExecute status
                RefreshSystemConfigCommand.RaiseCanExecuteChanged();
                foreach (SystemConfigItem item in ConfigItems)
                    item.SetCommand.RaiseCanExecuteChanged();
            });
        };

        // Initialize HvpsConnected from current state in case the event fired before this subscription was set up
        // This ensures buttons are correctly enabled/disabled even if timing is off during initialization
        HvpsConnected = _hvpsUartInterface.IsConnected;
        if (_hvpsConnected)
        {
            RaisePropertyChanged(nameof(HvpsNotConnected));
            RefreshSystemConfigCommand.RaiseCanExecuteChanged();
            foreach (SystemConfigItem item in ConfigItems)
                item.SetCommand.RaiseCanExecuteChanged();
        }

        // Initialize CNC software version from assembly
        try
        {
            var version = typeof(UnifiedCalibrationServiceViewModel).Assembly.GetName().Version;
            CncSoftwareVersion = version != null ? $"v{version}" : "Unknown";
        }
        catch
        {
            CncSoftwareVersion = "Unknown";
        }
    }

    public ObservableCollection<CheckableParameterViewModel> ParameterOptions { get; }
    public ICollectionView ParameterView { get; }
    public ObservableCollection<MonitoredParameterViewModel> MonitoredParameters { get; }
    public ObservableCollection<GraphPaneViewModel> Graphs { get; }
    public ObservableCollection<TelemetryStateItemViewModel> Interlocks { get; }
    public ObservableCollection<TelemetryStateItemViewModel> HvpsStates { get; }
    public ObservableCollection<TelemetryStateItemViewModel> InterlockIndicators { get; }
    public ObservableCollection<TelemetryStateItemViewModel> WarmingIndicators { get; }
    public ObservableCollection<FaultEntry> ActiveFaults { get; }
    public ObservableCollection<UcsiLogEntry> Logs { get; }
    public ObservableCollection<SystemConfigItem> ConfigItems { get; }

    public DelegateCommand AddGraphCommand { get; }
    public DelegateCommand ApplyMonitoredSelectionCommand { get; }
    public DelegateCommand RecordCommand { get; }
    public DelegateCommand LoadCommand { get; }
    public DelegateCommand PlayPauseCommand { get; }
    public DelegateCommand ReturnToLiveCommand { get; }
    public DelegateCommand ClearFaultsCommand { get; }
    public DelegateCommand RefreshSetpointsCommand { get; }
    public DelegateCommand RefreshSystemConfigCommand { get; }
    public DelegateCommand ExportSessionDataCommand { get; }
    public DelegateCommand SaveLogsCommand { get; }

    public ITelemetrySessionCoordinator Coordinator => _coordinator;
    public UcsiTelemetrySample? CurrentSample => _coordinator.CurrentSample;
    public UcsiMode Mode => _coordinator.Mode;

    public string ParameterFilterText
    {
        get => _parameterFilterText;
        set
        {
            if (SetProperty(ref _parameterFilterText, value))
                ParameterView.Refresh();
        }
    }
    public string ModeText { get => _modeText; private set => SetProperty(ref _modeText, value); }
    public string TransportText { get => _transportText; private set => SetProperty(ref _transportText, value); }
    public string ConnectionText { get => _connectionText; private set => SetProperty(ref _connectionText, value); }
    public string SystemStateText { get => _systemStateText; private set => SetProperty(ref _systemStateText, value); }
    public string RuntimeText { get => _runtimeText; private set => SetProperty(ref _runtimeText, value); }
    public string SampleRateText { get => _sampleRateText; private set => SetProperty(ref _sampleRateText, value); }
    public string RecordingCountText { get => _recordingCountText; private set => SetProperty(ref _recordingCountText, value); }
    public string ErrorText { get => _errorText; private set => SetProperty(ref _errorText, value); }
    public string GcbFirmwareVersion { get => _gcbFirmwareVersion; private set => SetProperty(ref _gcbFirmwareVersion, value); }
    public string HvpsFirmwareVersion { get => _hvpsFirmwareVersion; private set => SetProperty(ref _hvpsFirmwareVersion, value); }
    public string CncSoftwareVersion { get => _cncSoftwareVersion; private set => SetProperty(ref _cncSoftwareVersion, value); }
    public string RecordButtonText => _coordinator.TransportState == SessionTransportState.Recording ? "Stop" : "Record";
    public string PlayPauseButtonText => _coordinator.TransportState == SessionTransportState.Playing ? "Pause" : "Play";
    public bool IsReplay => _coordinator.Mode == UcsiMode.Replay;
    public bool CanRecord => !IsReplay && _coordinator.TransportState is SessionTransportState.Idle or SessionTransportState.Recording;
    public bool CanLoad => !IsReplay && _coordinator.TransportState == SessionTransportState.Idle;
    public bool CanPlayPause => IsReplay && _coordinator.TransportState is SessionTransportState.Paused or SessionTransportState.Playing;
    public bool CanClearFaults => !IsReplay && _coordinator.TransportState != SessionTransportState.Recording && _hostCommands.CanClearFaults;
    public string ClearFaultsToolTip => _hostCommands.CanClearFaults
        ? "Clear active system faults"
        : _hostCommands.ClearFaultsUnavailableReason;

    public double TimelineSeconds
    {
        get => _timelineSeconds;
        set
        {
            if (!SetProperty(ref _timelineSeconds, value) || _updatingTimeline || !IsReplay)
                return;
            QueueSeek(value);
        }
    }
    public double TimelineMaximumSeconds
    {
        get => _timelineMaximumSeconds;
        private set => SetProperty(ref _timelineMaximumSeconds, value);
    }
    public string TimelineText => $"{TimeSpan.FromSeconds(TimelineSeconds):hh\\:mm\\:ss\\.fff} / {TimeSpan.FromSeconds(TimelineMaximumSeconds):hh\\:mm\\:ss\\.fff}";

    public double HvpsCommandHV
    {
        get => _hvpsCommandHV;
        set
        {
            // Clamp HV to [0, 100] - static range
            double clampedValue = Math.Max(0, Math.Min(100, value));
            if (SetProperty(ref _hvpsCommandHV, clampedValue))
            {
                // When HV changes, e- changes too (e- = Power / HV)
                RaisePropertyChanged(nameof(HvpsCommandEmission));
                RaisePropertyChanged(nameof(IsEmissionValid));
                RaisePropertyChanged(nameof(EmissionTextBoxBorder));
            }
        }
    }

    // e- is derived: e- = Power / HV (should stay <= 4.0 mA)
    public double HvpsCommandEmission => _hvpsCommandHV > 0 ? _hvpsCommandPower / _hvpsCommandHV : 0;

    // Emission validation (max 4 mA) - used for UI display only, not for enabling button
    public bool IsEmissionValid => HvpsCommandEmission <= 4.0;

    // Emission button background - grey when off, green when ON (matches interlock indicator color)
    public SolidColorBrush EmissionButtonBrush =>
        _emissionOn
            ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 46, 143, 68)) // Green (same as interlock indicators)
            : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 43, 43, 43)); // Dark grey #292b2b

    // Emission button is always clickable
    public bool CanClickEmission => true;

    // TextBox border brush - red if invalid, transparent if valid
    public SolidColorBrush EmissionTextBoxBorder =>
        IsEmissionValid 
            ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 255, 255, 255))
            : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));

    // HV slider opacity - dim when disabled, bright when enabled
    public double HvCommandSliderOpacity => IsHvEnabled ? 1.0 : 0.5;

    // HV is only editable when Power > 0 AND all 3 interlocks are enabled
    public bool IsHvEnabled => 
        _hvpsCommandPower > 0 && 
        InterlockIndicators[0].IsActive &&  // HV Interlock
        InterlockIndicators[1].IsActive &&  // Grid Interlock
        (CurrentSample?.Telemetry.Hvps.GridInterlock ?? false);  // Grid Watchdog

    public double HvpsCommandPower
    {
        get => _hvpsCommandPower;
        set
        {
            // Clamp Power to 0-400 W
            double clampedValue = Math.Max(0, Math.Min(400, value));
            if (SetProperty(ref _hvpsCommandPower, clampedValue))
            {
                // Update dependent properties
                RaisePropertyChanged(nameof(IsHvEnabled));
                RaisePropertyChanged(nameof(HvCommandSliderOpacity));
                RaisePropertyChanged(nameof(HvpsCommandEmission));
                RaisePropertyChanged(nameof(IsEmissionValid));
                RaisePropertyChanged(nameof(EmissionTextBoxBorder));
                
                // When Power is 0, reset HV to 0 and send command
                if (clampedValue == 0 && _hvpsCommandHV > 0)
                {
                    HvpsCommandHV = 0;
                    _ = SendHvpsKvToBoard();
                }
                // When Power > 0 and HV > 0, send the updated command to firmware
                else if (clampedValue > 0 && _hvpsCommandHV > 0)
                {
                    _ = SendHvpsKvToBoard();
                }
                // When Power is set to 0 and HV is already 0, send command to ensure both are 0
                else if (clampedValue == 0 && _hvpsCommandHV == 0)
                {
                    _ = SendHvpsKvToBoard();
                }
            }
        }
    }

    public double HvpsCommandGrid
    {
        get => _hvpsCommandGrid;
        set
        {
            // Clamp Grid to 0-600 V
            double clampedValue = Math.Max(0, Math.Min(600, value));
            SetProperty(ref _hvpsCommandGrid, clampedValue);
        }
    }

    public double HvpsCommandHeat
    {
        get => _hvpsCommandHeat;
        set
        {
            // Clamp Heat to 0-4000 mA
            double clampedValue = Math.Max(0, Math.Min(4000, value));
            SetProperty(ref _hvpsCommandHeat, clampedValue);
        }
    }

    public double HvpsSetpointKV => CurrentSample?.Telemetry.KvSetpoint ?? 0.0;
    public double HvpsSetpointEmission => HvpsSetpointPower > 0 && HvpsSetpointKV > 0 ? HvpsSetpointPower / HvpsSetpointKV : 0.0;
    public double HvpsSetpointPower => CurrentSample?.Telemetry.HvpsPowerSetpoint ?? 0.0;
    public double HvpsSetpointGrid => CurrentSample?.Telemetry.GridSetpoint ?? 0.0;
    public double HvpsSetpointHeat => CurrentSample?.Telemetry.HeaterCurrentSetpoint ?? 0.0;

    public double HvpsFeedbackKV => CurrentSample?.Telemetry.KvFeedback ?? 0.0;
    public double HvpsFeedbackEmission => CurrentSample?.Telemetry.EmissionCurrent ?? 0.0;

    public double CoilsCommandXCoil
    {
        get => _coilsCommandXCoil;
        set
        {
            // Clamp X Coil to [-1.5, 1.5] A
            double clampedValue = Math.Max(-1.5, Math.Min(1.5, value));
            SetProperty(ref _coilsCommandXCoil, clampedValue);
        }
    }

    public double CoilsCommandYCoil
    {
        get => _coilsCommandYCoil;
        set
        {
            // Clamp Y Coil to [-1.5, 1.5] A
            double clampedValue = Math.Max(-1.5, Math.Min(1.5, value));
            SetProperty(ref _coilsCommandYCoil, clampedValue);
        }
    }

    public double CoilsCommandFocus
    {
        get => _coilsCommandFocus;
        set
        {
            // Clamp Focus to [0.0, 3.0] A
            double clampedValue = Math.Max(0.0, Math.Min(3.0, value));
            SetProperty(ref _coilsCommandFocus, clampedValue);
        }
    }

    public double CoilsFeedbackXCoil => CurrentSample?.Telemetry.XCoilCurrent ?? 0.0;
    public double CoilsFeedbackYCoil => CurrentSample?.Telemetry.YCoilCurrent ?? 0.0;
    public double CoilsFeedbackFocus => CurrentSample?.Telemetry.FocusCurrent ?? 0.0;
    public double HvpsFeedbackPower => (CurrentSample?.Telemetry.KvFeedback ?? 0.0) * (CurrentSample?.Telemetry.EmissionCurrent ?? 0.0);
    public double HvpsFeedbackGrid => CurrentSample?.Telemetry.GridVoltage ?? 0.0;
    public double HvpsFeedbackHeat => CurrentSample?.Telemetry.HeaterCurrentFeedback ?? 0.0;

    // Indicator light colors - Interlocks (using data binding pattern from working implementation)
    public SolidColorBrush InterlockHvColor => GetInterlockColor(InterlockIndicators[0]);
    public SolidColorBrush InterlockGridColor => GetInterlockColor(InterlockIndicators[1]);
    // Grid Watchdog uses HVPS-level GridInterlock (bit 2 of RawIoFlags), not system-level WatchdogReady
    public SolidColorBrush InterlockGridWatchdogColor => GetSimpleStateColor(CurrentSample?.Telemetry.Hvps.GridInterlock ?? false);

    private SolidColorBrush GetInterlockColor(TelemetryStateItemViewModel indicator)
    {
        if (!indicator.IsAvailable)
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 80, 80, 80)); // Grey when unavailable
        return indicator.IsActive
            ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 200, 0)) // Green when active
            : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 80, 80, 80)); // Grey when off
    }

    private SolidColorBrush GetSimpleStateColor(bool isActive)
    {
        return isActive
            ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 200, 0)) // Green when active
            : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 80, 80, 80)); // Grey when off
    }

    // Indicator light colors - Warming states (using data binding pattern from working implementation)
    public SolidColorBrush WarmingIndicatorColor => GetWarmingColor(WarmingIndicators[0]);
    public SolidColorBrush HvRampingIndicatorColor => GetWarmingColor(WarmingIndicators[1]);

    private SolidColorBrush GetWarmingColor(TelemetryStateItemViewModel indicator)
    {
        return indicator.IsActive
            ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 200, 0)) // Amber when active
            : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 80, 80, 80)); // Grey when off
    }

    public bool CoolingWaterPumpEnabled
    {
        get => _coolingWaterPumpEnabled;
        set => SetProperty(ref _coolingWaterPumpEnabled, value);
    }

    public bool CoolingRadiatorFanEnabled
    {
        get => _coolingRadiatorFanEnabled;
        set => SetProperty(ref _coolingRadiatorFanEnabled, value);
    }

    public string CoolingWaterPumpText => _coolingWaterPumpEnabled ? "Water Pump: On" : "Water Pump: Off";
    public string CoolingRadiatorFanText => _coolingRadiatorFanEnabled ? "Radiator Fan: On" : "Radiator Fan: Off";

    private void StartSetpointPollingWindow(double expectedKv, double expectedPower, double expectedGrid)
    {
        _expectedKvSetpoint = expectedKv;
        _expectedPowerSetpoint = expectedPower;
        _expectedGridSetpoint = expectedGrid;
        _setpointPollingStartUtc = DateTimeOffset.UtcNow;
        _lastSetpointPollRequestUtc = DateTimeOffset.MinValue;  // Force immediate poll
        _setpointPollingActive = true;
        
        _logBuffer.Log(
            $"Starting setpoint polling window: expecting KV={expectedKv:F1}kV, Power={expectedPower:F1}W, Grid={expectedGrid:F1}V",
            LogRecordSeverity.Info,
            LogRecordType.System);
    }

    private void RefreshSetpoints()
    {
        RefreshEnabled = false;
        _refreshDisabledUntilUtc = DateTimeOffset.UtcNow.AddSeconds(2);
        _telemetryProcessor.RequestSetpointPollingNow();
        _logBuffer.Log(
            "Manual setpoint refresh requested",
            LogRecordSeverity.Info,
            LogRecordType.System);
    }

    private void CheckSetpointPollingProgress()
    {
        double elapsedMs = (DateTimeOffset.UtcNow - _setpointPollingStartUtc).TotalMilliseconds;
        
        // Check if all 3 setpoints match expected values
        bool kvMatch = HvpsSetpointKV == _expectedKvSetpoint;
        bool powerMatch = HvpsSetpointPower == _expectedPowerSetpoint;
        bool gridMatch = HvpsSetpointGrid == _expectedGridSetpoint;
        bool allMatch = kvMatch && powerMatch && gridMatch;

        if (allMatch)
        {
            _setpointPollingActive = false;
            _logBuffer.Log(
                $"HVPS setpoints updated successfully after {elapsedMs:F0}ms: KV={HvpsSetpointKV:F1}kV, Power={HvpsSetpointPower:F1}W, Grid={HvpsSetpointGrid:F1}V",
                LogRecordSeverity.Info,
                LogRecordType.System);
            return;
        }

        // Debug: Log current state vs expected (first log only, then every 500ms)
        if (elapsedMs < 50 || (int)elapsedMs % 500 < 33)
        {
            _logBuffer.Log(
                $"[Polling {elapsedMs:F0}ms] Expected: KV={_expectedKvSetpoint:F1}, Power={_expectedPowerSetpoint:F1}, Grid={_expectedGridSetpoint:F1} | Received: KV={HvpsSetpointKV:F1}, Power={HvpsSetpointPower:F1}, Grid={HvpsSetpointGrid:F1} | Match: KV={kvMatch}, Power={powerMatch}, Grid={gridMatch}",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }

        // Check timeout (2 seconds)
        if (elapsedMs > 2000)
        {
            _setpointPollingActive = false;
            _logBuffer.Log(
                $"HVPS setpoints failed to update after 2 seconds. Final values: KV={HvpsSetpointKV:F1}kV (expected {_expectedKvSetpoint:F1}), Power={HvpsSetpointPower:F1}W (expected {_expectedPowerSetpoint:F1}), Grid={HvpsSetpointGrid:F1}V (expected {_expectedGridSetpoint:F1}). Verify connection and try again.",
                LogRecordSeverity.Error,
                LogRecordType.System);
            return;
        }

        // Request poll every 250ms via the processor (which will handle updating the state)
        if ((DateTimeOffset.UtcNow - _lastSetpointPollRequestUtc).TotalMilliseconds >= 250)
        {
            _telemetryProcessor.RequestSetpointPollingNow();
            _lastSetpointPollRequestUtc = DateTimeOffset.UtcNow;
        }
    }



    private void RefreshSystemConfig()
    {
        _configPollingActive = true;
        _configPollingSuccessful = false;  // Reset success flag
        _configPollingStartUtc = DateTimeOffset.UtcNow;
        _lastConfigPollRequestUtc = DateTimeOffset.MinValue;  // Force immediate poll
        RaisePropertyChanged(nameof(ConfigEditingEnabled));
        RefreshSystemConfigCommand.RaiseCanExecuteChanged();
        // Disable all Set buttons while polling is active
        foreach (SystemConfigItem item in ConfigItems)
            item.SetCommand.RaiseCanExecuteChanged();
        
        _logBuffer.Log(
            "Starting system config polling window",
            LogRecordSeverity.Info,
            LogRecordType.System);
    }

    private async Task SetConfigValue(int collectionIndex, float value)
    {
        try
        {
            // Get the firmware index from the ConfigItem
            int firmwareIndex = ConfigItems[collectionIndex].FirmwareIndex;
            string itemName = ConfigItems[collectionIndex].Name;
            
            // Start polling window BEFORE sending the command
            // This disables all Set and Refresh buttons immediately, preventing rapid clicks
            _configPollingActive = true;
            _configPollingSuccessful = false;
            _configPollingStartUtc = DateTimeOffset.UtcNow;
            _lastConfigPollRequestUtc = DateTimeOffset.MinValue;
            RaisePropertyChanged(nameof(ConfigEditingEnabled));
            RefreshSystemConfigCommand.RaiseCanExecuteChanged();
            // Disable all Set buttons while polling is active
            foreach (SystemConfigItem item in ConfigItems)
                item.SetCommand.RaiseCanExecuteChanged();
            
            await _hvpsUartInterface.SetSystemConfigValue(firmwareIndex, value);
            _logBuffer.Log(
                $"System config value set: {itemName} (firmware index {firmwareIndex}) = {value}",
                LogRecordSeverity.Info,
                LogRecordType.System);
            
            // Trigger refresh after set - wait 500ms to allow firmware to stabilize after CONFIG_SET
            // (firmware UART may be busy processing the command)
            _ = Task.Delay(500).ContinueWith(_ =>
            {
                if (_configPollingActive) // Only refresh if polling window still active
                {
                    _lastConfigPollRequestUtc = DateTimeOffset.MinValue; // Force immediate poll
                    RequestSystemConfigAsync().ConfigureAwait(false);
                }
            });
        }
        catch (Exception ex)
        {
            _logBuffer.Log(
                $"Failed to set system config value at collection index {collectionIndex}: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
            // End polling window on error so buttons re-enable
            _configPollingActive = false;
            RaisePropertyChanged(nameof(ConfigEditingEnabled));
            RefreshSystemConfigCommand.RaiseCanExecuteChanged();
            foreach (SystemConfigItem item in ConfigItems)
                item.SetCommand.RaiseCanExecuteChanged();
        }
    }

    private void CheckConfigPollingProgress()
    {
        if (!_configPollingActive)
            return;

        double elapsedMs = (DateTimeOffset.UtcNow - _configPollingStartUtc).TotalMilliseconds;

        // Exit early if we got a successful response
        if (_configPollingSuccessful)
        {
            _configPollingActive = false;
            RaisePropertyChanged(nameof(ConfigEditingEnabled));
            RefreshSystemConfigCommand.RaiseCanExecuteChanged();
            // Notify all Set buttons that they can execute again
            foreach (SystemConfigItem item in ConfigItems)
                item.SetCommand.RaiseCanExecuteChanged();
            _logBuffer.Log(
                $"System config refresh completed successfully after {elapsedMs:F0}ms",
                LogRecordSeverity.Info,
                LogRecordType.System);
            return;
        }

        // Check timeout (2 seconds)
        if (elapsedMs > 2000)
        {
            _configPollingActive = false;
            RaisePropertyChanged(nameof(ConfigEditingEnabled));
            RefreshSystemConfigCommand.RaiseCanExecuteChanged();
            // Notify all Set buttons that they can execute again
            foreach (SystemConfigItem item in ConfigItems)
                item.SetCommand.RaiseCanExecuteChanged();
            _logBuffer.Log(
                $"System config refresh completed after {elapsedMs:F0}ms",
                LogRecordSeverity.Info,
                LogRecordType.System);
            return;
        }

        // Request config every 250ms
        if ((DateTimeOffset.UtcNow - _lastConfigPollRequestUtc).TotalMilliseconds >= 250)
        {
            _ = RequestSystemConfigAsync();
            _lastConfigPollRequestUtc = DateTimeOffset.UtcNow;
        }
    }

    private async Task RequestSystemConfigAsync()
    {
        // Gate: Prevent overlapping ACFGS requests (only allow one concurrent request during polling window)
        if (_configPollingGate)
        {
            _logBuffer.Log(
                "Config poll already in progress, skipping overlapping request",
                LogRecordSeverity.Info,
                LogRecordType.System);
            return;
        }

        _configPollingGate = true;
        try
        {
            var response = await _hvpsUartInterface.RequestSystemConfig();
            
            // Update ConfigItems with received values using each item's firmware index
            for (int i = 0; i < ConfigItems.Count; i++)
            {
                int firmwareIndex = ConfigItems[i].FirmwareIndex;
                if (firmwareIndex >= 0 && firmwareIndex < response.Values.Length)
                {
                    ConfigItems[i].CurrentValue = response.Values[firmwareIndex];
                }
            }
            
            _logBuffer.Log(
                $"System config updated: {ConfigItems.Count} values received",
                LogRecordSeverity.Info,
                LogRecordType.System);
            
            // Mark polling as successful so CheckConfigPollingProgress can exit early
            _configPollingSuccessful = true;
        }
        catch (Exception ex)
        {
            _logBuffer.Log(
                $"Failed to request system config: {ex.Message}",
                LogRecordSeverity.Warn,
                LogRecordType.System);
        }
        finally
        {
            // Release the gate to allow the next polling request
            _configPollingGate = false;
        }
    }

    private void SetConfigValueFromUI(int index)
    {
        SystemConfigItem item = ConfigItems[index];
        
        if (string.IsNullOrWhiteSpace(item.InputValue))
        {
            _logBuffer.Log(
                $"Config item '{item.Name}' input is empty",
                LogRecordSeverity.Warn,
                LogRecordType.System);
            return;
        }

        if (!float.TryParse(item.InputValue, out float value))
        {
            _logBuffer.Log(
                $"Config item '{item.Name}' input '{item.InputValue}' is not a valid float",
                LogRecordSeverity.Error,
                LogRecordType.System);
            return;
        }

        _ = SetConfigValue(index, value);
    }

    public SolidColorBrush CoolingWaterPumpColor => _coolingWaterPumpEnabled
        ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 45, 92, 111))   // Blue
        : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 80, 80, 80));   // Grey

    public SolidColorBrush CoolingRadiatorFanColor => _coolingRadiatorFanEnabled
        ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 45, 92, 111))   // Blue
        : new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 80, 80, 80));   // Grey

    public async Task TickAsync()
    {
        if (_tickInProgress)
            return;
        _tickInProgress = true;
        try
        {
            await _coordinator.AdvancePresentationAsync();
            UcsiTelemetrySample? sample = _coordinator.CurrentSample;
            ModeText = _coordinator.Mode == UcsiMode.Live ? "LIVE" : "REPLAY";
            TransportText = _coordinator.TransportState.ToString();
            ConnectionText = sample is null
                ? "Waiting for telemetry"
                : _coordinator.Mode == UcsiMode.Replay || DateTimeOffset.UtcNow - sample.Value.ReceivedAtUtc <= TimeSpan.FromMilliseconds(1_500)
                    ? "Connected"
                    : "Communication unavailable";
            SystemStateText = sample?.Telemetry.ControlBoardState.ToString() ?? "N/A";
            RuntimeText = sample is null ? "00:00:00" : TimeSpan.FromMilliseconds(sample.Value.Telemetry.SystemRuntime).ToString("hh\\:mm\\:ss");
            SampleRateText = $"{_coordinator.LiveSampleRate:F1} samples/s";
            RecordingCountText = $"{_coordinator.AcceptedRecordingSamples:N0} accepted / {_coordinator.WrittenRecordingSamples:N0} written";
            ErrorText = _coordinator.LastError ?? string.Empty;
            foreach (MonitoredParameterViewModel parameter in MonitoredParameters)
                parameter.Update(sample);
            UpdateDetailedStatus(sample);
            if (_setpointPollingActive)
                CheckSetpointPollingProgress();
            if (_configPollingActive)
                CheckConfigPollingProgress();
            if (!RefreshEnabled && DateTimeOffset.UtcNow >= _refreshDisabledUntilUtc)
            {
                RefreshEnabled = true;
                RefreshSetpointsCommand.RaiseCanExecuteChanged();
                _logBuffer.Log(
                    $"Setpoint refresh completed. Values: KV={HvpsSetpointKV:F1}kV, Power={HvpsSetpointPower:F1}W, Grid={HvpsSetpointGrid:F1}V",
                    LogRecordSeverity.Info,
                    LogRecordType.System);
            }
            UpdateLogs();

            _updatingTimeline = true;
            TimelineMaximumSeconds = TimeSpan.FromTicks(_coordinator.TotalElapsedTicks).TotalSeconds;
            TimelineSeconds = TimeSpan.FromTicks(_coordinator.CurrentElapsedTicks).TotalSeconds;
            _updatingTimeline = false;
            RaisePropertyChanged(nameof(TimelineText));
            
            // Fetch firmware versions if we haven't already and are connected
            if (!_versionInfoFetched && sample != null)
            {
                _ = FetchVersionInfoAsync();
            }

            RaiseStateProperties();
        }
        finally
        {
            _tickInProgress = false;
        }
    }

    /// <summary>
    /// Fetch firmware version information from GCB and HVPS.
    /// Called once during initialization when telemetry is available.
    /// </summary>
    private async Task FetchVersionInfoAsync()
    {
        if (_versionInfoFetched)
            return;

        _versionInfoFetched = true;

        try
        {
            var versionInfo = await _commandInterface.GetVersionInfo();
            GcbFirmwareVersion = string.IsNullOrEmpty(versionInfo.FirmwareVersion) 
                ? "Unknown" 
                : versionInfo.FirmwareVersion;
            HvpsFirmwareVersion = string.IsNullOrEmpty(versionInfo.HvpsFirmwareVersion) 
                ? "Unknown" 
                : versionInfo.HvpsFirmwareVersion;
            
            _logBuffer.Log(
                $"Firmware versions retrieved - GCB: {GcbFirmwareVersion}, HVPS: {HvpsFirmwareVersion}",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            _logBuffer.Log(
                $"Failed to retrieve firmware versions: {ex.Message}",
                LogRecordSeverity.Warn,
                LogRecordType.System);
        }
    }

    private async Task ToggleRecordingAsync()
    {
        if (_coordinator.TransportState == SessionTransportState.Recording)
        {
            await RunCommandAsync(_coordinator.StopRecordingAsync);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "UCSI telemetry session (*.parquet)|*.parquet",
            AddExtension = true,
            DefaultExt = ".parquet",
            OverwritePrompt = true,
            FileName = $"Ucsi-{DateTime.Now:yyyyMMdd-HHmmss}.parquet",
        };
        if (dialog.ShowDialog() != true)
            return;
        await RunCommandAsync(() =>
        {
            _coordinator.StartRecording(dialog.FileName);
            return Task.CompletedTask;
        });
    }

    private async Task LoadAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "UCSI telemetry session (*.parquet)|*.parquet",
            CheckFileExists = true,
            Multiselect = false,
        };
        if (dialog.ShowDialog() != true)
            return;
        await RunCommandAsync(() => _coordinator.LoadReplayAsync(dialog.FileName));
    }

    private async Task ExportSessionDataAsync()
    {
        await RunCommandAsync(async () =>
        {
            // Get snapshot of live telemetry data
            IReadOnlyList<UcsiTelemetrySample> samples = _coordinator.LiveHistory.Snapshot();
            if (samples.Count == 0)
            {
                ErrorText = "No telemetry data available to export.";
                return;
            }

            // Export to CSV in application directory
            string outputPath = _exportService.ExportToCsv(samples);

            // Log success and display path
            _logBuffer.Log(
                $"Session data exported successfully: {Path.GetFileName(outputPath)} ({samples.Count} samples)",
                LogRecordSeverity.Info,
                LogRecordType.System);
            
            // Show confirmation in UI
            ErrorText = $"Exported {samples.Count} samples to {Path.GetFileName(outputPath)}";
        });
    }

    private async Task SaveLogsAsync()
    {
        await RunCommandAsync(async () =>
        {
            // Get all current logs
            IReadOnlyList<UcsiLogEntry> allLogs = _logBuffer.Snapshot();
            if (allLogs.Count == 0)
            {
                ErrorText = "No logs to save.";
                return;
            }

            // Generate timestamped filename
            string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
            string filename = $"log-output-{timestamp}.txt";
            string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export");
            Directory.CreateDirectory(exportDir);
            string outputPath = Path.Combine(exportDir, filename);

            // Write logs to text file
            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                writer.WriteLine(new string('=', 120));
                writer.WriteLine($"UCSI Log Export - {DateTimeOffset.UtcNow:O}");
                writer.WriteLine(new string('=', 120));
                writer.WriteLine();

                foreach (UcsiLogEntry entry in allLogs)
                {
                    writer.WriteLine($"[{entry.Timestamp:HH:mm:ss.fff}] {entry.Severity,-8} {entry.Type,-10} | {entry.Message}");
                }

                writer.WriteLine();
                writer.WriteLine(new string('=', 120));
                writer.WriteLine($"Total entries: {allLogs.Count}");
            }

            // Log success
            _logBuffer.Log(
                $"Logs saved successfully: {Path.GetFileName(outputPath)} ({allLogs.Count} entries)",
                LogRecordSeverity.Info,
                LogRecordType.System);

            // Show confirmation in UI
            ErrorText = $"Logs saved to {Path.GetFileName(outputPath)}";
        });
    }

    private async Task RunCommandAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            ErrorText = exception.Message;
            _logBuffer.Log(exception.Message, LogRecordSeverity.Error, LogRecordType.System);
        }
        finally
        {
            RaiseStateProperties();
        }
    }

    private void QueueSeek(double seconds)
    {
        _seekCancellation?.Cancel();
        _seekCancellation?.Dispose();
        _seekCancellation = new CancellationTokenSource();
        CancellationToken token = _seekCancellation.Token;
        _ = RunCommandAsync(async () =>
        {
            await Task.Delay(75, token);
            await _coordinator.SeekAsync(TimeSpan.FromSeconds(seconds).Ticks, token);
        });
    }

    private void ApplyMonitoredSelection()
    {
        ParameterView.Refresh();
        MonitoredParameters.Clear();
        foreach (CheckableParameterViewModel option in ParameterOptions.Where(option => option.IsSelected))
            MonitoredParameters.Add(new MonitoredParameterViewModel(option.Descriptor));
    }

    public void AddToMonitoredParameters(string parameterId)
    {
        // Check if already monitored
        if (MonitoredParameters.Any(p => p.Descriptor.Id == parameterId))
            return; // Already added, idempotent

        // Find the descriptor
        var option = ParameterOptions.FirstOrDefault(o => o.Id == parameterId);
        if (option != null)
        {
            // Add to monitored list
            MonitoredParameters.Add(new MonitoredParameterViewModel(option.Descriptor));
            // Mark as selected for consistency
            option.IsSelected = true;
            ParameterView.Refresh();
        }
    }

    private void RemoveGraph(GraphPaneViewModel graph) => Graphs.Remove(graph);

    private bool FilterParameter(object item)
    {
        if (item is not CheckableParameterViewModel parameter || string.IsNullOrWhiteSpace(ParameterFilterText))
            return true;
        return parameter.DisplayName.Contains(ParameterFilterText, StringComparison.OrdinalIgnoreCase)
            || parameter.Group.Contains(ParameterFilterText, StringComparison.OrdinalIgnoreCase)
            || parameter.Id.Contains(ParameterFilterText, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateDetailedStatus(UcsiTelemetrySample? sample)
    {
        ISystemTelemetry? telemetry = sample?.Telemetry;
        SystemInterlock[] values = Enum.GetValues<SystemInterlock>();
        for (int index = 0; index < values.Length; index++)
        {
            bool? state;
            if (values[index] == SystemInterlock.WatchdogReady)
            {
                // Watchdog ready tracks HVPS-level GridInterlock (bit 2 of RawIoFlags), not system-level WatchdogReady
                state = telemetry?.Hvps.GridInterlock;
            }
            else
            {
                state = telemetry?.Interlocks.GetState(values[index]);
            }
            Interlocks[index].Value = state.HasValue ? state.Value ? "Ready" : "Open" : "N/A";
            Interlocks[index].IsActive = state == true;
            Interlocks[index].IsAvailable = state.HasValue;
        }

        bool?[] hvps = telemetry is null
            ? new bool?[HvpsStates.Count]
            :
            [
                telemetry.Hvps.HighVoltageControlEnabled,
                // Grid Control Enabled = true only when BOTH Grid Interlock (bit 8) AND Grid Watchdog (bit 2) are enabled
                telemetry.Hvps.CalibrationGridInterlockEnabled == true && telemetry.Hvps.GridInterlock == true ? true : false,
                telemetry.Hvps.Warming,
                telemetry.Hvps.KilovoltageRamping,
                telemetry.Hvps.EmissionOn,
                telemetry.Hvps.PidEnabled,
                telemetry.Hvps.HighVoltageInterlock,
                telemetry.Hvps.HighVoltageStatus,
                telemetry.Hvps.FilamentClockFault,
                telemetry.Hvps.CathodeArc,
                telemetry.Hvps.FanFault,
                telemetry.Hvps.Overcurrent24VoltFault,
                telemetry.Hvps.MasterFault,
                telemetry.Hvps.HighVoltageOvercurrentFault,
                telemetry.Hvps.Temperature1Fault,
                telemetry.Hvps.CathodeOvercurrentFault,
                telemetry.Hvps.Temperature3Fault,
                telemetry.Hvps.Temperature2Fault,
            ];
        for (int index = 0; index < HvpsStates.Count; index++)
        {
            HvpsStates[index].Value = hvps[index].HasValue ? hvps[index]!.Value ? "On" : "Off" : "N/A";
            HvpsStates[index].IsActive = hvps[index] == true;
            HvpsStates[index].IsAvailable = hvps[index].HasValue;
        }

        // Sync PID Enabled state from telemetry (HvpsStates[5])
        _pidEnabled = hvps[5].GetValueOrDefault(false);

        // Sync Emission On state from telemetry (HvpsStates[4])
        bool emissionFromTelemetry = hvps[4].GetValueOrDefault(false);
        if (_emissionOn != emissionFromTelemetry)
        {
            _emissionOn = emissionFromTelemetry;
            RaisePropertyChanged(nameof(EmissionButtonBrush));
        }

        // Update interlock indicators for UI display
        // Note: GridInterlock (bit 2 of RawIoFlags) is actually a clock/status bit tracking Grid Watchdog
        // The actual Grid Interlock is CalibrationGridInterlockEnabled (bit 8 of RawStatusFlags)
        bool?[] interlockIndicators = telemetry is null
            ? new bool?[2]
            :
            [
                telemetry.Hvps.HighVoltageInterlock,
                telemetry.Hvps.CalibrationGridInterlockEnabled,
            ];
        for (int index = 0; index < InterlockIndicators.Count; index++)
        {
            InterlockIndicators[index].Value = interlockIndicators[index].HasValue ? interlockIndicators[index]!.Value ? "On" : "Off" : "N/A";
            InterlockIndicators[index].IsActive = interlockIndicators[index] == true;
            InterlockIndicators[index].IsAvailable = interlockIndicators[index].HasValue;
        }
        // Raise PropertyChanged for color properties that depend on InterlockIndicators
        RaisePropertyChanged(nameof(InterlockHvColor));
        RaisePropertyChanged(nameof(InterlockGridColor));
        // Interlocks are updated from telemetry, which affects IsHvEnabled
        RaisePropertyChanged(nameof(IsHvEnabled));
        RaisePropertyChanged(nameof(HvCommandSliderOpacity));

        // Update warming indicators from HVPS States (index 2 = Warming, index 3 = Kilovoltage Ramping)
        if (HvpsStates.Count >= 4)
        {
            WarmingIndicators[0].IsActive = HvpsStates[2].IsActive;
            WarmingIndicators[0].IsAvailable = HvpsStates[2].IsAvailable;
            WarmingIndicators[0].Value = HvpsStates[2].Value;
            WarmingIndicators[1].IsActive = HvpsStates[3].IsActive;
            WarmingIndicators[1].IsAvailable = HvpsStates[3].IsAvailable;
            WarmingIndicators[1].Value = HvpsStates[3].Value;
        }
        // Raise PropertyChanged for color properties that depend on WarmingIndicators
        RaisePropertyChanged(nameof(WarmingIndicatorColor));
        RaisePropertyChanged(nameof(HvRampingIndicatorColor));
        // Note: Grid Watchdog uses HVPS-level GridInterlock (bit 2 of RawIoFlags), not system-level WatchdogReady
        // When Grid Watchdog state changes, we need to notify the color property
        RaisePropertyChanged(nameof(InterlockGridWatchdogColor));

        ActiveFaults.Clear();
        if (sample is not null)
        {
            foreach (FaultEntry fault in sample.Value.ActiveFaults)
                ActiveFaults.Add(fault);
        }
    }

    private void UpdateLogs()
    {
        IReadOnlyList<UcsiLogEntry> entries = _logBuffer.Snapshot();
        if (entries.Count == Logs.Count)
            return;
        Logs.Clear();
        foreach (UcsiLogEntry entry in entries)
            Logs.Add(entry);
    }

    private void RaiseStateProperties()
    {
        RaisePropertyChanged(nameof(CurrentSample));
        RaisePropertyChanged(nameof(Mode));
        RaisePropertyChanged(nameof(RecordButtonText));
        RaisePropertyChanged(nameof(PlayPauseButtonText));
        RaisePropertyChanged(nameof(IsReplay));
        RaisePropertyChanged(nameof(CanRecord));
        RaisePropertyChanged(nameof(CanLoad));
        RaisePropertyChanged(nameof(CanPlayPause));
        RaisePropertyChanged(nameof(CanClearFaults));
        RaisePropertyChanged(nameof(HvpsSetpointKV));
        RaisePropertyChanged(nameof(HvpsSetpointEmission));
        RaisePropertyChanged(nameof(HvpsSetpointPower));
        RaisePropertyChanged(nameof(HvpsSetpointGrid));
        RaisePropertyChanged(nameof(HvpsSetpointHeat));
        RaisePropertyChanged(nameof(HvpsFeedbackKV));
        RaisePropertyChanged(nameof(HvpsFeedbackEmission));
        RaisePropertyChanged(nameof(HvpsFeedbackPower));
        RaisePropertyChanged(nameof(HvpsFeedbackGrid));
        RaisePropertyChanged(nameof(HvpsFeedbackHeat));
        RaisePropertyChanged(nameof(CoilsFeedbackXCoil));
        RaisePropertyChanged(nameof(CoilsFeedbackYCoil));
        RaisePropertyChanged(nameof(CoilsFeedbackFocus));
        RaisePropertyChanged(nameof(IsEmissionValid));
        RaisePropertyChanged(nameof(EmissionTextBoxBorder));
        RaisePropertyChanged(nameof(CoolingWaterPumpText));
        RaisePropertyChanged(nameof(CoolingRadiatorFanText));
        RaisePropertyChanged(nameof(CoolingWaterPumpColor));
        RaisePropertyChanged(nameof(CoolingRadiatorFanColor));
        RecordCommand.RaiseCanExecuteChanged();
        LoadCommand.RaiseCanExecuteChanged();
        PlayPauseCommand.RaiseCanExecuteChanged();
        ReturnToLiveCommand.RaiseCanExecuteChanged();
        ClearFaultsCommand.RaiseCanExecuteChanged();
    }

    private static string GetDisplayName<T>(T value) where T : Enum
    {
        var descriptor = typeof(T).GetMember(value.ToString())[0];
        return descriptor.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
            .OfType<System.ComponentModel.DataAnnotations.DisplayAttribute>()
            .FirstOrDefault()?.Name ?? value.ToString();
    }

    /// <summary>
    /// Sends HVPS KV (kilovoltage) command to the board with current HV and derived mA values.
    /// Called when user finishes editing the HV textbox (LostFocus event).
    /// Emission (mA) is derived from: Power / HV
    /// </summary>
    public async Task SendHvpsKvAsync()
    {
        await SendHvpsKvToBoard();
    }

    /// <summary>
    /// Sends HVPS Grid (grid voltage) command to the board with current Grid value.
    /// Called when user finishes editing the Grid textbox (LostFocus event).
    /// </summary>
    public async Task SendHvpsGridAsync()
    {
        await SendHvpsGridToBoard();
    }

    /// <summary>
    /// Sends HVPS Filament (heater) command to the board with current Heat value.
    /// Called when user finishes editing the Heat textbox (LostFocus event).
    /// </summary>
    public async Task SendHvpsFilamentAsync()
    {
        await SendHvpsFilamentToBoard();
    }

    /// <summary>
    /// Sends HVPS mA Limit command to the board with current MaLimitValue.
    /// Called when user clicks the Set button in the mA Limit section.
    /// </summary>
    public async Task SendMaLimitAsync()
    {
        await SendMaLimitToBoard();
    }

    private async Task SendHvpsKvToBoard()
    {
        try
        {
            await _commandInterface.SendHvpsKv(
                (float)_hvpsCommandHV,
                (float)_hvpsCommandPower);
            
            StartSetpointPollingWindow(_hvpsCommandHV, _hvpsCommandPower, _hvpsCommandGrid);
            
            _logBuffer.Log(
                $"HVPS KV command sent: HV={_hvpsCommandHV:F1}kV, Power={_hvpsCommandPower:F1}W",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            ErrorText = $"HVPS KV command failed: {ex.Message}";
            _logBuffer.Log(
                $"HVPS KV command failed: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
        }
    }

    private async Task SendHvpsGridToBoard()
    {
        try
        {
            await _commandInterface.SendHvpsGrid((float)_hvpsCommandGrid);
            
            StartSetpointPollingWindow(_hvpsCommandHV, _hvpsCommandPower, _hvpsCommandGrid);
            
            _logBuffer.Log(
                $"HVPS Grid command sent: Grid={_hvpsCommandGrid:F1}V",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            ErrorText = $"HVPS Grid command failed: {ex.Message}";
            _logBuffer.Log(
                $"HVPS Grid command failed: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
        }
    }

    private async Task SendHvpsFilamentToBoard()
    {
        try
        {
            await _commandInterface.SendHvpsFilament((float)_hvpsCommandHeat);
            
            _logBuffer.Log(
                $"HVPS Filament command sent: Heat={_hvpsCommandHeat:F0}mA",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            ErrorText = $"HVPS Filament command failed: {ex.Message}";
            _logBuffer.Log(
                $"HVPS Filament command failed: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
        }
    }

    public float MaLimitValue
    {
        get => _maLimitValue;
        set => SetProperty(ref _maLimitValue, value);
    }

    public async Task SendEmissionCommandAsync()
    {
        try
        {
            // Send START (0x03) if emission is off, STOP (0x04) if emission is on
            uint command = _emissionOn ? 0x04u : 0x03u; // 0x04 = STOP, 0x03 = START
            var response = await _commandInterface.SendHvpsEmission(command);
            
            // Note: Emission state is synced from telemetry in UpdateDetailedStatus() every tick,
            // not from the command response. Telemetry is the continuous source of truth.
        }
        catch (Exception ex)
        {
            ErrorText = $"Emission command failed: {ex.Message}";
        }
    }

    private async Task SendMaLimitToBoard()
    {
        try
        {
            await _commandInterface.SendHvpsMaLimit(MaLimitValue);
            
            _logBuffer.Log(
                $"HVPS mA Limit command sent: {MaLimitValue:F1}mA",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            ErrorText = $"HVPS mA Limit command failed: {ex.Message}";
            _logBuffer.Log(
                $"HVPS mA Limit command failed: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
        }
    }

    /// <summary>
    /// Sends HVPS PID Enable/Disable command to the board.
    /// Called when user toggles the PID Enabled checkbox.
    /// </summary>
    private async Task SendHvpsPidControlAsync(bool enabled)
    {
        try
        {
            await _commandInterface.SendHvpsPidControl(enabled);
            
            _logBuffer.Log(
                $"HVPS PID command sent: PID={(_pidEnabled ? "Enabled" : "Disabled")}",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            ErrorText = $"HVPS PID command failed: {ex.Message}";
            _logBuffer.Log(
                $"HVPS PID command failed: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
        }
    }

    /// <summary>
    /// Sends coil currents command to the board.
    /// Called when user adjusts coil sliders or textboxes.
    /// </summary>
    public async Task SendCoilsAsync()
    {
        try
        {
            await _commandInterface.SendCoils(
                (float)_coilsCommandXCoil,
                (float)_coilsCommandYCoil,
                (float)_coilsCommandFocus);
            
            _logBuffer.Log(
                $"Coils command sent: X={_coilsCommandXCoil:F3}A, Y={_coilsCommandYCoil:F3}A, Focus={_coilsCommandFocus:F3}A",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            ErrorText = $"Coils command failed: {ex.Message}";
            _logBuffer.Log(
                $"Coils command failed: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.System);
        }
    }
}
