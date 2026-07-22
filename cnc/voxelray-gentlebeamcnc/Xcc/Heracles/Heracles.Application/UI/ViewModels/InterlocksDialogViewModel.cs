using Heracles.Core.Models;
using Heracles.Application.AppLayer.Collimators;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Constants;
using Xcc.Core.Models;
using Xcc.Core.Services;

namespace Heracles.Application.UI.ViewModels;

public sealed class InterlocksDialogViewModel : DialogViewModelBase
{
    private static readonly SystemInterlock[] AllInterlocks = Enum.GetValues<SystemInterlock>();
    private readonly InterlockGroupStatusItem[] _technicalGroups;
    private readonly InterlockGroupStatusItem _masterFault;
    private readonly List<InterlockGroupStatusItem> _faultItems = [];
    private readonly ICollimatorModel _collimatorModel;
    private readonly IApplicatorReadinessSource _applicatorReadinessSource;
    private SystemInterlocks? _currentInterlocks;
    private ISystemTelemetry? _currentTelemetry;
    private readonly bool _isExternalApplication;
    private readonly IMainBoardAPI? _mainBoardApi;
    private readonly IPopUpService _popUpService;
    private bool _hasSystemFault;

    public InterlocksDialogViewModel(
        ICollimatorModel collimatorModel,
        IApplicatorReadinessSource applicatorReadinessSource,
        IGCBDataStore gcbDataStore,
        IEventAggregator eventAggregator,
        IHeraclesCoreSettings heraclesSettings,
        IContainerProvider containerProvider,
        IPopUpService popUpService)
    {
        _isExternalApplication = heraclesSettings is IHeraclesExternalSettings;
        _mainBoardApi = _isExternalApplication
            ? containerProvider.Resolve<IMainBoardAPI>()
            : null;
        _popUpService = popUpService;
        _collimatorModel = collimatorModel;
        _applicatorReadinessSource = applicatorReadinessSource;
        EStops = new InterlockGroupStatusItem(
            "E-stops",
            showDetailsWhenNotReady: true,
            new InterlockStatusItem(SystemInterlock.BaseEStopReleased, "Base e-stop"),
            new InterlockStatusItem(SystemInterlock.RemoteEStopReleased, "Remote e-stop"));
        Door = new InterlockGroupStatusItem(
            "Door closed",
            showDetailsWhenNotReady: false,
            new InterlockStatusItem(SystemInterlock.DoorClosed, "Door closed"));
        Keys = new InterlockGroupStatusItem(
            "Keys",
            showDetailsWhenNotReady: true,
            new InterlockStatusItem(SystemInterlock.BaseKeyOn, "Base key"),
            new InterlockStatusItem(SystemInterlock.RemoteKeyOn, "Remote key"));
        Applicator = new InterlockGroupStatusItem("Applicator", showDetailsWhenNotReady: false);
        OperatorInterlocks = [EStops, Door, Keys, Applicator];

        _technicalGroups =
        [
            new InterlockGroupStatusItem(
                "Robot arm",
                false,
                new InterlockStatusItem(SystemInterlock.Kuka1Ready, "Robot arm 1"),
                new InterlockStatusItem(SystemInterlock.Kuka2Ready, "Robot arm 2")),
            new InterlockGroupStatusItem(
                "Cooling system",
                false,
                new InterlockStatusItem(SystemInterlock.WaterLevelOk, "Water level"),
                new InterlockStatusItem(SystemInterlock.WaterTemperatureOk, "Water temperature"),
                new InterlockStatusItem(SystemInterlock.CoolerReady, "Cooler")),
            new InterlockGroupStatusItem(
                "Vacuum system",
                false,
                new InterlockStatusItem(SystemInterlock.IonPumpOk, "Vacuum system")),
            new InterlockGroupStatusItem(
                "Backup timers",
                false,
                new InterlockStatusItem(SystemInterlock.Timer1Ready, "Backup timer 1"),
                new InterlockStatusItem(SystemInterlock.Timer2Ready, "Backup timer 2")),
            new InterlockGroupStatusItem(
                "High-voltage system",
                false,
                new InterlockStatusItem(SystemInterlock.HvpsReady, "High-voltage system")),
            new InterlockGroupStatusItem(
                "Control system",
                false,
                new InterlockStatusItem(SystemInterlock.WatchdogReady, "Safety monitor"),
                new InterlockStatusItem(SystemInterlock.McuFaultClear, "System controller")),
            new InterlockGroupStatusItem(
                "Auxiliary safety circuit",
                false,
                new InterlockStatusItem(SystemInterlock.SpareInterlock1, "Spare interlock 1"),
                new InterlockStatusItem(SystemInterlock.SpareInterlock2, "Spare interlock 2")),
        ];
        _masterFault = new InterlockGroupStatusItem(
            "Master fault",
            false,
            new InterlockStatusItem(SystemInterlock.MasterFaultClear, "Master fault"));

        UpdateInterlocks(gcbDataStore.SystemTelemetry);
        UpdateFaults(gcbDataStore.ActiveFaults);
        eventAggregator
            .GetEvent<SystemTelemetryChangedEvent>()
            .Subscribe(UpdateInterlocks, ThreadOption.UIThread);
        eventAggregator
            .GetEvent<FaultsChangedEvent>()
            .Subscribe(UpdateFaults, ThreadOption.UIThread);
        _collimatorModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(ICollimatorModel.ActiveCollimator) or nameof(ICollimatorModel.Collimators))
                UpdateApplicatorReadiness();
        };
        _applicatorReadinessSource.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IApplicatorReadinessSource.CollimatorConfiguration))
                UpdateApplicatorReadiness();
        };
    }

    public InterlockGroupStatusItem EStops { get; }
    public InterlockGroupStatusItem Door { get; }
    public InterlockGroupStatusItem Keys { get; }
    public InterlockGroupStatusItem Applicator { get; }
    public IReadOnlyList<InterlockGroupStatusItem> OperatorInterlocks { get; }
    public ObservableCollection<InterlockGroupStatusItem> AttentionItems { get; } = [];
    public bool HasAttentionItems => AttentionItems.Count != 0;
    public bool ShowClearFaultsButton =>
        _isExternalApplication
        && (_masterFault.State == false || _hasSystemFault || _faultItems.Count != 0);
    public string SystemReadinessText => SystemIsReady switch
    {
        true => "Ready",
        false => "Attention required",
        null => "Status unavailable",
    };

    private bool? _systemIsReady;
    public bool? SystemIsReady
    {
        get => _systemIsReady;
        private set
        {
            if (SetProperty(ref _systemIsReady, value))
                RaisePropertyChanged(nameof(SystemReadinessText));
        }
    }
    private DelegateCommand? _clearFaultsCommand;
    public DelegateCommand ClearFaultsCommand => _clearFaultsCommand ??= new DelegateCommand(
        async () =>
        {
            if (_mainBoardApi is null)
                throw new InvalidOperationException("Fault clearing is unavailable in the Internal GUI.");

            try
            {
                await _mainBoardApi.ClearFaults();
            }
            catch (Exception ex)
            {
                _popUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.ClearErrorsTitle,
                    StringConstants.TreatmentConsole.ClearErrorsErrorMessage,
                    ex);
            }
        });


    private void UpdateInterlocks(ISystemTelemetry? telemetry)
    {
        _currentTelemetry = telemetry;
        _hasSystemFault = telemetry?.Faults.AnyActive == true;
        _currentInterlocks = telemetry?.Interlocks;

        foreach (var item in OperatorInterlocks)
        {
            if (item != Applicator)
                item.Update(_currentInterlocks);
        }
        foreach (var item in _technicalGroups)
            item.Update(_currentInterlocks);
        _masterFault.Update(_currentInterlocks);

        UpdateApplicatorReadiness();
        RefreshAttentionItems();
    }

    private void UpdateApplicatorReadiness()
    {
        var status = ApplicatorReadinessEvaluator.Evaluate(
            _collimatorModel,
            _applicatorReadinessSource.CollimatorConfiguration);
        Applicator.Update(
            status switch
            {
                ApplicatorReadiness.NoApplicator => "No applicator",
                ApplicatorReadiness.UnknownApplicator => "Unknown applicator",
                ApplicatorReadiness.IncorrectApplicator => "Incorrect applicator",
                _ => "Applicator",
            },
            status == ApplicatorReadiness.Ready);
        SystemIsReady = _currentTelemetry?.IsSystemReady(Applicator.State == true);
    }

    private void UpdateFaults(IReadOnlyList<FaultEntry> faults)
    {
        _faultItems.Clear();
        foreach (var fault in faults)
        {
            var item = new InterlockGroupStatusItem(fault.Message, false);
            item.Update(false);
            _faultItems.Add(item);
        }

        RefreshAttentionItems();
    }

    private void RefreshAttentionItems()
    {
        AttentionItems.Clear();
        foreach (var item in _faultItems)
            AttentionItems.Add(item);
        foreach (var item in _technicalGroups)
        {
            if (item.State == false)
                AttentionItems.Add(item);
        }

        if (_masterFault.State == false && !HasOtherBadInterlock(_currentInterlocks))
            AttentionItems.Add(_masterFault);

        RaisePropertyChanged(nameof(HasAttentionItems));
        RaisePropertyChanged(nameof(ShowClearFaultsButton));
    }

    private static bool HasOtherBadInterlock(SystemInterlocks? interlocks)
    {
        if (interlocks is null)
            return false;

        foreach (var interlock in AllInterlocks)
        {
            if (interlock != SystemInterlock.MasterFaultClear
                && interlocks.Value.GetState(interlock) == false)
                return true;
        }

        return false;
    }
}

