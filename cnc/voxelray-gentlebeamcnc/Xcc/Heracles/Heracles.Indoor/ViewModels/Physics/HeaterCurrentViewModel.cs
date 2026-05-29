using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;

using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class HeaterCurrentViewModel : BindableBase
    {
        #region Constructors
        public HeaterCurrentViewModel()
        {
            Store = new HeaterCurrentStore()
            {
                CollimatorConfiguration = new CollimatorConfiguration 
                {
                    Type = TargetType.TargetType_50mm_SSD_13_Fields
                }
            };
        }

        public HeaterCurrentViewModel(
            IHeaterCurrentStore heaterCurrentStore,
            IPopUpService popUpService,
            ILogRepository logWriter)
        {
            Store = heaterCurrentStore;
            PopUpService = popUpService;
            LogWriter = logWriter;

            Store.IsValidChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();
            Store.IsModifiedChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();
        }
        #endregion Constructors


        #region Properties
        public IHeaterCurrentStore Store { get; }
        public IPopUpService PopUpService { get; }
        public ILogRepository LogWriter { get; }
        #endregion Properties


        #region Observable tasks
        private ObservableTask _currentHeaterCurrentTask;
        public ObservableTask CurrentHeaterCurrentTask
        {
            get => _currentHeaterCurrentTask;
            set => SetProperty(ref _currentHeaterCurrentTask, value);
        }

        private DelegateCommand? _retryHeaterCurrentCommand;
        public DelegateCommand RetryHeaterCurrentCommand
        {
            get => _retryHeaterCurrentCommand;
            set => SetProperty(ref _retryHeaterCurrentCommand, value);
        }

        private DelegateCommand? _cancelHeaterCurrentCommand;
        public DelegateCommand CancelHeaterCurrentCommand => _cancelHeaterCurrentCommand ??= new DelegateCommand(
            () =>
            {
                CurrentHeaterCurrentTask = null;
            });
        #endregion Observable tasks


        #region Commands
        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            SubmitHeaterCurrent,
            () => Store.IsModified && Store.IsValid);

        #endregion Commands


        private void SubmitHeaterCurrent()
        {
            RetryHeaterCurrentCommand = new DelegateCommand(() =>
            {
                CurrentHeaterCurrentTask = new ObservableTask(
                    SubmitHeaterCurrentAsync(), 
                    StringConstants.Physics.HeaterCurrentSaveErrorMessage);
            });
            RetryHeaterCurrentCommand.Execute();
        }

        private async Task SubmitHeaterCurrentAsync()
        {
            try
            {
                await Store.SubmitHeaterCurrentAsync();

                PopUpService.ShowMessage(
                    StringConstants.Common.SettingsDialogTitle, 
                    StringConstants.Common.RestartExternalOnSaveNotification, 
                    Xcc.Core.Enums.ReportType.Info);
            }
            catch (Exception ex)
            {
                LogWriter.Log(
                    $"{StringConstants.Physics.HeaterCurrentSaveErrorMessage} {ex.Message}. {ex.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.Error);
                throw;
            }
        }

    }
}
