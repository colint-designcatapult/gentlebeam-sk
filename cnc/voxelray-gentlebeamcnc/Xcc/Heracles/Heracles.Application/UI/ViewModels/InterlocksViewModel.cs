using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xcc.Core.Constants;
using Xcc.Core.Models;

namespace Heracles.Application.UI.ViewModels
{
    public class InterlocksViewModel : BindableBase
    {
        public InterlocksViewModel(IGCBDataStore gcbDataStore, IDialogService dialogService)
        {
            GcbDataStore = gcbDataStore;
            DialogService = dialogService;

            gcbDataStore.PropertyChanged += (s, e) =>
            {
                if (gcbDataStore.Interlocks is null || gcbDataStore.SystemTelemetry is null)
                {
                    SystemIsReady = null;
                }
                else
                {
                    bool systemReady = true;
                    systemReady &= gcbDataStore.Interlocks.BaseKey;
                    systemReady &= gcbDataStore.Interlocks.RemoteKey;
                    systemReady &= gcbDataStore.Interlocks.DoorOpened;
                    systemReady &= gcbDataStore.Interlocks.BaseEStopEngaged;
                    systemReady &= gcbDataStore.Interlocks.RemoteEStopEngaged;
                    systemReady &= gcbDataStore.Interlocks.Timer1Expired;
                    systemReady &= gcbDataStore.Interlocks.Timer2Expired;
                    systemReady &= gcbDataStore.Interlocks.WaterLevel;
                    systemReady &= gcbDataStore.Interlocks.HeadInterfaceBoard;
                    systemReady &= gcbDataStore.Interlocks.HVPS;
                    systemReady &= gcbDataStore.Interlocks.CoolerAlarm;
                    systemReady &= gcbDataStore.Interlocks.Watchdog;
                    systemReady &= gcbDataStore.Interlocks.IonPumpHV;
                    SystemIsReady = systemReady;
                }
            };
        }

        public IGCBDataStore GcbDataStore { get; }
        public IDialogService DialogService { get; }

        public string SystemInterlockText => SystemIsReady is null or false ? StringConstants.SystemNotReadyUiMessage : StringConstants.SystemReadyUiMessage;

        private DelegateCommand? _showInterlocks;
        public DelegateCommand ShowInterlocksCommand => _showInterlocks ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("InterlocksDialogView");
            });


        private bool? _systemIsReady;
        public bool? SystemIsReady
        {
            get => _systemIsReady;
            set
            {
                if (SetProperty(ref _systemIsReady, value))
                {
                    RaisePropertyChanged(nameof(SystemInterlockText));
                }
            }
        }
    }
}
