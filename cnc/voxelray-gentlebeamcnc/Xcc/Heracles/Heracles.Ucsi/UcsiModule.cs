using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.Storage;
using Heracles.Ucsi.ViewModels;
using Prism.Ioc;
using Prism.Modularity;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.CommandAPI;
using Xcc.Infra.GryphonBoard.Comm;
using Empyrean.Common.Infra.Networking.Udp;

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
        
        // GCB command interface infrastructure for HVPS calibration
        containerRegistry.RegisterSingleton<IGcbXRayCommandOperator, GcbXRayCommandOperator>();
        // Register command options from configuration
        containerRegistry.RegisterSingleton<UcsiStandaloneCommandOptions>();
        // Use UcsiLogBuffer as the ILogWriter implementation
        containerRegistry.RegisterSingleton<ILogWriter>(c => c.Resolve<UcsiLogBuffer>());
        // Register the connection factory for real UDP communication to bench
        containerRegistry.RegisterSingleton<IGcbCommandConnectionFactory, UcsiGcbCommandConnectionFactory>();
        // Use UCSI-specific communication service with independent cancellation
        // Start the receive task immediately in the factory so it runs before any commands are sent
        containerRegistry.RegisterSingleton<IGcbCommunicationService>(container =>
        {
            var service = container.Resolve<UcsiGcbCommunicationService>();
            (service as IRawUdpClient)?.Start();
            return service;
        });
        // Register the command interface (uses GcbCommandInterface which requires logWriter and communication service)
        containerRegistry.RegisterSingleton<IGcbCommandInterface, GcbCommandInterface>();
        
        containerRegistry.RegisterSingleton<UnifiedCalibrationServiceViewModel>();
    }
}

public sealed class UcsiModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry) =>
        UcsiRegistration.RegisterTypes(containerRegistry);

    public void OnInitialized(IContainerProvider containerProvider)
    {
        // Service is already started in the factory registration
        // This method is required by IModule interface but intentionally empty
    }
}
