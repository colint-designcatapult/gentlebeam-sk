using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;

namespace Xcc.Application.UI.Mvvm
{
    public abstract class RegionViewModelBase(
        IRegionManager? regionManager, 
        IEventAggregator? eventAggregator = null,
        IDialogService? dialogService = null) 
        : ViewModelBase(eventAggregator, dialogService), INavigationAware, IConfirmNavigationRequest
    {
        #region Properties
        protected IRegionManager? RegionManager { get; } = regionManager;

        protected string? RegionName { get; private set; }
        #endregion Properties

        #region Commands
        private DelegateCommand? _exitCommand;
        public DelegateCommand? ExitCommand
        {
            get
            {
                return _exitCommand ??= new DelegateCommand(OnExit, CanExecuteExitCommand);
            }
        }

        protected virtual bool CanExecuteExitCommand()
        {
            return true;
        }

        protected virtual void OnExit()
        {
            if (RegionName is not null && RegionManager is not null)
            {
                RegionManager.Regions[RegionName].NavigationService.Journal.GoBack();
            }
        }
        #endregion Commands

        #region IConfirmNavigationRequest
        public virtual void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }
        #endregion IConfirmNavigationRequest

        #region INavigationAware
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            RegionName = navigationContext.NavigationService.Region.Name;
        }

        #endregion INavigationAware
    }
}
