using System.Threading;
using System.Reflection;
using Heracles.Application.Models.CollimatorConfiguration.CSV;
using Prism.Services.Dialogs;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Heracles.Indoor.ViewModels.Physics;
using Moq;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.Test.ViewModels.Physics;

[Apartment(ApartmentState.STA)]
internal sealed class PhysicsConfigurationAuditTests
{
    [Test]
    public async Task HeaterSave_AuditsCommittedChangeOnlyAfterPersistence()
    {
        var fixture = await CreateHeaterFixture();
        var completion = new TaskCompletionSource<IHeaterCurrentConfig>();
        fixture.Commands.Setup(x => x.UpdateAsync(It.IsAny<IHeaterCurrentConfig>(), It.IsAny<IHeaterCurrentConfig>()))
            .Returns(completion.Task);
        fixture.Store.HeaterCurrent.HeaterCurrent = 2200;
        fixture.ViewModel.SaveCommand.Execute();
        Assert.That(fixture.Audit.Records, Is.Empty);

        completion.SetResult(new HeaterCurrentConfig { Id = 31, PresetConfigurationId = 7, HeaterCurrent = 2200 });
        await fixture.ViewModel.CurrentHeaterCurrentTask.Task;

        Assert.That(fixture.Audit.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Audit.Records[0], Does.Contain("Id=31").And.Contain("Fields=HeaterCurrent"));
        Assert.That(fixture.Audit.Records[0], Does.Not.Contain("2200"));
    }

    [Test]
    public async Task HeaterSave_UnchangedRevertedAndUnappliedChangesStaySilent()
    {
        var fixture = await CreateHeaterFixture();
        fixture.ViewModel.SaveCommand.Execute();
        await fixture.ViewModel.CurrentHeaterCurrentTask.Task;
        fixture.Store.HeaterCurrent.HeaterCurrent = 2200;
        fixture.Store.HeaterCurrent.HeaterCurrent = 2000;
        fixture.ViewModel.SaveCommand.Execute();
        await fixture.ViewModel.CurrentHeaterCurrentTask.Task;
        fixture.Commands.Verify(x => x.UpdateAsync(It.IsAny<IHeaterCurrentConfig>(), It.IsAny<IHeaterCurrentConfig>()), Times.Never);

        fixture.Commands.Setup(x => x.UpdateAsync(It.IsAny<IHeaterCurrentConfig>(), It.IsAny<IHeaterCurrentConfig>()))
            .ReturnsAsync(new HeaterCurrentConfig { Id = 31, PresetConfigurationId = 7, HeaterCurrent = 2000 });
        fixture.Store.HeaterCurrent.HeaterCurrent = 2200;
        fixture.ViewModel.SaveCommand.Execute();
        await fixture.ViewModel.CurrentHeaterCurrentTask.Task;

        Assert.That(fixture.Audit.Records, Is.Empty);
    }

