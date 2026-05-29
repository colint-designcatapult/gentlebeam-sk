using System;
using System.Threading.Tasks;
using Heracles.Application.Models.CollimatorConfiguration;
using Prism.Commands;
using Prism.Mvvm;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class MagnetometerViewModel : BindableBase
    {
        #region Constructors
        public MagnetometerViewModel()
        {
            Store = new MagnetometerCorrectionsStore()
            {
                CollimatorConfiguration = null
            };
        }

        public MagnetometerViewModel(
            IMagnetometerCorrectionsStore magnetometerCorrectionsStore,
            IPopUpService popUpService,
            ILogWriter logWriter)
        {
            Store = magnetometerCorrectionsStore;
            PopUpService = popUpService;
            LogWriter = logWriter;

            Store.IsValidChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();
            Store.IsModifiedChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();
        }
        #endregion Constructors


        #region Properties
        public IMagnetometerCorrectionsStore Store { get; }
        public IPopUpService PopUpService { get; }
        public ILogWriter LogWriter { get; }
        #endregion Properties


        #region Observable tasks
        private ObservableTask? _currentMagnetometerTask;
        public ObservableTask? CurrentMagnetometerTask
        {
            get => _currentMagnetometerTask;
            set => SetProperty(ref _currentMagnetometerTask, value);
        }

        private DelegateCommand? _retryMagnetometerCommand;
        public DelegateCommand? RetryMagnetometerCommand
        {
            get => _retryMagnetometerCommand;
            set => SetProperty(ref _retryMagnetometerCommand, value);
        }

        private DelegateCommand? _cancelMagnetometerCommand;
        public DelegateCommand CancelMagnetometerCommand => _cancelMagnetometerCommand ??= new DelegateCommand(
            () =>
            {
                CurrentMagnetometerTask = null;
            });
        #endregion Observable tasks


        #region Commands
        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            SubmitMagnetometerParameters,
            () => Store.IsValid && Store.IsModified);
        #endregion Commands

        private void SubmitMagnetometerParameters()
        {
            RetryMagnetometerCommand = new DelegateCommand(() =>
            {
                CurrentMagnetometerTask = new ObservableTask(
                    SubmitMagnetometerParametersAsync(), 
                    StringConstants.Physics.MagnetometerConfigurationSaveErrorMessage);
            });
            RetryMagnetometerCommand.Execute();
        }

        private async Task SubmitMagnetometerParametersAsync()
        {
            try
            {
                await Store.SubmitMagnetometerParametersAsync();
                PopUpService.ShowMessage(
                    StringConstants.Common.SettingsDialogTitle,
                    StringConstants.Common.RestartExternalOnSaveNotification,
                    Xcc.Core.Enums.ReportType.Info);
            }
            catch (Exception ex)
            {
                LogWriter.Log(
                    $"{StringConstants.Physics.MagnetometerConfigurationSaveErrorMessage} {ex.Message}. {ex.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.Error);
                throw;
            }
        }
    }
}
