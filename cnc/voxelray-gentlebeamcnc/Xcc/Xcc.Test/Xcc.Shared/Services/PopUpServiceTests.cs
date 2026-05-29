using Moq;
using Prism.Services.Dialogs;
using Xcc.Application.Models;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Xcc.Shared.Services;
using Xcc.Shared.ViewModels;

namespace Xcc.Test.Xcc.Shared.Services
{
    public class PopUpServiceTests
    {
        private Mock<IDialogService> _dialogServiceMock = null!;
        private Mock<ILogWriter> _logWriterMock = null!;
        private PopUpService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
            
            _dialogServiceMock = new Mock<IDialogService>();
            _logWriterMock = new Mock<ILogWriter>();

            _sut = new PopUpService(_dialogServiceMock.Object, _logWriterMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            if (System.Windows.Application.Current != null) 
                System.Windows.Application.Current.Shutdown();
        }
        
        [Test]
        public void ShowDialog()
        {
            _sut.ShowDialog("Test Title");
            
            _dialogServiceMock.Verify(d =>
                d.ShowDialog("Test Title", 
                    It.IsAny<DialogParameters>(),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
        }
        
        [Test]
        public void ShowMessage_Call_ShowDialog()
        {
            _sut.ShowMessage("Test Title", "Test Message", ReportType.Info);

            _dialogServiceMock.Verify(d =>
                d.ShowDialog("ReportView", 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<Report>("Report").Header == "Test Title" &&
                        p.GetValue<Report>("Report").Message == "Test Message" &&
                        p.GetValue<Report>("Report").Type == ReportType.Info),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
        }
        
        [Test]
        public void LogAndShowMessage_Call_ShowDialog()
        {
            _sut.LogAndShowMessage("Test Title", "Test Message", ReportType.Info, LogRecordSeverity.Warn, LogRecordType.Security);

            _dialogServiceMock.Verify(d =>
                d.ShowDialog("ReportView", 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<Report>("Report").Header == "Test Title" &&
                        p.GetValue<Report>("Report").Message == "Test Message" &&
                        p.GetValue<Report>("Report").Type == ReportType.Info),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
            
            _logWriterMock.Verify(l =>
                l.LogAsync(
                    It.Is<string>(msg =>
                        msg.StartsWith("Test Title: Test Message")),
                    LogRecordSeverity.Warn,
                    LogRecordType.Security), Times.Once);
        }
        
        [Test]
        public void LogAndShowError_Call_ShowDialog_and_LogAsync_1()
        {
            _sut.LogAndShowError("Error Title", "Invalid error");
            
            _dialogServiceMock.Verify(d =>
                d.ShowDialog("ReportView", 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<Report>("Report").Header == "Error Title" &&
                        p.GetValue<Report>("Report").Message == "Invalid error" &&
                        p.GetValue<Report>("Report").Type == ReportType.Error),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
            
            _logWriterMock.Verify(l =>
                l.LogAsync(
                    It.Is<string>(msg =>
                        msg.StartsWith("Error Title: Invalid error")),
                    LogRecordSeverity.Error,
                    LogRecordType.Error), Times.Once);
        }
        
        [Test]
        public void LogAndShowError_Call_ShowDialog_and_LogAsync_2()
        {
            var ex = new InvalidOperationException("Some error");
            _sut.LogAndShowError("Error Title", "Invalid error", ex);
            
            _dialogServiceMock.Verify(d =>
                d.ShowDialog("ReportView", 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<Report>("Report").Header == "Error Title" &&
                        p.GetValue<Report>("Report").Message == "Invalid error" &&
                        p.GetValue<Report>("Report").Type == ReportType.Error),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
            
            _logWriterMock.Verify(l =>
                l.LogAsync(
                    It.Is<string>(msg =>
                        msg.StartsWith("Error Title: Invalid error") &&
                        msg.Contains("Some error")),
                    LogRecordSeverity.Error,
                    LogRecordType.Error), Times.Once);
        }
        
        [Test]
        public void LogAndShowError_Call_ShowDialog_and_LogAsync_3()
        {
            var ex = new InvalidOperationException("Some error", new Exception("Inner"));
            _sut.LogAndShowError("Error Title", "Invalid error", ex);

            _dialogServiceMock.Verify(d =>
                d.ShowDialog("ReportView", 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<Report>("Report").Header == "Error Title" &&
                        p.GetValue<Report>("Report").Message == "Invalid error" &&
                        p.GetValue<Report>("Report").Type == ReportType.Error),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
            
            _logWriterMock.Verify(l =>
                l.LogAsync(
                    It.Is<string>(msg =>
                        msg.StartsWith("Error Title: Invalid error") &&
                        msg.Contains("Some error") &&
                        msg.Contains("Inner")),
                    LogRecordSeverity.Error,
                    LogRecordType.Error), Times.Once);
        }
        
        [Test]
        public void YesNoDialog(
            [Values(ButtonResult.Yes, ButtonResult.No)] ButtonResult buttonResult,
            [Values("Text on yes button")] string yesButtonText,
            [Values("Text on no button")] string noButtonText)
        {
            var expectedResult = (DialogBoxResult)buttonResult;
            
            _dialogServiceMock
                .Setup(d => d.ShowDialog(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<Action<IDialogResult>>()))
                .Callback<string, IDialogParameters, Action<IDialogResult>>((_, _, cb) =>
                {
                    var resultMock = new Mock<IDialogResult>();
                    resultMock.Setup(r => r.Result).Returns(buttonResult);
                    cb(resultMock.Object);
                });

            var title = "dialog title";
            var message = "Message?";
            var result = _sut.YesNoDialog(title, message, yesButtonText, noButtonText);
            Assert.That(result, Is.EqualTo(expectedResult));
            
            _dialogServiceMock.Verify(d =>
                d.ShowDialog(It.IsAny<string>(), 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<string>("DialogTitle") == title &&
                        p.GetValue<string>("DialogMessage") == message &&
                        p.GetValue<DialogBoxButton>("LeftButton").Result == ButtonResult.Yes &&
                        p.GetValue<DialogBoxButton>("LeftButton").Text == yesButtonText &&
                        p.GetValue<DialogBoxButton>("CentralButton").Result == ButtonResult.No &&
                        p.GetValue<DialogBoxButton>("CentralButton").Text == noButtonText),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
        }
        
        [Test]
        public void YesNoCancelDialog([Values(ButtonResult.Yes, ButtonResult.No, ButtonResult.Cancel)] ButtonResult buttonResult,
            [Values("Text on yes button")] string yesButtonText,
            [Values("Text on no button")] string noButtonText,
            [Values("Text on cancel button")] string cancelButtonText)
        {
            var expectedResult = (DialogBoxResult)buttonResult;
            
            _dialogServiceMock
                .Setup(d => d.ShowDialog(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<Action<IDialogResult>>()))
                .Callback<string, IDialogParameters, Action<IDialogResult>>((_, _, cb) =>
                {
                    var resultMock = new Mock<IDialogResult>();
                    resultMock.Setup(r => r.Result).Returns(buttonResult);
                    cb(resultMock.Object);
                });
            
            var title = "dialog title";
            var message = "Message?";
            var result = _sut.YesNoCancelDialog(title, message, yesButtonText, noButtonText, cancelButtonText);
            Assert.That(result, Is.EqualTo(expectedResult));
            
            _dialogServiceMock.Verify(d =>
                d.ShowDialog(It.IsAny<string>(), 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<string>("DialogTitle") == title &&
                        p.GetValue<string>("DialogMessage") == message &&
                        p.GetValue<DialogBoxButton>("LeftButton").Result == ButtonResult.Yes &&
                        p.GetValue<DialogBoxButton>("LeftButton").Text == yesButtonText &&
                        p.GetValue<DialogBoxButton>("CentralButton").Result == ButtonResult.No &&
                        p.GetValue<DialogBoxButton>("CentralButton").Text == noButtonText &&
                        p.GetValue<DialogBoxButton>("RightButton").Result == ButtonResult.Cancel &&
                        p.GetValue<DialogBoxButton>("RightButton").Text == cancelButtonText),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
        }
        
        [Test]
        public void YesCancelDialog([Values(ButtonResult.Yes, ButtonResult.Cancel)] ButtonResult buttonResult,
            [Values("Text on yes button")] string yesButtonText,
            [Values("Text on cancel button")] string cancelButtonText)
        {
            var expectedResult = (DialogBoxResult)buttonResult;
            
            _dialogServiceMock
                .Setup(d => d.ShowDialog(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<Action<IDialogResult>>()))
                .Callback<string, IDialogParameters, Action<IDialogResult>>((_, _, cb) =>
                {
                    var resultMock = new Mock<IDialogResult>();
                    resultMock.Setup(r => r.Result).Returns(buttonResult);
                    cb(resultMock.Object);
                });
            
            var title = "dialog title";
            var message = "Message?";
            var result = _sut.YesCancelDialog(title, message, yesButtonText, cancelButtonText);
            Assert.That(result, Is.EqualTo(expectedResult));
            
            _dialogServiceMock.Verify(d =>
                d.ShowDialog(It.IsAny<string>(), 
                    It.Is<DialogParameters>(p => 
                        p.GetValue<string>("DialogTitle") == title &&
                        p.GetValue<string>("DialogMessage") == message &&
                        p.GetValue<DialogBoxButton>("LeftButton").Result == ButtonResult.Yes &&
                        p.GetValue<DialogBoxButton>("LeftButton").Text == yesButtonText &&
                        p.GetValue<DialogBoxButton>("CentralButton").Result == ButtonResult.Cancel &&
                        p.GetValue<DialogBoxButton>("CentralButton").Text == cancelButtonText),
                    It.IsAny<Action<IDialogResult>>()), Times.Once);
        }
    }
}