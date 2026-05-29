using System;
using System.Threading.Tasks;
using System.Windows.Data;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.Service.TreatmentConsole;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Models;
using Xcc.Application.ViewModels.TreatmentConsole.QualityAssurance.DailyWarmUpUiStateMachine;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Models.RDBMS;
using Xcc.Core.Services;

namespace Xcc.Application.ViewModels.TreatmentConsole.QualityAssurance
{
    namespace DailyWarmUpUiStateMachine
    {
        public enum UIButtonState
        {
            None = 0,
            WarmUp,
            WarmUpProgress,
            Stop,
            ClearErrors,
            Conditioning
        }

        public enum Action
        {
            None = 0,
            Conditioning,
            WarmUp
        }

        public interface IDailyWarmUpUiStateMachine
        {
            public ButtonInfo LeftButton { get; }
            public ButtonInfo RightButton { get; }
            Action OngoingAction { get; }

            void OnGcbStateChange(GcbStateNew gcbState);
            void OnWarmupAction(Action action);
        }

        public class ButtonInfo : Common.ButtonInfo
        {
            public UIButtonState State { get; set; }
        }

        public class DailyWarmUpStateMachine : BindableBase, IDailyWarmUpUiStateMachine
        {
            private ButtonInfo _leftButton = new ButtonInfo { State = UIButtonState.Conditioning, IsEnabled = true };
            private ButtonInfo _rightButton = new ButtonInfo { State = UIButtonState.Stop, IsEnabled = false };

            public Action OngoingAction { get; set; } = Action.None;
            public GcbStateNew LastGcbState { get; private set; }
            public ButtonInfo LeftButton { get => _leftButton; private set => SetProperty(ref _leftButton, value); }
            public ButtonInfo RightButton { get => _rightButton; private set => SetProperty(ref _rightButton, value); }

            public void OnGcbStateChange(GcbStateNew gcbState)
            {
                UpdateButtons(gcbState);

                LastGcbState = gcbState;
            }

            public void OnWarmupAction(Action action)
            {
                OngoingAction = action;
                UpdateButtons(LastGcbState);
            }

            private void UpdateButtons(GcbStateNew gcbState)
            {
                // Left button
                switch (gcbState)
                {
                    case GcbStateNew.Fault:
                    case GcbStateNew.ColdFault:
                    case GcbStateNew.WarmupFault:
                        LeftButton = new ButtonInfo { State = UIButtonState.ClearErrors, IsEnabled = true };
                        break;
                    case GcbStateNew.DailyWarmup:
                        LeftButton = new ButtonInfo { State = UIButtonState.WarmUpProgress, IsEnabled = true };
                        break;
                    case GcbStateNew.NoComm:
                    case GcbStateNew.Warmup:
                        LeftButton = new ButtonInfo { State = UIButtonState.Conditioning, IsEnabled = false };
                        break;
                    default:
                        LeftButton = new ButtonInfo { State = UIButtonState.Conditioning, IsEnabled = true };
                        break;
                }

                // Right button ('Stop') can stop ongoing processes only
                switch (gcbState)
                {
                    case GcbStateNew.DailyWarmup:
                    case GcbStateNew.Warmup:
                    case GcbStateNew.Cold:
                        RightButton = new ButtonInfo { State = UIButtonState.Stop, IsEnabled = OngoingAction != Action.None };
                        break;
                    default:
                        RightButton = new ButtonInfo { State = UIButtonState.Stop, IsEnabled = false };
                        break;
                }
            }
        }
    }


    public class DailyWarmUpViewModel : BindableBase
    {
        #region Contructors

