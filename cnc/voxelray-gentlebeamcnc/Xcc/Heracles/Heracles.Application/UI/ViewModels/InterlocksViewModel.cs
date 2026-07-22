using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Heracles.Application.UI.ViewModels
{
    public class InterlocksViewModel : BindableBase
    {
        public InterlocksViewModel(IGCBDataStore gcbDataStore, IDialogService dialogService)
        {
            GcbDataStore = gcbDataStore;
            DialogService = dialogService;

            UpdateSystemReadiness(gcbDataStore.SystemTelemetry);
            gcbDataStore.PropertyChanged += (s, e) =>
                UpdateSystemReadiness(gcbDataStore.SystemTelemetry);
        }

        private void UpdateSystemReadiness(ISystemTelemetry? telemetry) =>
            SystemIsReady = telemetry?.IsSystemReady();

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
