using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.UI;

namespace Xcc.Application.ViewModels.TreatmentConsole.QualityAssurance
{
    public class SafetyCheckTabViewModel : QaTabViewModelBase
    {
        #region Contructors
        public SafetyCheckTabViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IDialogService dialogService)
            : base(regionManager, eventAggregator, dialogService)
        {
            eventAggregator.GetEvent<SafetyCheckSavedEvent>().Subscribe(() => SwitchView = false);
        }

        #endregion Contructors

        #region Properties

        bool? _switchView;
        /// <summary>
        /// Bounded to the ToggleButton.IsChecked in the UI to switch UI between history of checks and daily check.
        /// </summary>
        public bool? SwitchView 
        {
            get => _switchView;
            set
            {
                if (SetProperty(ref _switchView, value) && _switchView is not null)
                {
                    if (_switchView.Value)
                    {
                        RegionManager?.RequestNavigate(Regions.External.QualityAssurance.SafetyChecksViewRegion, "SafetyCheckView");
                    }
                    else
                    {
                        RegionManager?.RequestNavigate(Regions.External.QualityAssurance.SafetyChecksViewRegion, "SafetyCheckReportsView");
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
