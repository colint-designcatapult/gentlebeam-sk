using System.Windows;
using Xcc.Shared.Views;

namespace Xcc.Test.Xcc.Shared.Views
{
    public class ViewsTests
    {
        [OneTimeSetUp]
        public void SetUpResourceDictionaries()
        {
            // Ensure an Application instance exists for WPF resource resolution
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }

            // Load the required resource dictionaries
            var colorResourcesUri = new Uri("pack://application:,,,/Xcc.Application;component/UI/Resources/ColorResources.xaml");
            var defaultStylesUri = new Uri("pack://application:,,,/Xcc.Styles;component/Styles/DefaultStylesDictionary.xaml");
            var fontSizeUri = new Uri("pack://application:,,,/Xcc.Styles;component/Styles/FontSizeDefault.xaml");

            try
            {
                var colorResources = new ResourceDictionary { Source = colorResourcesUri };
                var defaultStyles = new ResourceDictionary { Source = defaultStylesUri };
                var fontResources = new ResourceDictionary { Source = fontSizeUri };

                System.Windows.Application.Current.Resources.MergedDictionaries.Add(colorResources);
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(defaultStyles);
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(fontResources);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load resource dictionaries for View tests.", ex);
            }
        }

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