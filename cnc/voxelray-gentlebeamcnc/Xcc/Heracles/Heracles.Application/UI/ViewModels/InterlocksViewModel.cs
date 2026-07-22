using Heracles.Application.AppLayer.Collimators;
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
        private readonly ICollimatorModel _collimatorModel;
        private readonly IApplicatorReadinessSource _applicatorReadinessSource;

        public InterlocksViewModel(
            ICollimatorModel collimatorModel,
            IApplicatorReadinessSource applicatorReadinessSource,
            IGCBDataStore gcbDataStore,
            IDialogService dialogService)
        {
            _collimatorModel = collimatorModel;
            _applicatorReadinessSource = applicatorReadinessSource;
            GcbDataStore = gcbDataStore;
            DialogService = dialogService;

            UpdateSystemReadiness(gcbDataStore.SystemTelemetry);
            gcbDataStore.PropertyChanged += (_, _) => UpdateSystemReadiness(gcbDataStore.SystemTelemetry);
            _collimatorModel.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(ICollimatorModel.ActiveCollimator) or nameof(ICollimatorModel.Collimators))
                    UpdateSystemReadiness(GcbDataStore.SystemTelemetry);
            };
            _applicatorReadinessSource.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(IApplicatorReadinessSource.CollimatorConfiguration))
                    UpdateSystemReadiness(GcbDataStore.SystemTelemetry);
            };
        }

        private void UpdateSystemReadiness(ISystemTelemetry? telemetry)
        {
            var applicatorReadiness = ApplicatorReadinessEvaluator.Evaluate(
                _collimatorModel,
                _applicatorReadinessSource.CollimatorConfiguration);
            SystemIsReady = telemetry?.IsSystemReady(applicatorReadiness == ApplicatorReadiness.Ready);
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
