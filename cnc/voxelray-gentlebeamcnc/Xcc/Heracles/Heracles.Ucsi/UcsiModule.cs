using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.Storage;
using Heracles.Ucsi.ViewModels;
using Microsoft.Extensions.Configuration;
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
        
        // Direct HVPS UART communication interface for system configuration
        // Read COM port from configuration: Ucsi:Hardware:HvpsUartPort (default: COM1)
        // Initialize eagerly on startup (not lazy) to fetch system config values from HVPS on launch
        containerRegistry.RegisterSingleton<IUcsiHvpsUartCommandInterface>(container =>
        {
            var config = container.Resolve<IConfiguration>();
            var logWriter = container.Resolve<ILogWriter>();
            string portName = config.GetValue<string>("Ucsi:Hardware:HvpsUartPort") ?? "COM1";
            var interface_ = new UcsiHvpsUartCommandInterface(portName, logWriter);
            // Fire and forget - start initialization in background like UDP's Start()
            // Factory returns immediately without waiting, but initialization task runs on thread pool
            _ = interface_.InitializeAsync();
            return interface_;
        });
        
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
        
        containerRegistry.RegisterSingleton<SessionDataExportService>();
        containerRegistry.RegisterSingleton<UnifiedCalibrationServiceViewModel>();
    }
}

public sealed class UcsiModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry) =>
        UcsiRegistration.RegisterTypes(containerRegistry);

    public void OnInitialized(IContainerProvider containerProvider)
    {
        // HVPS UART initialization is started in the factory (fire-and-forget)
        // This method is required by IModule interface but intentionally empty
    }
}