public sealed class InterlockGroupStatusItem : BindableBase
{
    private readonly bool _showDetailsWhenNotReady;
    private bool? _state;

    public InterlockGroupStatusItem(
        string displayName,
        bool showDetailsWhenNotReady,
        params InterlockStatusItem[] details)
    {
        _displayName = displayName;
        DisplayName = displayName;
        _showDetailsWhenNotReady = showDetailsWhenNotReady;
        Details = details;
    }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }
    public IReadOnlyList<InterlockStatusItem> Details { get; }
    public bool ShowDetails => _showDetailsWhenNotReady && State != true;

    private string _displayName;
    public bool? State
    {
        get => _state;
        private set
        {
            if (SetProperty(ref _state, value))
                RaisePropertyChanged(nameof(ShowDetails));
        }
    }

    internal void Update(SystemInterlocks? interlocks)
    {
        var anyBad = false;
        var allGood = true;
        foreach (var detail in Details)
        {
            detail.State = interlocks?.GetState(detail.Interlock);
            anyBad |= detail.State == false;
            allGood &= detail.State == true;
        }

        State = anyBad ? false : allGood ? true : null;
    }

    internal void Update(bool? state) => State = state;

    internal void Update(string displayName, bool? state)
    {
        DisplayName = displayName;
        State = state;
    }
}

public sealed class InterlockStatusItem(SystemInterlock interlock, string displayName) : BindableBase
{
    private bool? _state;

    public SystemInterlock Interlock { get; } = interlock;
    public string DisplayName { get; } = displayName;

    public bool? State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
}
