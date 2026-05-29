using Heracles.Robot.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Heracles.Robot;

internal static class RegionNames
{
    public const string MainRegion = "MainRegion";
}

public class RobotModule(IRegionManager regionManager) : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        regionManager.RequestNavigate(RegionNames.MainRegion, "RobotTabsView");
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<RobotTabsView>();
        containerRegistry.RegisterForNavigation<RobotView>();
    }
}