        public DailyWarmUpViewModel(
            IEventAggregator eventAggregator,
            IWarmUpSettings warmupSettings,
            IMainBoardModel mainBoardModel,
            IWarmupObservableHistory warmupHistory,
            IWarmupService warmupService,
            IGcbIndicators gcbIndicators,
            IDialogService dialogService,
            IActiveHeadProvider activeHeadProvider,
            ILogWriter logWriter,
            IActionAuditService actionAuditService,
            IPopUpService popUpService)
        {
            WarmupSettings = warmupSettings;
            MainBoardModel = mainBoardModel;
            WarmupHistory = warmupHistory;
            WarmupService = warmupService;
            GcbIndicators = gcbIndicators;
            DialogService = dialogService;
            ActiveHeadProvider = activeHeadProvider;
            LogWriter = logWriter;
            ActionAuditService = actionAuditService;
            PopUpService = popUpService;

            WarmUpViewSource = new CollectionViewSource { Source = WarmupHistory.ObservableWarmupRecords };
            WarmUpViewSource.Filter += OnShowAllChanged;

            WarmupHistory.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IWarmupObservableHistory.ObservableWarmupRecords))
                {
                    WarmUpViewSource.Source = WarmupHistory.ObservableWarmupRecords;
                }
            };

            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnSystemTelemetryChanged, ThreadOption.UIThread);

            UIStateMachine = new DailyWarmUpStateMachine();

            _ = WarmupService.UpdateWarmupHistoryAsync();
        }

        #endregion Contructors


        #region Read-only properties
        public IWarmUpSettings WarmupSettings { get; }
        public IMainBoardModel MainBoardModel { get; }
        public IWarmupObservableHistory WarmupHistory { get; }
        public IWarmupService WarmupService { get; }
        public IGcbIndicators GcbIndicators { get; }
        public IDialogService DialogService { get; }
        public IActiveHeadProvider ActiveHeadProvider { get; }
        public ILogWriter LogWriter { get; }
        public IActionAuditService ActionAuditService { get; }
        public IPopUpService PopUpService { get; }
        public CollectionViewSource WarmUpViewSource { get; }
        public IDailyWarmUpUiStateMachine UIStateMachine { get; }
        #endregion Read-only properties

        #region Properties
        private GcbStateNew _previousState = GcbStateNew.NoComm;
        private GcbStateNew _state = GcbStateNew.NoComm;

        private bool _isWarmingUp;
        public bool IsWarmingUp { get => _isWarmingUp; set => SetProperty(ref _isWarmingUp, value); }

        private bool _showAll;
        public bool ShowAll
        {
            get => _showAll;
            set
            {
                if (SetProperty(ref _showAll, value))
                {
                    WarmUpViewSource.View.Refresh();
                }
            }
        }

        #endregion Properties

        #region Commands

        private DelegateCommand? _fullWarmupCommand;
        public DelegateCommand FullWarmupCommand => _fullWarmupCommand ??= new DelegateCommand(
            () =>
            {
                UIStateMachine.OnWarmupAction(DailyWarmUpUiStateMachine.Action.Conditioning);
                _ = WarmUp(WarmupType.Full);
            },
            canExecuteMethod: CanStartWarmUp);

        private DelegateCommand? _warmupCommand;
        public DelegateCommand WarmupCommand => _warmupCommand ??= new DelegateCommand(
            () =>
            {
                UIStateMachine.OnWarmupAction(DailyWarmUpUiStateMachine.Action.WarmUp);
                _ = WarmUp(WarmupType.Fast);
            },
            canExecuteMethod: CanStartWarmUp);

        private DelegateCommand? _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ??= new DelegateCommand(
            () =>
            {
                _= StopAsync();
            });

        private DelegateCommand? _clearErrorsCommand;
        public DelegateCommand ClearErrorsCommand => _clearErrorsCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    await MainBoardModel.ClearFaults();

                    //try
                    //{
                    //    await Conditioning();
                    //}
                    //catch (Exception ex)
                    //{
                    //    PopUpService.LogAndShowError("Warmup error", $"Failed to resume warmup after fault", ex);
                    //}
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        StringConstants.TreatmentConsole.ClearErrorsTitle,
                        StringConstants.TreatmentConsole.ClearErrorsErrorMessage,
                        ex);
                }
                finally
                {
                    ValidateCanExecuteCommands();
                }
            },
            canExecuteMethod: () => true);
        #endregion Commands

        #region Private methods
        private void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            _state = systemTelemetry?.ControlBoardState ?? GcbStateNew.NoComm;

            if (_previousState != _state)
            {
                UIStateMachine.OnGcbStateChange(_state);
                _previousState = _state;
            }
            
            ValidateCanExecuteCommands();
        }

        private void OnShowAllChanged(object sender, FilterEventArgs e)
        {
            if (ShowAll)
            {
                e.Accepted = true;
                return;
            }

            IWarmUp? warmUp = e.Item as IWarmUp;
            if (warmUp == null)
            {
                e.Accepted = true;
                return;
            }

            e.Accepted = warmUp.Type == WarmupType.Full;
        }

        private async Task StopAsync()
        {
            try
            {
                await MainBoardModel.Stop();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.StopTitle,
                    StringConstants.TreatmentConsole.StopErrorMessage,
                    ex);
            }
            finally
            {
                ValidateCanExecuteCommands();
            }
        }

        private async Task WarmUp(WarmupType warmupType)
        {
            if (IsWarmingUp)
                return;

            WarmupResult warmupResult = WarmupResult.Failed;
            try
            {
                IsWarmingUp = true;

                ValidateCanExecuteCommands();

                var activeHeadId = 0L;
                var activeHead = ActiveHeadProvider.ActiveHead;
                if (activeHead != null)
                    activeHeadId = activeHead.Id;

                await WarmupService.RunSafeWarmupAsync(
                    (warmupType == WarmupType.Fast) 
                        ? WarmupParameters.FastWarmup(WarmupSettings.WarmupSetpoint, activeHeadId) 
                        : WarmupParameters.Conditioning(WarmupSettings.ConditioningSetpoint, activeHeadId));
                warmupResult = WarmupResult.Done;
            }
            catch (TaskCanceledException ex)
            {
                warmupResult = WarmupResult.Cancelled;
                _ = LogWriter.LogAsync(
                    $"{StringConstants.TreatmentConsole.FullWarmupEventDialogTitle}: interrupted. {ex?.Message}", 
                    LogRecordSeverity.Info, LogRecordType.System);
            }
            catch (DataServiceException ex)
            {
                warmupResult = WarmupResult.DbError;
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.FullWarmupEventDialogTitle,
                    StringConstants.TreatmentConsole.FullWarmupSaveToDbFailedError,
                    ex);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.FullWarmupEventDialogTitle,
                    StringConstants.TreatmentConsole.FullWarmupFailedError,
                    ex);
            }
            finally
            {
                IsWarmingUp = false;
                UIStateMachine.OnWarmupAction(DailyWarmUpUiStateMachine.Action.None);

                ActionAuditService.RegisterAction(
                    actionDescription: (warmupType == WarmupType.Fast) ? "Warmup" : "Conditioning",
                    actionDetails: warmupResult.ToString());

                ValidateCanExecuteCommands();
            }
        }

        private bool CanStartWarmUp()
        {
            return !IsWarmingUp
                   && _state != GcbStateNew.NoComm
                   && (MainBoardModel.CanStartWarmUp() || _state == GcbStateNew.Startup);
        }

        private void ValidateCanExecuteCommands()
        {
            FullWarmupCommand.RaiseCanExecuteChanged();
            WarmupCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        // TODO: refactor later, TreatmentViewModel has the same
        #endregion Private methods

        enum WarmupResult
        {
            Done,
            Failed,
            Cancelled,
            DbError
        }
    }
}
