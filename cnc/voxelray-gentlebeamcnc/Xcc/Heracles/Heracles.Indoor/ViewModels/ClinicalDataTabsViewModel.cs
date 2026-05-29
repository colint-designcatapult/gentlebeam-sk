using Prism.Events;
using Prism.Regions;

using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;

namespace Heracles.Indoor.ViewModels
{
    public class UnloadFromTreatmentEvent : PubSubEvent { }

    public class ClinicalDataTabsViewModel : RegionViewModelBase
    {
        public ClinicalDataTabsViewModel() : base(null) 
        {
        }

        public ClinicalDataTabsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager) 
        {
            eventAggregator.GetEvent<UnloadFromTreatmentEvent>().Subscribe(() => SelectedTabIndex = 1, ThreadOption.UIThread);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            RegionManager.RequestNavigate(Regions.Main.ClinicalData.PlanRegion, "PlanView");
            RegionManager.RequestNavigate(Regions.Main.ClinicalData.TreatmentsRegion, "TreatmentsView");
            RegionManager.RequestNavigate(Regions.Main.ClinicalData.ImagesRegion, "ImagesView");
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }
    }
}
