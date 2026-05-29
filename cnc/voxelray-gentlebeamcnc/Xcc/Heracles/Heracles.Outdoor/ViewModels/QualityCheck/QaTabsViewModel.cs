using Heracles.External.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.UI;
using Xcc.Core.Logging;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class QaTabsViewModel : QaTabViewModelBase
    {
        #region Contructors
        public QaTabsViewModel() : base(null, null, null) { }

        public QaTabsViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            IUIStateMachine uiStateMachine,
            ILogWriter logWriter,
            IAuthorizedUserStore authorizedUserStore) : base(regionManager, eventAggregator, dialogService)
        {
            UIStateMachine = uiStateMachine;
            LogWriter = logWriter;
            AuthorizedUserStore = authorizedUserStore;

            eventAggregator.GetEvent<RequestQaTabChangeEvent>()
                .Subscribe(tabName => SelectedTabIndex = (int)tabName);
        }
        #endregion Contructors


        #region Read-only properties
        public IUIStateMachine UIStateMachine { get; }
        public ILogWriter LogWriter { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }

        #endregion Read-only properties


        #region Properties
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        } 
        #endregion


        #region Commands
        private DelegateCommand? _getFaultsCommand;
        public DelegateCommand GetFaultsCommand => _getFaultsCommand ??= new DelegateCommand(
            () =>
            {
                try
                {
                    DialogService.ShowDialog("FaultsView");
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync($"Failed to get Faults: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
                }
            });

        private DelegateCommand? _showDetailedTelemetryCommand;
        public DelegateCommand ShowDetailedTelemetryCommand => _showDetailedTelemetryCommand ??= new DelegateCommand(
            () =>
            {
                try
                {
                    DialogService.ShowDialog("TelemetryDialogView");
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync($"Failed to show detailed telemetry: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
                }
            });

        private DelegateCommand? _showInterlocks;
        public DelegateCommand ShowInterlocksCommand => _showInterlocks ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("InterlocksDialogView");
            });
        #endregion Commands


        #region RegionViewModelBase
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            RegionManager.RequestNavigate(Regions.External.QualityAssurance.QualityChecksTabRegion, "BeamQaSelectorView");
            RegionManager.RequestNavigate(Regions.External.QualityAssurance.SafetyChecksTabRegion, "SafetyCheckTabView");
        }
        #endregion RegionViewModelBase
    }
}
