using Heracles.Application.Models.Settings;
using Prism.Commands;
using Xcc.Application.UI.Mvvm;

namespace Heracles.Application.UI.ViewModels
{
    public class DeviceSerialViewModel : DialogViewModelBase
    {
        #region Contructors
        public DeviceSerialViewModel()
        {
            Title = "Device Serial ID";
        }

        public DeviceSerialViewModel(ISettingsModel settingsModel)
        {
            Title = "Device Serial ID";
            SettingsModel = settingsModel;
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
                settings.DeviceSerial = DeviceSerialId;
                var updatedSettings = await SettingsModel.SubmitSettingsAsync(settings);
                if (updatedSettings.DeviceSerial == DeviceSerialId)
                {
                    CloseDialog();
                }
            },
            () => string.IsNullOrWhiteSpace(DeviceSerialId) == false);

        public ISettingsModel SettingsModel { get; }
        #endregion Commands


        #region Private methods
        #endregion Private methods
    }
}
