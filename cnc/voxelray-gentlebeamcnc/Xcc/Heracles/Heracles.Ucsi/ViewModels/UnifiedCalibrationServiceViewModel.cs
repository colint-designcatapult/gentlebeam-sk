using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Prism.Commands;
using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Ucsi.ViewModels;

public interface IUcsiHostCommands
{
    bool CanClearFaults { get; }
    string ClearFaultsUnavailableReason { get; }
    Task ClearFaultsAsync();
}

public sealed class UnavailableUcsiHostCommands : IUcsiHostCommands
{
    public bool CanClearFaults => false;
    public string ClearFaultsUnavailableReason => "Clear Faults is unavailable in this host.";
    public Task ClearFaultsAsync() => Task.CompletedTask;
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
    private double _timelineSeconds;
    private double _timelineMaximumSeconds;
    private CancellationTokenSource? _seekCancellation;
    private double _hvpsCommandHV;
    private double _hvpsCommandPower;
    private double _hvpsCommandGrid;
    private double _hvpsCommandHeat;
    private float _maLimitValue = 4.0f;
    private bool _pidEnabled;
    private bool _coolingWaterPumpEnabled;
    private bool _coolingRadiatorFanEnabled;

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
        IGcbCommandInterface commandInterface)
    {
        _coordinator = coordinator;
        _catalog = catalog;
        _hostCommands = hostCommands;
        _logBuffer = logBuffer;
        _commandInterface = commandInterface;

        // Initialize mA limit asynchronously on startup
        _ = InitializeMaLimitAsync();

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
            new("High Voltage Interlock"), new("High Voltage Status"), new("Master Fault"),
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

        AddGraphCommand = new DelegateCommand(() => Graphs.Add(new GraphPaneViewModel(_nextGraphNumber++, catalog, RemoveGraph)));
        ApplyMonitoredSelectionCommand = new DelegateCommand(ApplyMonitoredSelection);
        RecordCommand = new DelegateCommand(async () => await ToggleRecordingAsync(), () => CanRecord);
        LoadCommand = new DelegateCommand(async () => await LoadAsync(), () => CanLoad);
        PlayPauseCommand = new DelegateCommand(async () => await RunCommandAsync(() => _coordinator.TogglePlaybackAsync()), () => CanPlayPause);
        ReturnToLiveCommand = new DelegateCommand(async () => await RunCommandAsync(_coordinator.ReturnToLiveAsync), () => IsReplay);
        ClearFaultsCommand = new DelegateCommand(async () => await RunCommandAsync(_hostCommands.ClearFaultsAsync), () => CanClearFaults);
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

    public DelegateCommand AddGraphCommand { get; }
    public DelegateCommand ApplyMonitoredSelectionCommand { get; }
    public DelegateCommand RecordCommand { get; }
    public DelegateCommand LoadCommand { get; }
    public DelegateCommand PlayPauseCommand { get; }
    public DelegateCommand ReturnToLiveCommand { get; }
    public DelegateCommand ClearFaultsCommand { get; }

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
            // Clamp HV to [0, 120] - static range
            double clampedValue = Math.Max(0, Math.Min(120, value));
            if (SetProperty(ref _hvpsCommandHV, clampedValue))
            {
                // When HV changes, e- changes too (e- = Power / HV)
                RaisePropertyChanged(nameof(HvpsCommandEmission));
                RaisePropertyChanged(nameof(IsEmissionValid));
                RaisePropertyChanged(nameof(EmissionValidationError));
                RaisePropertyChanged(nameof(HasEmissionExceedsMaximumError));
                RaisePropertyChanged(nameof(EmissionTextBoxBorder));
                RaisePropertyChanged(nameof(CanEnableEmission));
            }
        }
    }

    // e- is derived: e- = Power / HV (should stay <= 4.0 mA)
    public double HvpsCommandEmission => _hvpsCommandHV > 0 ? _hvpsCommandPower / _hvpsCommandHV : 0;

    // Emission is valid if it doesn't exceed 4.0 mA
    public bool IsEmissionValid => HvpsCommandEmission <= 4.0;

    // Error message for invalid emission
    public string EmissionValidationError => 
        !IsEmissionValid ? "Emission limit exceeded (>4 mA)" : string.Empty;

    // Bool flag for error state - true when emission exceeds maximum
    public bool HasEmissionExceedsMaximumError => !IsEmissionValid;

    // Emission button can only be enabled if valid AND both Power and HV are set
    public bool CanEnableEmission => IsEmissionValid && HvpsCommandPower > 0 && HvpsCommandHV > 0;

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
                // When Power changes, update dependent properties
                RaisePropertyChanged(nameof(IsHvEnabled));
                RaisePropertyChanged(nameof(HvCommandSliderOpacity));
                RaisePropertyChanged(nameof(HvpsCommandEmission));
                RaisePropertyChanged(nameof(IsEmissionValid));
                RaisePropertyChanged(nameof(EmissionValidationError));
                RaisePropertyChanged(nameof(HasEmissionExceedsMaximumError));
                RaisePropertyChanged(nameof(EmissionTextBoxBorder));
                RaisePropertyChanged(nameof(CanEnableEmission));
                
                // When Power is 0, reset HV to 0 and send command to firmware
                if (clampedValue == 0 && _hvpsCommandHV > 0)
                {
                    HvpsCommandHV = 0;
                    _ = SendHvpsKvToBoard(); // Send HV=0 command to firmware
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
    public double HvpsSetpointEmission => CurrentSample?.Telemetry.EmissionCurrentLimit ?? 0.0;
    public double HvpsSetpointPower => CurrentSample?.Telemetry.HvpsPowerSetpoint ?? 0.0;
    public double HvpsSetpointGrid => CurrentSample?.Telemetry.GridSetpoint ?? 0.0;
    public double HvpsSetpointHeat => CurrentSample?.Telemetry.HeaterCurrentSetpoint ?? 0.0;

    public double HvpsFeedbackKV => CurrentSample?.Telemetry.KvFeedback ?? 0.0;
    public double HvpsFeedbackEmission => CurrentSample?.Telemetry.EmissionCurrent ?? 0.0;
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
                telemetry.Hvps.MasterFault,
            ];
        for (int index = 0; index < HvpsStates.Count; index++)
        {
            HvpsStates[index].Value = hvps[index].HasValue ? hvps[index]!.Value ? "On" : "Off" : "N/A";
            HvpsStates[index].IsActive = hvps[index] == true;
            HvpsStates[index].IsAvailable = hvps[index].HasValue;
        }

        // Sync PID Enabled state from telemetry (HvpsStates[5])
        _pidEnabled = hvps[5].GetValueOrDefault(false);

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
        RaisePropertyChanged(nameof(HvpsCommandPower));
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
        RaisePropertyChanged(nameof(IsEmissionValid));
        RaisePropertyChanged(nameof(EmissionValidationError));
        RaisePropertyChanged(nameof(HasEmissionExceedsMaximumError));
        RaisePropertyChanged(nameof(EmissionTextBoxBorder));
        RaisePropertyChanged(nameof(CanEnableEmission));
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

    /// <summary>
    /// Sends a Version Info request to the board for diagnostics.
    /// Called when user clicks the Test Version Request button.
    /// </summary>
    public async Task SendVersionRequestAsync()
    {
        try
        {
            var versionInfo = await _commandInterface.GetVersionInfo();
            ErrorText = $"Firmware Version: {versionInfo.Major}.{versionInfo.Minor} (Level: {versionInfo.Level}, Mode: {versionInfo.Mode})";
        }
        catch (Exception ex)
        {
            ErrorText = $"Version request failed: {ex.Message}";
        }
    }

    private async Task SendHvpsKvToBoard()
    {
        try
        {
            await _commandInterface.SendHvpsKv(
                (float)_hvpsCommandHV,
                (float)_hvpsCommandPower);
            
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

    private async Task InitializeMaLimitAsync()
    {
        try
        {
            await _commandInterface.SendHvpsMaLimit(_maLimitValue);
            _logBuffer.Log(
                $"HVPS mA Limit initialized: {_maLimitValue:F1}mA",
                LogRecordSeverity.Info,
                LogRecordType.System);
        }
        catch (Exception ex)
        {
            _logBuffer.Log(
                $"HVPS mA Limit initialization failed: {ex.Message}",
                LogRecordSeverity.Warn,
                LogRecordType.System);
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
}
