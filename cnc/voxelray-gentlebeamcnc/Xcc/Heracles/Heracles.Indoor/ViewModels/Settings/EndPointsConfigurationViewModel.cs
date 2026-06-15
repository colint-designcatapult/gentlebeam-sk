using System;
using System.Threading.Tasks;
using Heracles.Application.Common;
using Heracles.Application.Models.Settings;
using Prism.Commands;
using Prism.Mvvm;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Settings
{
    public class EndPointsConfigurationViewModel : BindableBase
    {
        #region Contructors
        public EndPointsConfigurationViewModel()
        {
            EndPointsConfiguration = new EndPointsConfiguration();
        }

        public EndPointsConfigurationViewModel(
            ISettingsModel settingsModel,
            IPopUpService popupService,
            ILogWriter logWriter)
        {
            SettingsModel = settingsModel;
            PopUpService = popupService;
            LogWriter = logWriter;
            EndPointsConfiguration = new EndPointsConfiguration(settingsModel.Settings.EndPointsConfiguration);

            SettingsModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SettingsModel.Settings))
                {
                    EndPointsConfiguration = new EndPointsConfiguration(SettingsModel.Settings.EndPointsConfiguration);
                }
            };

            //CurrentTask = FetchSettingsAsync();
        }

        #endregion Contructors

        #region Properties
        private EndPointsConfiguration _endPointsConfiguration;
        public EndPointsConfiguration EndPointsConfiguration { get => _endPointsConfiguration; private set => SetProperty(ref _endPointsConfiguration, value); }
        Task CurrentTask { get; set; }
        #endregion Properties

        #region Commands
        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    await SettingsModel.SubmitSettingsAsync(new SystemSettings(SettingsModel.Settings) { EndPointsConfiguration = EndPointsConfiguration });
                    PopUpService.ShowMessage(
                        StringConstants.SystemSettings.SettingsTitle,
                        StringConstants.SystemSettings.RestartOnSaveNotification,
                        Xcc.Core.Enums.ReportType.Info);
                }
                catch (Exception ex)
                {
                    PopUpService.ShowMessage(
                        StringConstants.Common.DatabaseErrorTitle,
                        StringConstants.SystemSettings.SettingsSaveErrorMessage,
                        Xcc.Core.Enums.ReportType.Error);
                    await LogWriter.LogAsync($"Settings update error: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
                }
            }).ObservesCanExecute(() => EndPointsConfiguration.IsModified);

        public ISettingsModel SettingsModel { get; }
        public IPopUpService PopUpService { get; }
        public ILogWriter LogWriter { get; }
        #endregion Commands

        #region Private methods
        private async Task FetchSettingsAsync()
        {
            try
            {
                await SettingsModel.FetchSettingsAsync();
            }
            catch (Exception ex)
            {
                await LogWriter.LogAsync($"Settings fetch error: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
            }
        }
        #endregion Private methods
    }
}
