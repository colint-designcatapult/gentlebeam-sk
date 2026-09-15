using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Moq;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Domain.System;
using Xcc.Core.Logging;

namespace Heracles.Indoor.Test.ViewModels.Settings;

internal sealed class CollimatorConfigurationAuditTests
{
    [Test]
    public async Task Create_ReportsCommittedConfigurationPresetAndApplicatorWithoutSerial()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.CreateCollimatorAsync("private-serial", TargetType.TargetType_50mm_SSD_15mm_Field,
            Energy.Energy_50, true, fixture.Audit.Object);

        Assert.That(created.Id, Is.EqualTo(33));
        Assert.That(fixture.Records, Has.Count.EqualTo(3));
        Assert.That(fixture.Records[0], Does.Contain("Entity=CollimatorConfiguration; Id=11"));
        Assert.That(fixture.Records[1], Does.Contain("Entity=PresetConfiguration; Id=22").And.Contain("IsDefault"));
        Assert.That(fixture.Records[2], Does.Contain("Entity=Collimator; Id=33"));
        Assert.That(fixture.Records.Any(x => x.Contains("private-serial")), Is.False);
        fixture.Records.Clear();

        await fixture.Service.UpdateCollimatorAsync("private-serial", TargetType.TargetType_50mm_SSD_15mm_Field,
            Energy.Energy_50, true, fixture.Audit.Object);
        Assert.That(fixture.Records, Is.Empty);
        await fixture.Service.UpdateCollimatorAsync("private-serial", TargetType.TargetType_50mm_SSD_15mm_Field,
            Energy.Energy_50, false, fixture.Audit.Object);
        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("Entity=Collimator; Id=33").And.Contain("Fields=IsActive"));
    }

    [Test]
    public void Create_WhenDefaultPresetFailsRetainsOnlyCommittedConfigurationAudit()
    {
        var fixture = CreateFixture();
        fixture.Presets.Setup(x => x.CreateAsync(It.IsAny<IPresetConfiguration>()))
            .ThrowsAsync(new InvalidOperationException("Preset failed"));

        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateCollimatorAsync("private-serial",
            TargetType.TargetType_50mm_SSD_15mm_Field, Energy.Energy_50, true, fixture.Audit.Object));
        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("Entity=CollimatorConfiguration; Id=11"));
        fixture.Collimators.Verify(x => x.CreateAsync(It.IsAny<ICollimator>()), Times.Never);
    }

    [Test]
    public void Create_WhenApplicatorFailsRetainsConfigurationAndPresetAudits()
    {
        var fixture = CreateFixture();
        fixture.Collimators.Setup(x => x.CreateAsync(It.IsAny<ICollimator>()))
            .ThrowsAsync(new InvalidOperationException("Applicator failed"));

        Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateCollimatorAsync("private-serial",
            TargetType.TargetType_50mm_SSD_15mm_Field, Energy.Energy_50, true, fixture.Audit.Object));
        Assert.That(fixture.Records, Has.Count.EqualTo(2));
        Assert.That(fixture.Records.Any(x => x.Contains("Entity=Collimator;")), Is.False);
    }

    [Test]
    public async Task OutputFactorSave_WhenRowsFailRetainsCommittedDoseRateAudit()
    {
        var fixture = CreateFixture();
        var collimator = await fixture.Service.CreateCollimatorAsync("private-serial", TargetType.TargetType_50mm_SSD_15mm_Field,
            Energy.Energy_50, true, fixture.Audit.Object);
        fixture.Records.Clear();
        var factors = new Mock<IOutputFactorCommands>();
        factors.Setup(x => x.CreateAsync(It.IsAny<IOutputFactor>())).ThrowsAsync(new InvalidOperationException("Factor failed"));
        var store = new OutputFactorConfigurationStore(fixture.Model, fixture.Service, factors.Object)
        {
            CollimatorConfiguration = collimator.Configuration!,
            DoseRate = 200
        };

        Assert.ThrowsAsync<InvalidOperationException>(() => store.SubmitOutputFactorsAsync(fixture.Audit.Object));
        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("Entity=CollimatorConfiguration; Id=11").And.Contain("Fields=ReferencedDoseRate"));
        Assert.That(fixture.Model.FindConfigurationById(11).ReferencedDoseRate, Is.EqualTo(200));
    }

    [Test]
    public async Task BackgroundConfigurationAndDefaultPresetCreation_StaySilent()
    {
        var fixture = CreateFixture();
        var configuration = await fixture.Repository.CreateCollimatorConfigurationAsync(
            TargetType.TargetType_50mm_SSD_15mm_Field, Energy.Energy_50);

        Assert.That(configuration.DefaultPreset!.Id, Is.EqualTo(22));
        Assert.That(fixture.Records, Is.Empty);
    }

    private static Fixture CreateFixture()
    {
        var configurations = new Mock<ICollimatorConfigurationCommands>();
        configurations.Setup(x => x.CreateAsync(It.IsAny<ICollimatorConfiguration>()))
            .ReturnsAsync((ICollimatorConfiguration value) => new CollimatorConfiguration(value) { Id = 11 });
        configurations.Setup(x => x.UpdateAsync(It.IsAny<ICollimatorConfiguration>(), It.IsAny<ICollimatorConfiguration>()))
            .ReturnsAsync((ICollimatorConfiguration _, ICollimatorConfiguration value) => new CollimatorConfiguration(value));
        var presets = new Mock<IPresetConfigurationCommands>();
        presets.Setup(x => x.CreateAsync(It.IsAny<IPresetConfiguration>()))
            .ReturnsAsync((IPresetConfiguration value) => new PresetConfiguration(value) { Id = 22 });
        var collimators = new Mock<ICollimatorCommands>();
        collimators.Setup(x => x.CreateAsync(It.IsAny<ICollimator>()))
            .ReturnsAsync((ICollimator value) => new Collimator(value) { Id = 33 });
        collimators.Setup(x => x.UpdateAsync(It.IsAny<ICollimator>(), It.IsAny<ICollimator>()))
            .ReturnsAsync((ICollimator _, ICollimator value) => new Collimator(value));
        var records = new List<string>();
        var audit = new Mock<IActionAuditService>();
        audit.Setup(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((action, details) => records.Add($"{action}: {details}"));
        var repository = new CollimatorRepository(Mock.Of<IHeadCommands>(), configurations.Object, presets.Object,
            collimators.Object, Mock.Of<ILogWriter>());
        var model = new CollimatorModel();
        model.Reset(new Head { Id = 3 }, Array.Empty<ICollimatorConfiguration>(), Array.Empty<ICollimator>());
        return new Fixture(new CollimatorService(model, repository), repository, model, presets, collimators, audit, records);
    }

    private sealed record Fixture(CollimatorService Service, CollimatorRepository Repository, CollimatorModel Model,
        Mock<IPresetConfigurationCommands> Presets, Mock<ICollimatorCommands> Collimators,
        Mock<IActionAuditService> Audit, List<string> Records);
}
