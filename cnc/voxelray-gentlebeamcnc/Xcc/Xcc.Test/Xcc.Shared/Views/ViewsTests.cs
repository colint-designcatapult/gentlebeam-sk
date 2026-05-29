using Xcc.Shared.Views;

namespace Xcc.Test.Xcc.Shared.Views
{
    public class ViewsTests
    {
        [Apartment(ApartmentState.STA)]
        [TestCase(typeof(CreateFilterDialogView))]
        [TestCase(typeof(DatePickerDialogView))]
        [TestCase(typeof(DialogBoxView))]
        [TestCase(typeof(ReportView))]
        [TestCase(typeof(TelemetryDialogView))]
        //[TestCase(typeof(EnterStringDialogView))]
        public void Ctor_DoesNotThrow(Type viewType)
        {
            Assert.DoesNotThrow(() =>
            {
                var instance = Activator.CreateInstance(viewType);
                Assert.That(instance, Is.Not.Null);
            });
        }
        
        [Apartment(ApartmentState.STA)]
        [TestCase(typeof(FaultsView))]
        [TestCase(typeof(LogView))]
        //[TestCase(typeof(SystemSignalsView))]
        public void Ctor_DoesNotThrow_VM_NoParametrelessCtor(Type viewType)
        {
            // Some view models do not have parameterless constructors,
            // which causes InitializeComponent() to throw an exception.
            // For testing purposes, we override the ViewModel factory to return null.
            Prism.Mvvm.ViewModelLocationProvider.SetDefaultViewModelFactory(_ => null);
            
            Assert.DoesNotThrow(() =>
            {
                var instance = Activator.CreateInstance(viewType);
                Assert.That(instance, Is.Not.Null);
            });
        }
    }
}