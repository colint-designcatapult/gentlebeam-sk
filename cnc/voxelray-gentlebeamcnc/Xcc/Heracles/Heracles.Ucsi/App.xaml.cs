using System.IO;
using System.Windows;
using Heracles.Ucsi.Services;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using Prism.Unity;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;

namespace Heracles.Ucsi;

public partial class App : PrismApplication
{
    private StandaloneUcsiLifecycle? _lifecycle;

    protected override Window CreateShell()
    {
        try
        {
            _lifecycle = Container.Resolve<StandaloneUcsiLifecycle>();
            _lifecycle.Start();
            return Container.Resolve<MainWindow>();
        }
        catch (Exception exception)
        {
            System.Windows.MessageBox.Show(
                $"UCSI could not start its telemetry listener.\n\n{exception.Message}",
                "UCSI startup failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        IConfiguration configuration = BuildConfiguration();
        containerRegistry.RegisterInstance(configuration);
        containerRegistry.RegisterSingleton<IAppGlobals, AppGlobals>();
        containerRegistry.RegisterSingleton<IGCBDataStore, GCBDataStore>();

        var hub = new DecodedTelemetryFrameHub();
        containerRegistry.RegisterInstance(hub);
        containerRegistry.RegisterInstance<IDecodedTelemetryFrameSink>(hub);
        containerRegistry.RegisterInstance<IDecodedTelemetryFrameSource>(hub);

        UcsiRegistration.RegisterTypes(containerRegistry);
        var logBuffer = new UcsiLogBuffer();
        containerRegistry.RegisterInstance(logBuffer);
        containerRegistry.RegisterInstance<ILogWriter>(logBuffer);

        containerRegistry.RegisterSingleton<UcsiStandaloneTelemetryOptions>();
        containerRegistry.RegisterSingleton<IGcbTelemetryConnectionFactory, StandaloneTelemetryConnectionFactory>();
        containerRegistry.RegisterSingleton<ISystemTelemetryChanged, StandaloneSystemTelemetrySink>();
        containerRegistry.RegisterSingleton<ISystemTelemetryProcessor, SystemTelemetryProcessor>();
        containerRegistry.RegisterSingleton<ITelemetryService, GcbTelemetryService>();
        containerRegistry.RegisterSingleton<StandaloneUcsiLifecycle>();
    }

    protected override void OnExit(ExitEventArgs eventArgs)
    {
        try
        {
            _lifecycle?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        finally
        {
            base.OnExit(eventArgs);
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string? settingsArgument = Environment.GetCommandLineArgs()
            .FirstOrDefault(argument => argument.StartsWith("--appsettings=", StringComparison.OrdinalIgnoreCase));
        string selectedSettings = settingsArgument is null
            ? "appsettings.json"
            : settingsArgument[(settingsArgument.IndexOf('=') + 1)..];
        string settingsPath = Path.IsPathRooted(selectedSettings)
            ? selectedSettings
            : Path.Combine(basePath, selectedSettings);
        if (!File.Exists(settingsPath))
            throw new FileNotFoundException("The selected UCSI settings file does not exist.", settingsPath);

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(settingsPath, optional: false, reloadOnChange: false)
            .Build();
    }
}
