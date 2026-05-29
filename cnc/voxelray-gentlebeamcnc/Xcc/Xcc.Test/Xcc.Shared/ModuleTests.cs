using Moq;
using Prism.Ioc;
using Xcc.Shared;
using Xcc.Shared.Views;

namespace Xcc.Test.Xcc.Shared
{
    public class ModuleTests
    {
        [Test]
        public void OnInitialized_DoesNotThrow()
        {
            var module = new Module();
            Assert.DoesNotThrow(() => module.OnInitialized(Mock.Of<IContainerProvider>()));
        }
        
        [Test]
        public void RegisterTypes()
        {
            var mockRegistry = new Mock<IContainerRegistry>();
            
            var sut = new Module();
            sut.RegisterTypes(mockRegistry.Object);
            
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(LogView), nameof(LogView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(ReportView), nameof(ReportView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(DialogBoxView), nameof(DialogBoxView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(FaultsView), nameof(FaultsView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(DatePickerDialogView), nameof(DatePickerDialogView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(EnterStringDialogView), nameof(EnterStringDialogView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(TelemetryDialogView), nameof(TelemetryDialogView)), Times.Once);
            mockRegistry.Verify(r => r.Register(typeof(object), typeof(CreateFilterDialogView), nameof(CreateFilterDialogView)), Times.Once);
        }
    }
}