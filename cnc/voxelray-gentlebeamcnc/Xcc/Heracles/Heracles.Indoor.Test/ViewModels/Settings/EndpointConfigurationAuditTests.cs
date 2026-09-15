using System.ComponentModel;
using System.Threading;
using Heracles.Application.Models.Settings;
using Heracles.Application.UI.ViewModels;
using Heracles.Core.Models;
using Heracles.Indoor.ViewModels.Settings;
using Moq;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.Test.ViewModels.Settings;

[Apartment(ApartmentState.STA)]
internal sealed class EndpointConfigurationAuditTests
{
    [Test]
    public void EndpointSave_AuditsOnlyPersistedFieldNamesAndStableSettingsIdentity()
    {
        var fixture = CreateFixture();
        fixture.ViewModel.EndPointsConfiguration.GCBCommandsEndPoint.Port = 23456;
        fixture.ViewModel.SaveCommand.Execute();

        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("Id=9").And.Contain("Fields=GCBCommandsEndPoint.Port"));
        Assert.That(fixture.Records[0], Does.Not.Contain("23456").And.Not.Contain("172.31.1.100"));
        Assert.That(fixture.Model.Object.Settings.EndPointsConfiguration.GCBCommandsEndPoint.Port, Is.EqualTo(23456));
    }

    [Test]
    public void EndpointSave_UnchangedRevertedAndLocalOnlyChangesStaySilent()
    {
        var fixture = CreateFixture();
        fixture.ViewModel.SaveCommand.Execute();
        var originalPort = fixture.ViewModel.EndPointsConfiguration.GCBCommandsEndPoint.Port;
        fixture.ViewModel.EndPointsConfiguration.GCBCommandsEndPoint.Port = 23456;
        fixture.ViewModel.EndPointsConfiguration.GCBCommandsEndPoint.Port = originalPort;
        fixture.ViewModel.SaveCommand.Execute();
        // The record-and-verify endpoint is deliberately overridden by local constants in SettingsModel.
        fixture.ViewModel.EndPointsConfiguration.RecordAndVerifyEndPoint.Port = 34567;
        fixture.ViewModel.SaveCommand.Execute();

        Assert.That(fixture.Records, Is.Empty);
        fixture.Model.Verify(x => x.SubmitSettingsAsync(It.IsAny<ISystemSettings>()), Times.Never);
    }

    [Test]
    public void EndpointSave_FailedOrUnappliedChangesStaySilent()
    {
        var fixture = CreateFixture();
        fixture.Model.Setup(x => x.SubmitSettingsAsync(It.IsAny<ISystemSettings>()))
            .ThrowsAsync(new InvalidOperationException("Write failed"));
        fixture.ViewModel.EndPointsConfiguration.GCBCommandsEndPoint.Port = 23456;
        fixture.ViewModel.SaveCommand.Execute();
        Assert.That(fixture.Records, Is.Empty);

        fixture.Model.Setup(x => x.SubmitSettingsAsync(It.IsAny<ISystemSettings>()))
            .ReturnsAsync(fixture.Model.Object.Settings);
        fixture.ViewModel.SaveCommand.Execute();
        Assert.That(fixture.Records, Is.Empty);
    }

    [Test]
    public void EndpointInitializationAndReload_DoNotAuditAndPreserveDatabaseEndpoint()
    {
        var fixture = CreateFixture();
        fixture.Model.Object.Settings.EndPointsConfiguration.DatabaseEndpoint.Port = 12345;
        fixture.Model.Raise(x => x.PropertyChanged += null, new PropertyChangedEventArgs(nameof(ISettingsModel.Settings)));

        Assert.That(fixture.ViewModel.EndPointsConfiguration.DatabaseEndpoint.Port, Is.EqualTo(12345));
        Assert.That(fixture.Records, Is.Empty);
        fixture.ViewModel.SaveCommand.Execute();
        Assert.That(fixture.Records, Is.Empty);
        fixture.Model.Verify(x => x.SubmitSettingsAsync(It.IsAny<ISystemSettings>()), Times.Never);
    }

    [Test]
    public void DeviceSerialSave_AuditsChangeWithoutSerialValueAndNotUnchangedSave()
    {
        var fixture = CreateFixture();
        var viewModel = new DeviceSerialViewModel(fixture.Model.Object, fixture.Audit.Object) { DeviceSerialId = "device-secret-serial" };
        viewModel.AcceptCommand.Execute();
        viewModel.AcceptCommand.Execute();

        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("Id=9").And.Contain("Fields=DeviceSerial").And.Not.Contain("device-secret-serial"));
    }

    private static Fixture CreateFixture()
    {
        ISystemSettings settings = new SystemSettings { Id = 9 };
        var model = new Mock<ISettingsModel>();
        model.SetupGet(x => x.Settings).Returns(() => settings);
        model.Setup(x => x.SubmitSettingsAsync(It.IsAny<ISystemSettings>()))
            .ReturnsAsync((ISystemSettings value) => settings = new SystemSettings(value));
        var log = new Mock<ILogWriter>();
        log.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()))
            .Returns(Task.CompletedTask);
        var records = new List<string>();
        var audit = new Mock<IActionAuditService>();
        audit.Setup(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((action, details) => records.Add($"{action}: {details}"));
        var viewModel = new EndPointsConfigurationViewModel(model.Object, Mock.Of<IPopUpService>(), log.Object, audit.Object);
        return new Fixture(viewModel, model, audit, records);
    }

    private sealed record Fixture(EndPointsConfigurationViewModel ViewModel, Mock<ISettingsModel> Model,
        Mock<IActionAuditService> Audit, List<string> Records);
}
