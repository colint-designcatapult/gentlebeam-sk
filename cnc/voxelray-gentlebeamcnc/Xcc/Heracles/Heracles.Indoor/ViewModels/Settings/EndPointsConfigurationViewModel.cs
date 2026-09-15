using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Models;
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
            ILogWriter logWriter,
            IActionAuditService actionAuditService)
        {
            SettingsModel = settingsModel;
            PopUpService = popupService;
            LogWriter = logWriter;
            ActionAuditService = actionAuditService;
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
                    var before = new EndPointsConfiguration(SettingsModel.Settings.EndPointsConfiguration);
                    var requestedFields = ChangedEndPoints(before, EndPointsConfiguration);
                    if (requestedFields.Count == 0)
                    {
                        EndPointsConfiguration.AcceptChanges();
                        return;
                    }
                    var saved = await SettingsModel.SubmitSettingsAsync(new SystemSettings(SettingsModel.Settings)
                    {
                        EndPointsConfiguration = new EndPointsConfiguration(EndPointsConfiguration)
                    });
                    var changedFields = ChangedEndPoints(before, saved.EndPointsConfiguration);
                    if (changedFields.Count > 0)
                        ActionAuditService.RegisterAction("Configuration saved",
                            $"Entity=SystemSettings; Id={saved.Id}; Fields={string.Join(",", changedFields)}");
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
        private IActionAuditService ActionAuditService { get; }
        #endregion Commands

        #region Private methods
        private static List<string> ChangedEndPoints(EndPointsConfiguration before, Heracles.Core.Models.IEndPointsConfiguration after)
        {
            var fields = new List<string>();
            AddChanges(before.DatabaseEndpoint, after.DatabaseEndpoint, nameof(before.DatabaseEndpoint), fields);
            AddChanges(before.TreatmentHeadCamEndPoint, after.TreatmentHeadCamEndPoint, nameof(before.TreatmentHeadCamEndPoint), fields);
            AddChanges(before.GCBTelemetryEndPoint, after.GCBTelemetryEndPoint, nameof(before.GCBTelemetryEndPoint), fields);
            AddChanges(before.GCBCommandsEndPoint, after.GCBCommandsEndPoint, nameof(before.GCBCommandsEndPoint), fields);
            return fields;
        }

        private static void AddChanges(ISystemEndPoint before, ISystemEndPoint after, string name, List<string> fields)
        {
            if (before.IPAddressPart1 != after.IPAddressPart1) fields.Add($"{name}.IPAddressPart1");
            if (before.IPAddressPart2 != after.IPAddressPart2) fields.Add($"{name}.IPAddressPart2");
            if (before.IPAddressPart3 != after.IPAddressPart3) fields.Add($"{name}.IPAddressPart3");
            if (before.IPAddressPart4 != after.IPAddressPart4) fields.Add($"{name}.IPAddressPart4");
            if (before.Port != after.Port) fields.Add($"{name}.Port");
        }

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
