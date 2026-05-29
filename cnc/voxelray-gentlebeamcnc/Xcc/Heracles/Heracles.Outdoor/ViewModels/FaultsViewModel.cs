using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
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

        public FaultsViewModel(IMainBoardModel mainBoardModel, ILogRepository logWriter)
        {
            MainBoardModel = mainBoardModel;
            LogWriter = logWriter;
        }

        #region Properties
        public IMainBoardModel MainBoardModel { get; }
        public ILogRepository LogWriter { get; }
        
        public ObservableCollection<FaultEntry> Faults { get; } = [];
        ObservableTask FetchFaultsTask { get; set; }
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
            () =>
            {
                MainBoardModel.ClearFaults();
                FetchFaultsTask = new ObservableTask(GetFaults());
            },
            () => true);


        #region Private methods
        private async Task GetFaults()
        {
            try
            {
                Faults.Clear();

                // In    current version GCB returns only last detailed fault.
                // Yoni said its ok to show only one fault in the list.
                var faultEntry = await MainBoardModel.GetFaults();
                if (faultEntry.FaultId != 0) // Id=0 isn't a fault
                {
                    Faults.Add(faultEntry);
                    _ = LogWriter.LogAsync($"GCB Fault: {faultEntry}", LogRecordSeverity.Error, LogRecordType.Error);
                }
            }
            catch (Exception ex)
            {
                _= LogWriter.LogAsync($"Failed to get faults: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
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
