using Prism.Ioc;
using Prism.Modularity;
using Xcc.Application.Views;
using Xcc.Application.Views.TreatmentConsole.QualityAssurance;

namespace Xcc.Application
{
    public class ApplicationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider) {}

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<UserManagementView>();
            containerRegistry.RegisterForNavigation<UserRolesView>();
            containerRegistry.RegisterForNavigation<DailyWarmUpView>();
            containerRegistry.RegisterForNavigation<SafetyCheckTabView>();
        }
    }
}

