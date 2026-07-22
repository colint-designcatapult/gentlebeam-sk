using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Application.Models;
using System.Collections.ObjectModel;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.External.ViewModels
{
    class FaultsViewModel : BindableBase, IDialogAware
    {
        public FaultsViewModel()
        {
        }

        public FaultsViewModel(
            IMainBoardModel mainBoardModel,
            IEventAggregator eventAggregator,
            ILogRepository logWriter)
        {
            MainBoardModel = mainBoardModel;
            eventAggregator.GetEvent<FaultsChangedEvent>().Subscribe(OnFaultsChanged, ThreadOption.UIThread);
            LogWriter = logWriter;
        }

        #region Properties
        public IMainBoardModel MainBoardModel { get; }
        public ILogRepository LogWriter { get; }
        
        public ObservableCollection<FaultEntry> Faults { get; } = [];
        ObservableTask? FetchFaultsTask { get; set; }
        #endregion Properties


        private DelegateCommand? _closeDialogCommand;
        public DelegateCommand CloseDialogCommand => _closeDialogCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            },
            canExecuteMethod: CanCloseDialog);

        private DelegateCommand? _clearErrorsCommand;
        public DelegateCommand ClearErrorsCommand => _clearErrorsCommand ??= new(
            async () =>
            {
                await MainBoardModel.ClearFaults();
                await GetFaults();
            },
            () => true);


        #region Private methods
        private async Task GetFaults()
        {
            try
            {
                FaultSnapshot snapshot = await MainBoardModel.GetFaults();
                OnFaultsChanged(snapshot.Entries);
                foreach (FaultEntry faultEntry in snapshot.Entries)
                {
                    _ = LogWriter.LogAsync($"GCB Fault: {faultEntry}", LogRecordSeverity.Error, LogRecordType.Error);
                }
            }
            catch (Exception ex)
            {
                _= LogWriter.LogAsync($"Failed to get faults: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
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

        #endregion Private methods


        #region IDialogAware
        public event Action<IDialogResult> RequestClose;

        public string Title { get; set; } = string.Empty;
        
        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) 
        {
            FetchFaultsTask = new ObservableTask(GetFaults());
        }
        #endregion IDialogAware
    }
}
