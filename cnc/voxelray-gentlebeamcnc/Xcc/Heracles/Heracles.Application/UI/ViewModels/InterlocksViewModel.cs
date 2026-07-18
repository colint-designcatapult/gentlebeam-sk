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
                var interlocks = gcbDataStore.SystemTelemetry?.Interlocks;
                if (interlocks is null)
                {
                    SystemIsReady = null;
                }
                else
                {
                    SystemIsReady =
                        interlocks.Value.CollimatorOn == true
                        && interlocks.Value.RemoteKeyOn == true
                        && interlocks.Value.DoorClosed == true
                        && interlocks.Value.BaseEStopReleased == true
                        && interlocks.Value.RemoteEStopReleased == true
                        && interlocks.Value.Timer1Ready == true
                        && interlocks.Value.Timer2Ready == true
                        && interlocks.Value.WaterLevelOk == true
                        && interlocks.Value.HeadInterfaceBoardReady == true
                        && interlocks.Value.HvpsReady == true
                        && interlocks.Value.CoolerReady == true
                        && interlocks.Value.WatchdogReady == true
                        && interlocks.Value.IonPumpOk == true;
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
