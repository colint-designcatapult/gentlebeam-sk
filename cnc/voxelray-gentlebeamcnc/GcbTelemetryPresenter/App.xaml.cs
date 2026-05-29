using System.IO;
using System.Windows;
using Empyrean.Common.Infra.Settings;
using GcbTelemetryPresenter.AppLayer;
using GcbTelemetryPresenter.Views;
using Microsoft.Extensions.Configuration;
using Xcc.Application.Helpers;

namespace GcbTelemetryPresenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        public static IConfiguration? Configuration { get; private set; }
        
        /// <summary>
        /// Called after application starts
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // In shell launcher mode, it starts with 'C:/Windows/system32' as a current directory
            // so we need to change this
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(currentDir);

            CultureInfoHelper.SetCurrentCulture();

            base.OnStartup(e);
        }

        public async void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.GetContainer().AddExtension(new Diagnostic());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(typeof(IConfiguration), AddConfiguration());

            containerRegistry.RegisterSingleton<IAppSettings, AppSettings>();
            containerRegistry.RegisterSingleton<ISettingsReader, SettingsReader>();
        }
        private IConfiguration AddConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }

        /// <summary>
        /// Called after ConfigureModuleCatalog. RegisterTypes methods for modules listed in the ConfigureModuleCatalog called after this one
        /// </summary>
        protected override Window CreateShell() => Container.Resolve<PresenterView>();
    }
}
