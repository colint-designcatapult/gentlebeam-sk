using Heracles.Application.Events;
using Heracles.External.Models;

using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Core.Services;

namespace Heracles.External.ViewModels
{
    public class ExternalTabsViewModel : RegionViewModelBase
    {
        public ExternalTabsViewModel(
            IRegionManager regionManager,
            IGCBDataStore gcbDataStore,
            IEventAggregator eventAggregator,
            IExitingModel exitingModel,
            IDialogService dialogService,
            IUIStateMachine uIStateMachine,
            IAuthorizedUserStore authorizedUserStore,
            IMainBoardAPI mainBoardAPI,
            SystemService systemService,
            IPopUpService popUpService) : base(regionManager, eventAggregator, dialogService)
        {
            GcbDataStore = gcbDataStore;
            ExitingModel = exitingModel;
            UIStateMachine = uIStateMachine;
            AuthorizedUserStore = authorizedUserStore;
            MainBoardApi = mainBoardAPI;
            SystemService = systemService;
            PopUpService = popUpService;

            eventAggregator.GetEvent<RequestExternalTabChangeEvent>()
                .Subscribe(externalTabName => SelectedTabIndex = (int)externalTabName);

            AuthorizedUserStore.AuthorizedUserChanged += (_, user) =>
            {
                if (user is null)
                    SelectedTab = null;
            };
        }


        #region Injected Dependencies
        private IExitingModel ExitingModel { get; }
        public IUIStateMachine UIStateMachine { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public IMainBoardAPI MainBoardApi { get; }
        public SystemService SystemService { get; }
        public IPopUpService PopUpService { get; }
        public IGCBDataStore GcbDataStore { get; }  
        #endregion Injected Dependencies


        #region Properties
        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        private object? _selectedTab;
        public object? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }
        #endregion Properties


        private DelegateCommand? _exitApplicationCommand;
        public DelegateCommand ExitApplicationCommand => _exitApplicationCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.Report(
                    StringConstants.ConfirmExitHeader,
                    StringConstants.ConfirmExitMessage,
                    ReportType.Confirmation,
                    result =>
                    {
                        if (result.Result != ButtonResult.OK)
                            return;

                        ExitingModel.ExitApplication();
                    });
            });

        private DelegateCommand? _loginCommand;
        public DelegateCommand LoginCommand => _loginCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("LoginView");
            });



        private DelegateCommand? _aboutCommand;
        public DelegateCommand AboutCommand => _aboutCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    string versionMessage = await SystemService.GetSystemVersionInfo();

                    string? gcbVersion = await GetGcbVersion();
                    if (gcbVersion is not null)
                    {
                        versionMessage = $"{versionMessage}{Environment.NewLine}Main control board version: {gcbVersion}";
                    }

                    PopUpService.ShowMessage(
                        StringConstants.Common.SystemInfoTitle,
                        versionMessage,
                        ReportType.Info);
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(StringConstants.Common.SystemInfoTitle, StringConstants.Common.SystemInfoFailed, ex);
                }
            });


        private async Task<string?> GetGcbVersion()
        {
            try
            {
                var gcbVersionInfo = await MainBoardApi.GetVersionInfo();

                return $"{gcbVersionInfo.Major}.{gcbVersionInfo.Minor}.{gcbVersionInfo.Level}";
            }
            catch(Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.SystemInfoTitle,
                    StringConstants.TreatmentConsole.FailedToGetGcbVersion,
                    ex);
            }

            return null;
        }

        #region RegionViewModelBase
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            RegionManager.RequestNavigate(Regions.External.TreatmentRegion, "TreatmentView");
            RegionManager.RequestNavigate(Regions.External.QualityAssuranceRegion, "QaTabsView");
        }
        #endregion RegionViewModelBase
    }
}
