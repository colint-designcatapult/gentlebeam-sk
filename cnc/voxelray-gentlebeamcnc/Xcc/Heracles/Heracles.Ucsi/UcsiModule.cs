using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.Storage;
using Heracles.Ucsi.ViewModels;
using Prism.Ioc;
using Prism.Modularity;
using Xcc.Core.Domain.GryphonBoard;
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
        // Register the connection factory for real UDP communication to bench
        containerRegistry.RegisterSingleton<IGcbCommandConnectionFactory, UcsiGcbCommandConnectionFactory>();
        // Use UCSI-specific communication service with independent cancellation
        // The service is not started in the factory - it will be started in OnInitialized
        containerRegistry.RegisterSingleton<IGcbCommunicationService>(container =>
        {
            var service = container.Resolve<UcsiGcbCommunicationService>();
            // NOTE: Do NOT call Start() here. It will be called in OnInitialized to ensure
            // proper initialization order. Starting here can cause timing issues.
            return service;
        });
        // Use UcsiLogBuffer as the ILogWriter implementation (already registered above)
        containerRegistry.RegisterSingleton<ILogWriter>(c => c.Resolve<UcsiLogBuffer>());
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
        try
        {
            // Start the GCB communication service receive loop so it can listen for responses
            var gcbComm = containerProvider.Resolve<IGcbCommunicationService>() as IRawUdpClient;
            if (gcbComm != null)
            {
                gcbComm.Start();
                System.Diagnostics.Debug.WriteLine("[UCSI] GCB Communication Service started successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[UCSI] ERROR: GcbCommunicationService could not be cast to IRawUdpClient");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UCSI] ERROR starting GCB Communication Service: {ex.Message}");
        }
    }
}
