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
using Xcc.Application.AppLayer.Service;

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
    private readonly IActionAuditService _actionAuditService;
    private bool _isTabActive;
    private sealed class PendingConfigChange(int firmwareIndex, float previousValue, float requestedValue)
    {
        public int FirmwareIndex { get; } = firmwareIndex;
        public float PreviousValue { get; } = previousValue;
        public float RequestedValue { get; } = requestedValue;
        public int WriteSucceeded;
        public int ReadbackConfirmed;
    }
    private PendingConfigChange? _pendingConfigChange;
    private uint _loadedConfigMask;
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
    private const int TelemetryFreshnessMilliseconds = 1_500;
    private double _emissionKv;
    private double _emissionPower;
    private double _emissionFilament = 1_000;
    private double _emissionDurationSeconds = 1;
    private double _emissionXCoilAmps;
    private double _emissionYCoilAmps;
    private double _emissionFocusCoilAmps;
    private CancellationTokenSource? _emissionSequenceCancellation;
    private Task? _emissionSequenceTask;
    private bool _emissionSequenceActive;
    private bool _emissionStopInProgress;
    private string _emissionSequenceStatus = "Idle.";

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
        SessionDataExportService exportService,
        IActionAuditService actionAuditService)
    {
        _coordinator = coordinator;
        _catalog = catalog;
        _hostCommands = hostCommands;
        _logBuffer = logBuffer;
        _commandInterface = commandInterface;
        _telemetryProcessor = telemetryProcessor;
        _hvpsUartInterface = hvpsUartInterface;
        _exportService = exportService;
        _actionAuditService = actionAuditService;

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
            // Firmware values must be read before a user can overwrite them; initial zeroes are placeholders.
            var setCommand = new DelegateCommand(
                () => SetConfigValueFromUI(collectionIndex),
                () => ConfigEditingEnabled && HvpsConnected && (_loadedConfigMask & (1u << def.fwIndex)) != 0);
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
        EmissionRunStopCommand = new DelegateCommand(
            async () => await RunOrStopEmissionAsync(),
            () => IsEmissionTabAvailable && !_emissionStopInProgress);
        StopCalibrationCommand = new DelegateCommand(
            async () => await StopCalibrationAsync(),
            () => CanStopCalibration);

        // Subscribe to HVPS connection state changes
        // The service event fires on the message loop thread, so we need to marshal to UI thread
        _hvpsUartInterface.ConnectionStateChanged += (sender, args) =>
        {
            // Use Dispatcher to ensure UI updates happen on the UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                HvpsConnected = args.IsConnected;
                if (!args.IsConnected)
                {
                    _loadedConfigMask = 0;
                    Interlocked.Exchange(ref _pendingConfigChange, null);
                }
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

    public void Activate()
    {
        if (_isTabActive) return;
        _isTabActive = true;
        _ = Task.Run(() => _hvpsUartInterface.InitializeAsync());
        _ = FetchVersionInfoAsync();
    }

    public void Deactivate()
    {
        if (!_isTabActive) return;
        _isTabActive = false;
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
    public DelegateCommand EmissionRunStopCommand { get; }
    public DelegateCommand StopCalibrationCommand { get; }

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
    public string EmissionSequenceStatus
    {
        get => _emissionSequenceStatus;
        private set => SetProperty(ref _emissionSequenceStatus, value);
    }
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

    public bool HasFreshLiveTelemetry =>
        Mode == UcsiMode.Live
        && CurrentSample is { } sample
        && DateTimeOffset.UtcNow - sample.ReceivedAtUtc <= TimeSpan.FromMilliseconds(TelemetryFreshnessMilliseconds);

    public bool CanUseCalibrationControls =>
        HasFreshLiveTelemetry
        && CurrentSample!.Value.Telemetry.ControlBoardState is
            GcbStateNew.Cold or GcbStateNew.ColdFault or GcbStateNew.Fault or GcbStateNew.Calibration;

    public bool CanStopCalibration =>
        HasFreshLiveTelemetry
        && CurrentSample?.Telemetry.ControlBoardState == GcbStateNew.Calibration;

    public bool CalibrationControlsUnavailable => !CanUseCalibrationControls;

    public string CalibrationControlsUnavailableReason =>
        !HasFreshLiveTelemetry
            ? Mode == UcsiMode.Replay
                ? "Calibration controls are unavailable during telemetry replay."
                : "Calibration controls are unavailable without current live telemetry."
            : $"Calibration controls are unavailable while the control state is {CurrentSample?.Telemetry.ControlBoardState.ToString() ?? SystemStateText}.";

    public bool IsEmissionTabAvailable =>
        HasFreshLiveTelemetry
        && CurrentSample is { } sample
        && (_emissionSequenceActive
            || IsEmissionStartState(sample.Telemetry.ControlBoardState)
            || IsNormalActiveState(sample.Telemetry.ControlBoardState));

    public bool EmissionTabUnavailable => !IsEmissionTabAvailable;

    public string EmissionTabUnavailableReason =>
        !HasFreshLiveTelemetry
            ? Mode == UcsiMode.Replay
                ? "Emission is unavailable during telemetry replay."
                : "Emission is unavailable without current live telemetry."
            : CurrentSample?.Telemetry.ControlBoardState == GcbStateNew.Calibration
                ? "Emission is unavailable while the control state is Calibration."
                : $"Emission cannot start or stop while the control state is {CurrentSample?.Telemetry.ControlBoardState.ToString() ?? SystemStateText}.";

    public string EmissionButtonText =>
        _emissionSequenceActive || IsNormalActiveState(CurrentSample?.Telemetry.ControlBoardState)
            ? "Stop"
            : "Emission";

    public bool IsEmissionInputValid =>
        double.IsFinite(EmissionKv)
        && double.IsFinite(EmissionPower)
        && double.IsFinite(EmissionMa)
        && double.IsFinite(EmissionFilament)
        && double.IsFinite(EmissionDurationSeconds)
        && double.IsFinite(EmissionXCoilAmps)
        && double.IsFinite(EmissionYCoilAmps)
        && double.IsFinite(EmissionFocusCoilAmps)
        && EmissionKv > 0
        && EmissionPower > 0
        && EmissionMa is > 0 and <= 6
        && EmissionFilament is >= 1_000 and <= 3_250
        && EmissionDurationSeconds is >= 0.1 and <= 180
        && EmissionXCoilAmps is >= -1.5 and <= 1.5
        && EmissionYCoilAmps is >= -1.5 and <= 1.5
        && EmissionFocusCoilAmps is >= 0 and <= 3;

    public double EmissionKv
    {
        get => _emissionKv;
        set
        {
            if(SetProperty(ref _emissionKv, value))
            {
                RaisePropertyChanged(nameof(EmissionMa));
                RaisePropertyChanged(nameof(IsEmissionInputValid));
            }
        }
    }

    public double EmissionPower
    {
        get => _emissionPower;
        set
        {
            if(SetProperty(ref _emissionPower, value))
            {
                RaisePropertyChanged(nameof(EmissionMa));
                RaisePropertyChanged(nameof(IsEmissionInputValid));
            }
        }
    }

    public double EmissionMa => EmissionKv > 0 ? EmissionPower / EmissionKv : 0;

    public double EmissionFilament
    {
        get => _emissionFilament;
        set
        {
            if(SetProperty(ref _emissionFilament, value))
                RaisePropertyChanged(nameof(IsEmissionInputValid));
        }
    }

    public double EmissionDurationSeconds
    {
        get => _emissionDurationSeconds;
        set
        {
            if(SetProperty(ref _emissionDurationSeconds, value))
                RaisePropertyChanged(nameof(IsEmissionInputValid));
        }
    }

    public double EmissionXCoilAmps
    {
        get => _emissionXCoilAmps;
        set
        {
            if(SetProperty(ref _emissionXCoilAmps, value))
                RaisePropertyChanged(nameof(IsEmissionInputValid));
        }
    }

    public double EmissionYCoilAmps
    {
        get => _emissionYCoilAmps;
        set
        {
            if(SetProperty(ref _emissionYCoilAmps, value))
                RaisePropertyChanged(nameof(IsEmissionInputValid));
        }
    }

    public double EmissionFocusCoilAmps
    {
        get => _emissionFocusCoilAmps;
        set
        {
            if(SetProperty(ref _emissionFocusCoilAmps, value))
                RaisePropertyChanged(nameof(IsEmissionInputValid));
        }
    }

    public double EmissionFeedbackKv => CurrentSample?.Telemetry.KvFeedback ?? 0;
    public double EmissionFeedbackMa => CurrentSample?.Telemetry.EmissionCurrent ?? 0;
    public double EmissionFeedbackPower => EmissionFeedbackKv * EmissionFeedbackMa;
    public double EmissionFeedbackFilament => CurrentSample?.Telemetry.HeaterCurrentFeedback ?? 0;
    public double EmissionFeedbackXCoil => (CurrentSample?.Telemetry.XCoilCurrent ?? 0) / 1_000;
    public double EmissionFeedbackYCoil => (CurrentSample?.Telemetry.YCoilCurrent ?? 0) / 1_000;
    public double EmissionFeedbackFocusCoil => (CurrentSample?.Telemetry.FocusCurrent ?? 0) / 1_000;
    public double EmissionFeedbackElapsedSeconds => CurrentSample?.Telemetry.PrimaryTimerValue ?? 0;

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

    public double CoilsFeedbackXCoil => (CurrentSample?.Telemetry.XCoilCurrent ?? 0.0) / 1_000;
    public double CoilsFeedbackYCoil => (CurrentSample?.Telemetry.YCoilCurrent ?? 0.0) / 1_000;
    public double CoilsFeedbackFocus => (CurrentSample?.Telemetry.FocusCurrent ?? 0.0) / 1_000;
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
        if(!EnsureCalibrationControlsAvailable())
            return;

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
        if(!CanUseCalibrationControls)
        {
            _setpointPollingActive = false;
            return;
        }

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
        Interlocked.Exchange(ref _pendingConfigChange, null);
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
        PendingConfigChange? pendingChange = null;
        try
        {
            // Get the firmware index from the ConfigItem
            int firmwareIndex = ConfigItems[collectionIndex].FirmwareIndex;
            string itemName = ConfigItems[collectionIndex].Name;
            float previousValue = (float)ConfigItems[collectionIndex].CurrentValue;
            
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
            
            if (previousValue != value)
            {
                pendingChange = new PendingConfigChange(firmwareIndex, previousValue, value);
                Interlocked.Exchange(ref _pendingConfigChange, pendingChange);
            }
            await _hvpsUartInterface.SetSystemConfigValue(firmwareIndex, value);
            if (pendingChange is not null)
            {
                Volatile.Write(ref pendingChange.WriteSucceeded, 1);
                RegisterConfirmedConfigChange(pendingChange);
            }
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
            Interlocked.CompareExchange(ref _pendingConfigChange, null, pendingChange);
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

    private void RegisterConfirmedConfigChange(PendingConfigChange pendingChange)
    {
        if (Volatile.Read(ref pendingChange.WriteSucceeded) == 1 &&
            Volatile.Read(ref pendingChange.ReadbackConfirmed) == 1 &&
            ReferenceEquals(Interlocked.CompareExchange(ref _pendingConfigChange, null, pendingChange), pendingChange))
        {
            _actionAuditService.RegisterAction("Configuration change confirmed",
                $"Entity=HVPSConfiguration; Id={pendingChange.FirmwareIndex}; Fields=Value");
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
            Interlocked.Exchange(ref _pendingConfigChange, null);
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
        var pendingChange = Volatile.Read(ref _pendingConfigChange);
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
                    _loadedConfigMask |= 1u << firmwareIndex;
                }
            }

            if (pendingChange is not null)
            {
                if (pendingChange.FirmwareIndex < response.Values.Length &&
                    response.Values[pendingChange.FirmwareIndex] == pendingChange.RequestedValue &&
                    response.Values[pendingChange.FirmwareIndex] != pendingChange.PreviousValue)
                {
                    Volatile.Write(ref pendingChange.ReadbackConfirmed, 1);
                    RegisterConfirmedConfigChange(pendingChange);
                }
                else
                {
                    Interlocked.CompareExchange(ref _pendingConfigChange, null, pendingChange);
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
        if (!ConfigEditingEnabled || !HvpsConnected || (_loadedConfigMask & (1u << item.FirmwareIndex)) == 0)
            return;
        
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
            if(!string.IsNullOrWhiteSpace(_coordinator.LastError))
                ErrorText = _coordinator.LastError;
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

        Exception? lastException = null;
        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                var versionInfo = await _commandInterface.GetVersionInfo();
                _versionInfoFetched = true;
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
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < 5)
                    await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        _logBuffer.Log(
            $"Failed to retrieve firmware versions after 5 attempts: {lastException?.Message}",
            LogRecordSeverity.Warn,
            LogRecordType.System);
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
            DateTimeOffset exportedAtUtc = DateTimeOffset.UtcNow;
            IReadOnlyList<UcsiTelemetrySample> samples =
                _coordinator.LiveHistory.GetSince(exportedAtUtc - TimeSpan.FromMinutes(5));
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
            bool? state = telemetry?.Interlocks.GetState(values[index]);
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
        RaisePropertyChanged(nameof(HasFreshLiveTelemetry));
        RaisePropertyChanged(nameof(CanUseCalibrationControls));
        RaisePropertyChanged(nameof(CanStopCalibration));
        RaisePropertyChanged(nameof(CalibrationControlsUnavailable));
        RaisePropertyChanged(nameof(CalibrationControlsUnavailableReason));
        RaisePropertyChanged(nameof(IsEmissionTabAvailable));
        RaisePropertyChanged(nameof(EmissionTabUnavailable));
        RaisePropertyChanged(nameof(EmissionTabUnavailableReason));
        RaisePropertyChanged(nameof(EmissionButtonText));
        RaisePropertyChanged(nameof(IsEmissionInputValid));
        RaisePropertyChanged(nameof(EmissionFeedbackKv));
        RaisePropertyChanged(nameof(EmissionFeedbackMa));
        RaisePropertyChanged(nameof(EmissionFeedbackPower));
        RaisePropertyChanged(nameof(EmissionFeedbackFilament));
        RaisePropertyChanged(nameof(EmissionFeedbackXCoil));
        RaisePropertyChanged(nameof(EmissionFeedbackYCoil));
        RaisePropertyChanged(nameof(EmissionFeedbackFocusCoil));
        RaisePropertyChanged(nameof(EmissionFeedbackElapsedSeconds));
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
        EmissionRunStopCommand.RaiseCanExecuteChanged();
        StopCalibrationCommand.RaiseCanExecuteChanged();
    }

    private static string GetDisplayName<T>(T value) where T : Enum
    {
        var descriptor = typeof(T).GetMember(value.ToString())[0];
        return descriptor.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
            .OfType<System.ComponentModel.DataAnnotations.DisplayAttribute>()
            .FirstOrDefault()?.Name ?? value.ToString();
    }

    private static bool IsEmissionStartState(GcbStateNew? state) =>
        state is GcbStateNew.Cold or GcbStateNew.Primed or GcbStateNew.Staged;

    private static bool IsNormalActiveState(GcbStateNew? state) =>
        state is GcbStateNew.DailyWarmup
            or GcbStateNew.Warmup
            or GcbStateNew.Staging
            or GcbStateNew.HvpsCheck
            or GcbStateNew.HVSetup
            or GcbStateNew.Ready
            or GcbStateNew.Launching
            or GcbStateNew.Emission
            or GcbStateNew.Termination
            or GcbStateNew.Discharge;

    private bool EnsureCalibrationControlsAvailable()
    {
        if(CanUseCalibrationControls)
            return true;

        SetCommandError(CalibrationControlsUnavailableReason, LogRecordSeverity.Warn);
        return false;
    }

    private void SetCommandError(string message, LogRecordSeverity severity = LogRecordSeverity.Error)
    {
        ErrorText = message;
        _logBuffer.Log(message, severity, LogRecordType.System);
    }

    private void ReportEmissionSequence(string message, LogRecordSeverity severity = LogRecordSeverity.Info)
    {
        EmissionSequenceStatus = message;
        _logBuffer.Log(message, severity, LogRecordType.System);
    }

    public async Task RunOrStopEmissionAsync()
    {
        GcbStateNew? currentState = CurrentSample?.Telemetry.ControlBoardState;
        if(_emissionSequenceActive || IsNormalActiveState(currentState))
        {
            await StopNormalEmissionAsync();
            return;
        }

        if(!IsEmissionTabAvailable || !IsEmissionStartState(currentState))
        {
            SetCommandError(EmissionTabUnavailableReason, LogRecordSeverity.Warn);
            return;
        }

        if(!IsEmissionInputValid)
        {
            string message =
                $"Emission parameters are invalid: kV={EmissionKv}, power={EmissionPower}, derived mA={EmissionMa}, " +
                $"filament={EmissionFilament}, time={EmissionDurationSeconds}, X={EmissionXCoilAmps}, " +
                $"Y={EmissionYCoilAmps}, focus={EmissionFocusCoilAmps}.";
            ReportEmissionSequence(message, LogRecordSeverity.Error);
            SetCommandError(message);
            return;
        }

        var parameters = new EmissionParameters(
            (float)EmissionKv,
            (float)EmissionPower,
            (float)EmissionFilament,
            (float)EmissionDurationSeconds,
            (float)EmissionXCoilAmps,
            (float)EmissionYCoilAmps,
            (float)EmissionFocusCoilAmps);
        ReportEmissionSequence(
            $"Starting one-point emission: {parameters.Kv:F1} kV, {parameters.Power:F1} W, " +
            $"{parameters.Power / parameters.Kv:F3} mA, filament {parameters.Filament:F0} mA, " +
            $"duration {parameters.DurationSeconds:F1} s.");
        var cancellation = new CancellationTokenSource();
        _emissionSequenceCancellation = cancellation;
        _emissionSequenceActive = true;
        Task sequenceTask = RunEmissionSequenceAsync(parameters, cancellation.Token);
        _emissionSequenceTask = sequenceTask;
        RaiseStateProperties();

        try
        {
            await sequenceTask;
        }
        catch(OperationCanceledException)
        {
            ReportEmissionSequence("Emission sequence canceled.", LogRecordSeverity.Warn);
        }
        catch(Exception exception)
        {
            ReportEmissionSequence($"Emission sequence failed: {exception.Message}", LogRecordSeverity.Error);
            SetCommandError(exception.Message);
        }
        finally
        {
            if(ReferenceEquals(_emissionSequenceTask, sequenceTask))
            {
                _emissionSequenceTask = null;
                _emissionSequenceCancellation = null;
                _emissionSequenceActive = false;
                cancellation.Dispose();
                RaiseStateProperties();
            }
        }
    }

    private async Task StopNormalEmissionAsync()
    {
        if(_emissionStopInProgress)
            return;

        _emissionStopInProgress = true;
        RaiseStateProperties();
        try
        {
            _emissionSequenceCancellation?.Cancel();
            Task? sequenceTask = _emissionSequenceTask;
            if(sequenceTask is not null)
            {
                try
                {
                    await sequenceTask;
                }
                catch(OperationCanceledException)
                {
                }
                catch(Exception exception)
                {
                    _logBuffer.Log(exception.Message, LogRecordSeverity.Warn, LogRecordType.System);
                }
            }

            await ExecuteEmissionStepAsync("Stop", _commandInterface.Stop);
            ReportEmissionSequence("Stop command accepted; waiting for firmware state transition.");
        }
        catch(Exception exception)
        {
            SetCommandError(exception.Message);
        }
        finally
        {
            _emissionStopInProgress = false;
            RaiseStateProperties();
        }
    }

    private async Task RunEmissionSequenceAsync(EmissionParameters parameters, CancellationToken cancellationToken)
    {
        GcbStateNew state = GetCurrentLiveState();
        ReportEmissionSequence($"Emission sequence entered from control state {state}.");
        if(state == GcbStateNew.Staged)
        {
            await ExecuteEmissionStepAsync("Stop staged plan", _commandInterface.Stop);
            await WaitForControlStateAsync("Stop staged plan", cancellationToken, GcbStateNew.Cold);
            state = GcbStateNew.Cold;
        }

        if(state == GcbStateNew.Cold)
        {
            // Cold does not clear PLAN_STAGED_BOOL. Without an explicit wipe,
            // warmup returns to Staged instead of Primed after the first run.
            await ExecuteEmissionStepAsync("Reset timers", _commandInterface.ResetTimers);
            await ExecuteEmissionStepAsync("Clear stale plan", _commandInterface.ClearPlan);
            await ExecuteEmissionStepAsync(
                "Warmup",
                () => _commandInterface.WarmUp(parameters.Filament));
            await WaitForControlStateAsync("Warmup", cancellationToken, GcbStateNew.Primed);
        }
        else if(state != GcbStateNew.Primed)
        {
            throw new InvalidOperationException($"Emission cannot start from control state {state}.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        GcbSession session = await ExecuteEmissionStepAsync(
            "New session",
            _commandInterface.NewSession);
        await WaitForControlStateAsync("New session", cancellationToken, GcbStateNew.Staging);

        var point = new GcbOperationalPoint
        {
            TotalPointTime = parameters.DurationSeconds,
            InitialRemainingPointTime = parameters.DurationSeconds,
            RemainingPointTime = parameters.DurationSeconds,
            SetpointKv = parameters.Kv,
            TargetMA = parameters.Power / parameters.Kv,
            FilamentSetpoint = parameters.Filament,
            XCoilSetpoint = parameters.XCoilAmps * 1_000,
            YCoilSetpoint = parameters.YCoilAmps * 1_000,
            FocusCoilSetpoint = parameters.FocusCoilAmps * 1_000,
        };

        await ExecuteEmissionStepAsync(
            "Load operational point",
            () => _commandInterface.SendOperationalPoint(OperationalPointCmdType.Load, point, session));
        cancellationToken.ThrowIfCancellationRequested();
        await ExecuteEmissionStepAsync("Stage plan", _commandInterface.StagePlan);
        await WaitForControlStateAsync("Stage plan", cancellationToken, GcbStateNew.Staged);
        await ExecuteEmissionStepAsync(
            "Confirm operational point",
            () => _commandInterface.SendOperationalPoint(OperationalPointCmdType.Confirmation, point, session));
        cancellationToken.ThrowIfCancellationRequested();
        await ExecuteEmissionStepAsync(
            "Release plan",
            () => _commandInterface.ReleasePlan(GCBReleaseCommandScope.Plan, session));
        await WaitForControlStateAsync("Release plan", cancellationToken, GcbStateNew.Ready);
        await ExecuteEmissionStepAsync(
            "Release point",
            () => _commandInterface.ReleasePlan(GCBReleaseCommandScope.Point, session));
        await WaitForControlStateAsync("Release point", cancellationToken, GcbStateNew.Emission);

        _logBuffer.Log(
            "One-point emission reached the Emission state.",
            LogRecordSeverity.Info,
            LogRecordType.System);
    }

    private GcbStateNew GetCurrentLiveState()
    {
        if(!HasFreshLiveTelemetry || CurrentSample is not { } sample)
            throw new InvalidOperationException("Current live telemetry is required for emission.");
        return sample.Telemetry.ControlBoardState;
    }

    private async Task WaitForControlStateAsync(
        string step,
        CancellationToken cancellationToken,
        params GcbStateNew[] targetStates)
    {
        GcbStateNew? lastReportedState = null;
        while(true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GcbStateNew state = GetCurrentLiveState();
            if(targetStates.Contains(state))
            {
                ReportEmissionSequence($"{step}: reached control state {state}.");
                return;
            }
            if(state != lastReportedState)
            {
                ReportEmissionSequence(
                    $"{step}: waiting for {string.Join(" or ", targetStates)}; current state is {state}.");
                lastReportedState = state;
            }
            if(state is GcbStateNew.FaultDischarge
                or GcbStateNew.Fault
                or GcbStateNew.ColdFault
                or GcbStateNew.WarmupFault
                or GcbStateNew.SystemCrash
                or GcbStateNew.Calibration)
            {
                throw new InvalidOperationException($"{step} entered terminal control state {state}.");
            }
            await Task.Delay(20, cancellationToken);
        }
    }

    private async Task ExecuteEmissionStepAsync(string step, Func<Task> action)
    {
        ReportEmissionSequence($"Sending {step} command.");
        try
        {
            await action();
            ReportEmissionSequence($"{step} command accepted.");
        }
        catch(Exception exception)
        {
            throw new InvalidOperationException($"{step} failed: {exception.Message}", exception);
        }
    }

    private async Task<T> ExecuteEmissionStepAsync<T>(string step, Func<Task<T>> action)
    {
        ReportEmissionSequence($"Sending {step} command.");
        try
        {
            T result = await action();
            ReportEmissionSequence($"{step} command accepted.");
            return result;
        }
        catch(Exception exception)
        {
            throw new InvalidOperationException($"{step} failed: {exception.Message}", exception);
        }
    }

    private readonly record struct EmissionParameters(
        float Kv,
        float Power,
        float Filament,
        float DurationSeconds,
        float XCoilAmps,
        float YCoilAmps,
        float FocusCoilAmps);

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
        if(!EnsureCalibrationControlsAvailable())
            return;

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
        if(!EnsureCalibrationControlsAvailable())
            return;

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
        if(!EnsureCalibrationControlsAvailable())
            return;

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

    public async Task StopCalibrationAsync()
    {
        if(!CanStopCalibration)
        {
            SetCommandError(
                $"Stop calibration is unavailable while the control state is {CurrentSample?.Telemetry.ControlBoardState.ToString() ?? SystemStateText}.",
                LogRecordSeverity.Warn);
            return;
        }

        try
        {
            await _commandInterface.SendHvpsEmission(0x04u);
            _logBuffer.Log(
                "Calibration stop command sent.",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch(Exception exception)
        {
            SetCommandError($"Calibration stop command failed: {exception.Message}");
        }
    }

    public async Task SendEmissionCommandAsync()
    {
        if(!EnsureCalibrationControlsAvailable())
            return;

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
        if(!EnsureCalibrationControlsAvailable())
            return;

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
        if(!EnsureCalibrationControlsAvailable())
            return;

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
        if(!EnsureCalibrationControlsAvailable())
            return;

        try
        {
            await _commandInterface.SendCoils(
                (float)_coilsCommandXCoil * 1_000,
                (float)_coilsCommandYCoil * 1_000,
                (float)_coilsCommandFocus * 1_000);
            
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
