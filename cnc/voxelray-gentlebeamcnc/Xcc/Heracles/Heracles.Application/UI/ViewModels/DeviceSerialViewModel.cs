using Heracles.Application.Models.Settings;
using Prism.Commands;
using Xcc.Application.UI.Mvvm;
using Xcc.Application.AppLayer.Service;

namespace Heracles.Application.UI.ViewModels
{
    public class DeviceSerialViewModel : DialogViewModelBase
    {
        #region Contructors
        public DeviceSerialViewModel()
        {
            Title = "Device Serial ID";
        }

        public DeviceSerialViewModel(ISettingsModel settingsModel, IActionAuditService actionAuditService)
        {
            Title = "Device Serial ID";
            SettingsModel = settingsModel;
            ActionAuditService = actionAuditService;
        }
        #endregion Contructors


        #region Properties
        private string _deviceSerialId = "123-456-789";
        public string DeviceSerialId
        {
            get => _deviceSerialId;
            set
            {
                SetProperty(ref _deviceSerialId, value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion Properties


        #region Commands
        private DelegateCommand? _acceptCommand;
        public DelegateCommand AcceptCommand => _acceptCommand ??= new DelegateCommand(
            async () =>
            {
                var settings = new SystemSettings(
                    (SettingsModel.Settings is null) ? await SettingsModel.FetchSettingsAsync() : SettingsModel.Settings);
                var previousSerial = settings.DeviceSerial;
                if (previousSerial == DeviceSerialId)
                {
                    CloseDialog();
                    return;
                }
                settings.DeviceSerial = DeviceSerialId;
                var updatedSettings = await SettingsModel.SubmitSettingsAsync(settings);
                if (updatedSettings.DeviceSerial != previousSerial)
                    ActionAuditService.RegisterAction("Configuration saved",
                        $"Entity=SystemSettings; Id={updatedSettings.Id}; Fields=DeviceSerial");
                if (updatedSettings.DeviceSerial == DeviceSerialId)
                {
                    CloseDialog();
                }
            },
            () => string.IsNullOrWhiteSpace(DeviceSerialId) == false);

        public ISettingsModel SettingsModel { get; }
        private IActionAuditService ActionAuditService { get; }
        #endregion Commands


        #region Private methods
        #endregion Private methods
    }
}
