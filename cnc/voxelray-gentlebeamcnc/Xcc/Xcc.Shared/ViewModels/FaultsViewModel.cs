using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Prism.Events;
using Xcc.Application.Models;

namespace Xcc.Shared.ViewModels
{
    class FaultsViewModel : BindableBase, IDialogAware
    {
        public FaultsViewModel()
        {
            throw new Exception("Design-only constructor");
        }

        public FaultsViewModel(
            IMainBoardModel mainBoardModel, 
            IEventAggregator eventAggregator,
            ILogRepository logWriter)
        {
            MainBoardModel = mainBoardModel;
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnSystemTelemetryChanged);
            LogWriter = logWriter;
        }


        #region Properties
        public IMainBoardModel MainBoardModel { get; }
        public ILogRepository LogWriter { get; }
        
        public ObservableCollection<FaultEntry> Faults { get; } = [];
        ObservableTask? FetchFaultsTask { get; set; }

        private bool _isClearErrorsRunning = false;
        public bool IsClearErrorsRunning
        {
            get => _isClearErrorsRunning;
            set
            {
                if (SetProperty(ref _isClearErrorsRunning, value))
                {
                    //ClearErrorsCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion Properties

        #region Commands
        private DelegateCommand? _closeDialogCommand;
        public DelegateCommand CloseDialogCommand => _closeDialogCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            },
            () => CanCloseDialog());

        private DelegateCommand? _clearErrorsCommand;
        public DelegateCommand ClearErrorsCommand => _clearErrorsCommand ??= new(
            async () =>
            {
                try
                {
                    IsClearErrorsRunning = true;
                    await MainBoardModel.ClearFaults();
                    Faults.Clear();

                    ScheduleFaultUpdate();
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync(
                        $"ClearFaults error: failed to clear the faults. {ex.Message}",
                        LogRecordSeverity.Error,
                        LogRecordType.System);
                }
                finally
                {
                    IsClearErrorsRunning = false;
                }
            },
            () => true);//!IsClearErrorsRunning);
        #endregion Commands


        #region Private methods
        private void RunGetFaultsTask()
        {
            FetchFaultsTask = new ObservableTask(GetFaults());
        }

        private async Task GetFaults()
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    // In current version GCB returns only last detailed fault.
                    // Yoni said its ok to show only one fault in the list.
                    var faultEntry = await MainBoardModel.GetFaults();
                    // We could wait long enough to have race condition here,
                    // so clear the view just in case:
                    Faults.Clear();
                    if (faultEntry.FaultId != 0) // Id=0 isn't a fault
                    {
                        Faults.Add(faultEntry);
                        _= LogWriter.LogAsync($"GCB Fault: {faultEntry}", LogRecordSeverity.Error, LogRecordType.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                LogWriter.Log($"Failed to get faults: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
        }
        private void OnSystemTelemetryChanged(ISystemTelemetry? telemetry)
        {
            if (telemetry?.IsFaultState() == true 
                && Faults?.Count == 0)
            {
                ScheduleFaultUpdate();
            }
        }

        private void ScheduleFaultUpdate()
        {
            // If it either completed or didn't start yet, then we schedule a new task
            if (FetchFaultsTask?.IsCompleted != false)
            {
                RunGetFaultsTask();
            }
        }
        #endregion Private methods


        #region IDialogAware
        public event Action<IDialogResult>? RequestClose;

        public string Title { get; set; } = string.Empty;
        
        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) 
        {
            ScheduleFaultUpdate();
        }

        #endregion IDialogAware
    }
}
