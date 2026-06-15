using Heracles.Application.Common;
using Heracles.Application.Models.CollimatorConfiguration;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Windows.Controls;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Application.UI.UserControls;
using Xcc.Core.Enums;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels
{
    public class MainTabsViewModel : RegionViewModelBase
    {
        public MainTabsViewModel(IAuthorizedUserStore authorizedUserStore,
            IRegionManager regionManager,
            IDialogService dialogService,
            IHeaterCurrentStore heaterCurrentStore,
            ICoilConfigurationStore coilConfigurationStore,
            IMagnetometerCorrectionsStore magnetometerCorrectionsStore,
            IOutputFactorConfigurationStore outputFactorConfigurationStore,
            IExitingModel exitingModel,
            IEventAggregator eventAggregator,
            SystemService systemService,
            IPopUpService popUpService) : base(regionManager, eventAggregator, dialogService)
        {
            ExitingModel = exitingModel;
            SystemService = systemService;
            PopUpService = popUpService;
            AuthorizedUserStore = authorizedUserStore;
            HeaterCurrentStore = heaterCurrentStore;
            CoilConfigurationStore = coilConfigurationStore;
            MagnetometerCorrectionsStore = magnetometerCorrectionsStore;
            OutputFactorConfigurationStore = outputFactorConfigurationStore;

            AuthorizedUserStore.AuthorizedUserChanged += (_, user) =>
            {
                if (user is null)
                    SelectedTab = null;
            };
        }

        #region Injected Dependencies
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public IHeaterCurrentStore HeaterCurrentStore { get; }
        public ICoilConfigurationStore CoilConfigurationStore { get; }
        public IMagnetometerCorrectionsStore MagnetometerCorrectionsStore { get; }
        public IOutputFactorConfigurationStore OutputFactorConfigurationStore { get; }
        private IExitingModel ExitingModel { get; }
        public SystemService SystemService { get; }
        public IPopUpService PopUpService { get; }

        #endregion Injected Dependencies


        #region Tabs-related properties
        TabItem? _selectedTab;
        public TabItem? SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        private DelegateCommand<Tuple<object, object>>? _tabChangePreventedCommand;
        public DelegateCommand<Tuple<object, object>> TabChangePreventedCommand => _tabChangePreventedCommand ??= new DelegateCommand<Tuple<object, object>>(
            tabs =>
            {
                var lockedTab = (XccTabItem)tabs.Item1; //only XccTabItem can be locked
                var desiredTab = (TabItem)tabs.Item2;

                string message = StringConstants.MainTabs.TabSwitchUnsavedChangesWarning;

                if (DialogService.Confirmation(lockedTab.Tag.ToString(), message))
                {
                    lockedTab.SetPreventTabChangeCurrent(false);
                    SelectedTab = desiredTab;
                    lockedTab.SetPreventTabChangeCurrent(true);
                }
            });
        #endregion Tabs-related properties


        private DelegateCommand? _exitApplicationCommand;
        public DelegateCommand ExitApplicationCommand => _exitApplicationCommand ??= new DelegateCommand(
            () =>
            {
                if (DialogService.Confirmation(
                    StringConstants.MainTabs.ApplicationExitConfirmationTitle,
                    StringConstants.MainTabs.ApplicationExitConfirmationMessage))
                {
                    ExitingModel.ExitApplication();
                }
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

                    PopUpService.ShowMessage(
                            Xcc.Core.Constants.StringConstants.Common.SystemInfoTitle,
                            versionMessage,
                            ReportType.Info);
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        Xcc.Core.Constants.StringConstants.Common.SystemInfoTitle,
                        Xcc.Core.Constants.StringConstants.Common.SystemInfoFailed,
                        ex);
                }
            });
    }
}
