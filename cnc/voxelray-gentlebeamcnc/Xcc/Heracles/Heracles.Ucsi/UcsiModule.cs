using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.Storage;
using Heracles.Ucsi.ViewModels;
using Prism.Ioc;
using Prism.Modularity;

namespace Heracles.Ucsi;

public static class UcsiRegistration
{
    public static void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<TelemetryParameterCatalog>();
        containerRegistry.RegisterSingleton<TelemetryHistoryBuffer>();
        containerRegistry.RegisterSingleton<ParquetTelemetrySessionWriter>();
        containerRegistry.RegisterSingleton<ParquetTelemetrySessionReader>();
        containerRegistry.RegisterManySingleton<TelemetrySessionCoordinator>();
        containerRegistry.RegisterSingleton<UcsiLogBuffer>();
        containerRegistry.RegisterSingleton<IUcsiHostCommands, UnavailableUcsiHostCommands>();
        containerRegistry.RegisterSingleton<UnifiedCalibrationServiceViewModel>();
    }
}

public sealed class UcsiModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry) =>
        UcsiRegistration.RegisterTypes(containerRegistry);

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
