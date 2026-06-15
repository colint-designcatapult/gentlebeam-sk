using FFmpeg.AutoGen;
using Heracles.Application.Models;
using Heracles.Application.Services;
using Heracles.Core.Models;
using Heracles.Robot.Models.Interlock;
using Heracles.Robot.Views;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Heracles.Robot.Models;
using Heracles.Robot.Services;
using Xcc.Application.Common;
using Xcc.Core.Logging;
using Xcc.Infra.Logging;

namespace Heracles.Robot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            // location of the ffmpeg binaries
            //Unosquare.FFME.Library.FFmpegDirectory = @"C:\ffmpeg";
            //Unosquare.FFME.Library.FFmpegDirectory = @"C:\Users\david\source\repos\ffmpeg-n4.4-latest-win64-gpl-shared-4.4\bin";

            //Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpegLoadMode.MinimumFeatures;
            //Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpegLoadMode.VideoOnly;
            // location of the ffmpeg binaries
            Unosquare.FFME.Library.FFmpegDirectory = @"C:\ffmpeg";

            //Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpegLoadMode.MinimumFeatures;
            Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpegLoadMode.VideoOnly;

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // services:
            containerRegistry.RegisterSingleton<Xcc.Core.Models.IAppGlobals, Application.Models.AppGlobals>();
            containerRegistry.RegisterManySingleton<HeraclesMainSettings>();
            containerRegistry.RegisterInstance(typeof(IConfiguration), AddConfiguration());

            containerRegistry.RegisterSingleton<ILogRepository, TextLogRepositoryAdapter>(); // Log service should be initialized before any other service, but after AppSettings
            //containerRegistry.RegisterSingleton<IRobotArmService, Heracles.Infra.Services.RobotArmGrpcService>();
            containerRegistry.RegisterSingleton<IInterlockService, KeyboardInterlock>();
            containerRegistry.RegisterSingleton<IRobotArmService, RobotArmGrpcWithInterlockService>();

            containerRegistry.RegisterSingleton<IAcbMessageConverter, AcbMessageConverter>();
            containerRegistry.RegisterSingleton<IAcbCommunicationService, AcbCommunicationService>();

            var appSettings = Container.Resolve<Heracles.Core.Models.IHeraclesMainSettings>();

            if (appSettings.UseDummyHeadActuators)
                containerRegistry.RegisterSingleton<IAcbService, DummyAcbService>();
            else
                containerRegistry.RegisterSingleton<IAcbService, AcbSafeService>();

            containerRegistry.RegisterManySingleton<Application.Models.SystemConfiguration>(
                typeof(Core.Models.ISystemConfiguration),
                typeof(Xcc.Core.Models.ISystemConfiguration));

            containerRegistry.RegisterSingleton<IWakeOnLanService, WakeOnLanService>();

            //containerRegistry.Register<IConfigurationService, ConfigurationService>();
            //// instances:
            //containerRegistry.RegisterInstance(typeof(IConfiguration), AddConfiguration());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            DisposeResources();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var defaultCulture = new CultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = defaultCulture;
            Thread.CurrentThread.CurrentUICulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            base.OnStartup(e);

            Container.Resolve<IWakeOnLanService>().WakeUpAsync();
        }

        private void DisposeResources()
        {
            if (Container.Resolve<IInterlockService>() is IDisposable interlockService)
                interlockService.Dispose();

            if (Container.Resolve<IRobotArmService>() is IDisposable disposable)
                disposable.Dispose();

            if (Container.Resolve<IAcbCommunicationService>() is IDisposable acbCommSevice)
                acbCommSevice.Dispose();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<RobotModule>();
            moduleCatalog.AddModule<Xcc.Shared.Module>();
        }

        protected IConfiguration AddConfiguration()
        {
            var appsettings = ApplicationArgs.GetAppSettings() ?? "appsettings.json";

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appsettings);

            return builder.Build();
        }
    }
}
