using Heracles.Application.AppLayer.QualityAssurance.QualityCheck.Events;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.UI;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class BeamQaSelectorViewModel : QaTabViewModelBase
    {
        #region Contructors
        public BeamQaSelectorViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IDialogService dialogService)
            :base(regionManager, eventAggregator, dialogService)
        {
            RegionManager = regionManager;

            // Switch to Reports view when QC is finished
            //eventAggregator.GetEvent<QualityCheckFinishedEvent>().Subscribe(() => SwitchView = false);
        }
        #endregion Contructors

        
        #region Properties
        public IRegionManager RegionManager { get; }

        bool? _switchView;
        /// <summary>
        /// Bounded to the ToggleButton.IsChecked in the UI to switch UI between history of checks and daily check.
        /// </summary>
        public bool? SwitchView
        {
            get => _switchView;
            set
            {
                if (SetProperty(ref _switchView, value) && value is not null)
                {
                    if (_switchView.Value)
                    {
                        RegionManager.RequestNavigate(Regions.External.QualityAssurance.QualityChecksViewRegion, "BeamQaView");
                    }
                    else
                    {
                        RegionManager.RequestNavigate(Regions.External.QualityAssurance.QualityChecksViewRegion, "BeamQaReportsView");
                    }
                }
            }
        }

        #endregion Properties


        #region INavigationAware
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            SwitchView = true;
        }

        //public override bool IsNavigationTarget(NavigationContext navigationContext) => true;
        //public override void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion INavigationAware
    }
}
