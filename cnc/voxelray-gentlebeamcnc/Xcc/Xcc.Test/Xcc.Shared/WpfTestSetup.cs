using System.Windows;

namespace Xcc.Test.Xcc.Shared;

[SetUpFixture]
[Apartment(ApartmentState.STA)]
[SingleThreaded]
public class WpfTestSetup
{
    [OneTimeSetUp]
    public void CreateApplication()
    {
        // Keep the application and all descendant tests on the same STA thread.
        // Dispatcher.Invoke can then execute without an Application.Run message loop.
        new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
    }

    [OneTimeTearDown]
    public void ShutdownApplication()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