    [Test]
    public async Task HeaterSave_FailureIsNotReportedAsAChange()
    {
        var fixture = await CreateHeaterFixture();
        fixture.Commands.Setup(x => x.UpdateAsync(It.IsAny<IHeaterCurrentConfig>(), It.IsAny<IHeaterCurrentConfig>()))
            .ThrowsAsync(new InvalidOperationException("Persistence failed"));
        fixture.Store.HeaterCurrent.HeaterCurrent = 2200;
        fixture.ViewModel.SaveCommand.Execute();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await fixture.ViewModel.CurrentHeaterCurrentTask.Task);
        Assert.That(fixture.Audit.Records, Is.Empty);
    }

    [Test]
    public async Task PhysicsInitialization_AndNonUserPersistenceStaySilentWithAnAuthenticatedUser()
    {
        var fixture = await CreateHeaterFixture();
        Assert.That(fixture.Audit.Records, Is.Empty);
        fixture.Store.HeaterCurrent.HeaterCurrent = 2200;
        await fixture.Store.SubmitHeaterCurrentAsync();
        Assert.That(fixture.Store.HeaterCurrent.HeaterCurrent, Is.EqualTo(2200));
        Assert.That(fixture.Audit.Records, Is.Empty);
    }

    [Test]
    public async Task MagnetometerSave_PartialCommitIsAuditedAndNotRepeatedOnRetry()
    {
        var audit = new AuditCapture();
        var matrices = new Mock<ICorrectionMatrixCommands>();
        var references = new Mock<IReferenceFieldCommands>();
        bool failBack = true;
        matrices.Setup(x => x.CreateAsync(It.IsAny<ICorrectionMatrixEntry>()))
            .Returns((ICorrectionMatrixEntry entry) =>
            {
                if (entry.MagnetometerType == MagnetometerType.Back && failBack)
                    throw new InvalidOperationException("Back matrix failed");
                entry.Id = entry.MagnetometerType == MagnetometerType.Front ? 41 : 42;
                return Task.FromResult(entry);
            });
        references.Setup(x => x.CreateAsync(It.IsAny<IReferenceFieldEntry>()))
            .ReturnsAsync((IReferenceFieldEntry entry) =>
            {
                entry.Id = entry.MagnetometerType == MagnetometerType.Front ? 43 : 44;
                return entry;
            });
        var store = new MagnetometerCorrectionsStore(matrices.Object, references.Object, Mock.Of<ICollimatorModel>())
        {
            CollimatorConfiguration = Configuration()
        };
        store.Corrections.FrontMatrix.Set(1, 0, 0, 0, 1, 0);
        store.Corrections.BackMatrix.Set(1, 0, 0, 0, 1, 0);
        store.Corrections.FrontReferenceField.Set(1, 2, 3);
        store.Corrections.BackReferenceField.Set(1, 2, 3);

        Assert.ThrowsAsync<InvalidOperationException>(() => store.SubmitMagnetometerParametersAsync(audit.Service));
        Assert.That(audit.Records, Has.Count.EqualTo(1));
        Assert.That(audit.Records[0], Does.Contain("Entity=CorrectionMatrix; Id=41"));
        Assert.That(store.Corrections.FrontMatrix.Id, Is.EqualTo(41));

        failBack = false;
        await store.SubmitMagnetometerParametersAsync(audit.Service);
        Assert.That(audit.Records, Has.Count.EqualTo(4));
        Assert.That(audit.Records.Count(x => x.Contains("Entity=CorrectionMatrix; Id=41")), Is.EqualTo(1));
        store.Corrections.FrontMatrix.Cm11 = 2;
        store.Corrections.FrontMatrix.Cm11 = 1;
        await store.SubmitMagnetometerParametersAsync(audit.Service);
        Assert.That(audit.Records, Has.Count.EqualTo(4));
    }

    [Test]
    public async Task OutputFactorSave_PartialCommitAndRetryReportOnlyCommittedRows()
    {
        var audit = new AuditCapture();
        var commands = new Mock<IOutputFactorCommands>();
        bool failSecond = true;
        int calls = 0;
        commands.Setup(x => x.CreateAsync(It.IsAny<IOutputFactor>())).Returns((IOutputFactor entry) =>
        {
            if (++calls == 2 && failSecond)
                throw new InvalidOperationException("Second factor failed");
            return Task.FromResult<IOutputFactor>(new OutputFactorEntry(entry) { Id = 50 + calls });
        });
        var store = new OutputFactorConfigurationStore(Mock.Of<ICollimatorModel>(), null!, commands.Object)
        {
            CollimatorConfiguration = Configuration(TargetType.TargetType_50mm_SSD_13_Fields)
        };
        foreach (var factor in store.Configuration.OutputFactors) factor.Factor = 1;

        Assert.ThrowsAsync<InvalidOperationException>(() => store.SubmitOutputFactorsAsync(audit.Service));
        Assert.That(audit.Records, Has.Count.EqualTo(1));
        Assert.That(audit.Records[0], Does.Contain("Entity=OutputFactor; Id=51"));
        failSecond = false;
        await store.SubmitOutputFactorsAsync(audit.Service);
        Assert.That(audit.Records, Has.Count.EqualTo(13));
        store.Configuration.OutputFactors[0].Factor = 1.1;
        store.Configuration.OutputFactors[0].Factor = 1;
        await store.SubmitOutputFactorsAsync(audit.Service);
        Assert.That(audit.Records, Has.Count.EqualTo(13));
    }

    [Test]
    public async Task CoilSave_ImportedValuesAreComparedToPersistedSnapshotNotDirtyFlags()
    {
        var audit = new AuditCapture();
        var commands = new Mock<ICoilConfigurationCommands>();
        var store = new CoilConfigurationStore(commands.Object, Mock.Of<ICollimatorModel>())
        {
            CollimatorConfiguration = Configuration()
        };
        var form = store.Configuration.GetConfiguration().Single();
        var saved = new CoilConfigurationEntry { Id = 61, PresetConfigurationId = 7, FieldName = form.FieldName,
            XDeflectionCurrent = 10, YDeflectionCurrent = 20, FocusCurrent = 1000 };
        commands.Setup(x => x.ReadListAsync(7)).ReturnsAsync(new List<ICoilConfigurationEntry> { saved });
        commands.Setup(x => x.UpdateAsync(It.IsAny<ICoilConfigurationEntry>(), It.IsAny<ICoilConfigurationEntry>()))
            .ReturnsAsync((ICoilConfigurationEntry _, ICoilConfigurationEntry entry) => new CoilConfigurationEntry(entry));
        await store.FetchCollimatorConfigurationAsync();
        form.SetupFormValue(new CoilConfigurationEntry(saved) { FocusCurrent = 1100 });
        await store.SubmitCollimatorConfigurationAsync(audit.Service);
        Assert.That(audit.Records, Has.Count.EqualTo(1));
        Assert.That(audit.Records[0], Does.Contain("Id=61").And.Contain("Fields=FocusCurrent"));

        form.FocusCurrent = 1200;
        form.FocusCurrent = 1100;
        await store.SubmitCollimatorConfigurationAsync(audit.Service);
        Assert.That(audit.Records, Has.Count.EqualTo(1));
        commands.Verify(x => x.UpdateAsync(It.IsAny<ICoilConfigurationEntry>(), It.IsAny<ICoilConfigurationEntry>()), Times.Once);
    }

    [Test]
    public async Task CsvImport_UnchangedValuesKeepPersistedIdentitiesAndDoNotCreateDuplicates()
    {
        var heater = await CreateHeaterFixture();
        var coilCommands = new Mock<ICoilConfigurationCommands>();
        var coils = new CoilConfigurationStore(coilCommands.Object, Mock.Of<ICollimatorModel>())
        {
            CollimatorConfiguration = Configuration()
        };
        var coil = new CoilConfigurationEntry
        {
            Id = 61, PresetConfigurationId = 7, FieldName = coils.Configuration.GetConfiguration().Single().FieldName,
            XDeflectionCurrent = 10, YDeflectionCurrent = 20, FocusCurrent = 1000
        };
        coilCommands.Setup(x => x.ReadListAsync(7)).ReturnsAsync(new List<ICoilConfigurationEntry> { coil });
        await coils.FetchCollimatorConfigurationAsync();
        var factorCommands = new Mock<IOutputFactorCommands>();
        var factors = new OutputFactorConfigurationStore(Mock.Of<ICollimatorModel>(), null!, factorCommands.Object)
        {
            CollimatorConfiguration = Configuration()
        };
        var factor = new OutputFactorEntry
        {
            Id = 71, PresetConfigurationId = 7, FieldName = factors.Configuration.OutputFactors.Single().FieldName, Factor = 1
        };
        factorCommands.Setup(x => x.ReadListAsync(7)).ReturnsAsync(new List<IOutputFactor> { factor });
        await factors.FetchOutputFactorsAsync();
        var viewModel = new ConfigurationViewModel(Mock.Of<IMagnetometerCorrectionsStore>(), heater.Store, coils, factors,
            Mock.Of<IPresetConfigurationCommands>(), Mock.Of<ICollimatorModel>(), Mock.Of<IDialogService>(),
            heater.Audit.Log.Object, Mock.Of<IPopUpService>(), heater.Audit.Service);
        var csv = new CsvConfiguration
        {
            CoilConfigurations = new[] { new CoilConfigurationEntry
            {
                FieldName = coil.FieldName, XDeflectionCurrent = 10, YDeflectionCurrent = 20, FocusCurrent = 1000
            } },
            HeaterCurrentConfig = new HeaterCurrentConfig { HeaterCurrent = 2000 },
            OutputFactorEntries = new[] { new OutputFactorEntry { FieldName = factor.FieldName, Factor = 1 } }
        };
        // Enter at the validated import boundary without opening the OS file picker.
        typeof(ConfigurationViewModel).GetMethod("FillConfigurationModelsWithCsvConfig", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(viewModel, new object[] { csv });
        await coils.SubmitCollimatorConfigurationAsync(heater.Audit.Service);
        await heater.Store.SubmitHeaterCurrentAsync(heater.Audit.Service);
        await factors.SubmitOutputFactorsAsync(heater.Audit.Service);

        Assert.That(heater.Audit.Records, Is.Empty);
        Assert.That(coils.Configuration.GetConfiguration().Single().Id, Is.EqualTo(61));
        Assert.That(heater.Store.HeaterCurrent.Id, Is.EqualTo(31));
        Assert.That(factors.Configuration.OutputFactors.Single().Id, Is.EqualTo(71));
        coilCommands.Verify(x => x.CreateAsync(It.IsAny<ICoilConfigurationEntry>()), Times.Never);
        heater.Commands.Verify(x => x.CreateAsync(It.IsAny<IHeaterCurrentConfig>()), Times.Never);
        factorCommands.Verify(x => x.CreateAsync(It.IsAny<IOutputFactor>()), Times.Never);
    }

    [Test]
    public async Task PresetApproval_RecordsChangedApprovalWithoutCredentialsAndNotFailures()
    {
        var audit = new AuditCapture();
        var preset = Configuration().DefaultPreset!;
        var commands = new Mock<IPresetConfigurationCommands>();
        commands.Setup(x => x.ApproveAsync(7, "approver", "private-password"))
            .ReturnsAsync(new PresetConfiguration { Id = 7, ApprovedBy = "approver" });
        var approval = new PresetConfigurationApprovalAction(commands.Object, preset, audit.Service);
        await approval.ApproveAsync("approver", "private-password");
        await approval.ApproveAsync("approver", "private-password");
        Assert.That(audit.Records, Has.Count.EqualTo(1));
        Assert.That(audit.Records[0], Does.Contain("Id=7").And.Not.Contain("private-password").And.Not.Contain("approver"));
        commands.Setup(x => x.ApproveAsync(7, "approver", "wrong"))
            .ThrowsAsync(new InvalidOperationException("Approval rejected"));
        Assert.ThrowsAsync<InvalidOperationException>(() => approval.ApproveAsync("approver", "wrong"));
        Assert.That(audit.Records, Has.Count.EqualTo(1));
    }

    private static CollimatorConfiguration Configuration(TargetType type = TargetType.TargetType_50mm_SSD_15mm_Field)
    {
        var configuration = new CollimatorConfiguration { Id = 5, Type = type, Energy = Energy.Energy_50, ReferencedDoseRate = 100 };
        configuration.AddPreset(new PresetConfiguration { Id = 7, CollimatorConfigurationId = 5, IsActive = true, IsDefault = true });
        return configuration;
    }

    private static async Task<HeaterFixture> CreateHeaterFixture()
    {
        var commands = new Mock<IHeaterCurrentConfigCommands>();
        commands.Setup(x => x.ReadListAsync(7)).ReturnsAsync(new List<IHeaterCurrentConfig>
        {
            new HeaterCurrentConfig { Id = 31, PresetConfigurationId = 7, HeaterCurrent = 2000 }
        });
        commands.Setup(x => x.UpdateAsync(It.IsAny<IHeaterCurrentConfig>(), It.IsAny<IHeaterCurrentConfig>()))
            .ReturnsAsync((IHeaterCurrentConfig _, IHeaterCurrentConfig entry) => new HeaterCurrentConfig(entry));
        var store = new HeaterCurrentStore(commands.Object, Mock.Of<ICollimatorModel>()) { CollimatorConfiguration = Configuration() };
        var audit = new AuditCapture();
        var viewModel = new HeaterCurrentViewModel(store, Mock.Of<IPopUpService>(), audit.Log.Object, audit.Service);
        await store.FetchHeaterCurrentAsync();
        return new HeaterFixture(store, commands, viewModel, audit);
    }

    private sealed record HeaterFixture(HeaterCurrentStore Store, Mock<IHeaterCurrentConfigCommands> Commands,
        HeaterCurrentViewModel ViewModel, AuditCapture Audit);

    private sealed class AuditCapture
    {
        public List<string> Records { get; } = new();
        public Mock<ILogRepository> Log { get; } = new();
        public IActionAuditService Service { get; }
        public AuditCapture()
        {
            Log.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()))
                .Callback<string, LogRecordSeverity, LogRecordType>((message, _, type) =>
                {
                    if (type == LogRecordType.User) Records.Add(message);
                }).Returns(Task.CompletedTask);
            Service = new ActionAuditService(Log.Object, new AuthorizedUserStore
            {
                AuthorizedUser = new User { Id = 17, Username = "operator" }
            });
        }
    }
}
