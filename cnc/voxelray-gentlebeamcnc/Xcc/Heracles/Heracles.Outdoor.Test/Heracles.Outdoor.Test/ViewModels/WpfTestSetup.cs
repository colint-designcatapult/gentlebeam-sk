using System.Windows;
using NUnit.Framework;

namespace Heracles.Outdoor.Test.ViewModels;

[SetUpFixture]
[Apartment(ApartmentState.STA)]
[SingleThreaded]
public class WpfTestSetup
{
    [OneTimeSetUp]
    public void CreateApplication()
    {
        // Keep Application and all descendant tests on the same STA thread for Dispatcher.Invoke.
        // NUnit owns and pumps the synchronization context used by asynchronous tests.
        new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
    }

    [OneTimeTearDown]
    public void ShutdownApplication()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
