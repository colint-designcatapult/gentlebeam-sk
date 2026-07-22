using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Models;

namespace Xcc.Shared.ViewModels
{
    class FaultsViewModel : BindableBase, IDialogAware
    {
        public FaultsViewModel(
            IEventAggregator eventAggregator,
            ILogRepository logWriter,
            IGCBDataStore gcbDataStore)
        {
            LogWriter = logWriter;
            GcbDataStore = gcbDataStore;
            eventAggregator.GetEvent<FaultsChangedEvent>().Subscribe(OnFaultsChanged, ThreadOption.UIThread);
        }

        public FaultsViewModel(
            IMainBoardModel mainBoardModel,
            IEventAggregator eventAggregator,
            ILogRepository logWriter,
            IGCBDataStore gcbDataStore)
            : this(eventAggregator, logWriter, gcbDataStore)
        {
            MainBoardModel = mainBoardModel;
        }


        #region Properties
        public IMainBoardModel? MainBoardModel { get; }
        public ILogRepository LogWriter { get; }
        private IGCBDataStore GcbDataStore { get; }
        
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
                    if (MainBoardModel is not null)
                    {
                        await MainBoardModel.ClearFaults();
                        await GetFaults();
                    }
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
            () => MainBoardModel is not null);
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
                if (MainBoardModel is null)
                {
                    OnFaultsChanged(GcbDataStore.ActiveFaults);
                    return;
                }

                FaultSnapshot snapshot = await MainBoardModel.GetFaults();
                OnFaultsChanged(snapshot.Entries);
                foreach (FaultEntry faultEntry in snapshot.Entries)
                {
                    _ = LogWriter.LogAsync($"GCB Fault: {faultEntry}", LogRecordSeverity.Error, LogRecordType.Error);
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log($"Failed to get faults: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
        }
        private void OnFaultsChanged(IReadOnlyList<FaultEntry> faults)
        {
            Faults.Clear();
            foreach (FaultEntry fault in faults)
            {
                Faults.Add(fault);
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
