using Heracles.Application.UI.Views;
using Prism.Mvvm;

namespace Heracles.Application.Test.Views;

[TestFixture]
internal sealed class MonitorViewTests
{
    [OneTimeSetUp]
    public void RegisterViewModelFactory()
    {
        ViewModelLocationProvider.Register<MonitorView>(() => new object());
        ViewModelLocationProvider.Register<InterlocksView>(() => new object());
        ViewModelLocationProvider.Register<Xcc.Application.Views.ServiceButtonsView>(() => new object());
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void Constructor_WhenSystemTelemetryIsUnavailable_DoesNotThrow()
    {
        var resources = System.Windows.Application.Current?.Resources
            ?? new System.Windows.Application().Resources;
        var constants = new System.Windows.ResourceDictionary
        {
            Source = new Uri(
                "pack://application:,,,/Xcc.Application;Component/UI/Resources/Constants.xaml",
                UriKind.Absolute),
        };
        resources.MergedDictionaries.Add(constants);

        try
        {
            Assert.DoesNotThrow(() => _ = new MonitorView());
        }
        finally
        {
            resources.MergedDictionaries.Remove(constants);
        }
    }
}
