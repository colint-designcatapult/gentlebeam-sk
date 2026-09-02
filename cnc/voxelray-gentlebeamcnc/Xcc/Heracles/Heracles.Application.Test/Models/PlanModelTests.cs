using System.Reflection;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Moq;
using Prism.Events;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Heracles.Application.Test.Models;

[TestFixture]
public sealed class PlanModelTests
{
    [TestCaseSource(nameof(SupportedClinicalTargets))]
    public async Task PrescriptionUpserts_AlwaysProduceOnePlusC(TargetType targetType)
    {
        var fixture = CreateFixture(targetType);
        await fixture.Model.OnUpdatePrescriptionAsync(fixture.Prescription);

        fixture.Model.AddOrUpdateTreatmentField(fixture.Prescription);
        fixture.Model.AddOrUpdateTreatmentField(fixture.Prescription);

        Assert.Multiple(() =>
        {
            Assert.That(fixture.Model.TreatmentFields, Has.Count.EqualTo(1));
            Assert.That(fixture.Model.TreatmentField.Name, Is.EqualTo(TreatmentFieldName.PlusC));
            Assert.That(fixture.Model.TreatmentField.Energy, Is.EqualTo(fixture.Prescription.Energy));
        });
    }

    [Test]
    public async Task SubmitBlankPlan_CreatesOnePlusCWithReturnedPlanId()
    {
        var fixture = CreateFixture(TargetType.TargetType_30mm_SSD_7_Fields);
        await fixture.Model.OnUpdatePrescriptionAsync(fixture.Prescription);
        fixture.Model.AddOrUpdateTreatmentField(fixture.Prescription);
        fixture.Model.AddOrUpdateTreatmentField(fixture.Prescription);
        ITreatmentField? submittedField = null;
        fixture.PlanRepository
            .Setup(repository => repository.CreatePlanAsync(It.IsAny<IPlan>()))
            .ReturnsAsync((IPlan plan) => new Plan(plan) { Id = 91 });
        fixture.PlanRepository
            .Setup(repository => repository.CreateTreatmentFieldAsync(It.IsAny<ITreatmentField>()))
            .Callback<ITreatmentField>(field => submittedField = new TreatmentField(field))
            .ReturnsAsync((ITreatmentField field) => new TreatmentField(field) { Id = 92 });

        var savedPlan = await fixture.Model.SubmitAsync();

        Assert.Multiple(() =>
        {
            Assert.That(savedPlan.Id, Is.EqualTo(91));
            Assert.That(submittedField, Is.Not.Null);
            Assert.That(submittedField!.PlanId, Is.EqualTo(91));
            Assert.That(submittedField.Name, Is.EqualTo(TreatmentFieldName.PlusC));
        });
        fixture.PlanRepository.Verify(
            repository => repository.CreateTreatmentFieldAsync(It.IsAny<ITreatmentField>()),
            Times.Once);
        fixture.PlanRepository.Verify(
            repository => repository.UpdateTreatmentFieldAsync(It.IsAny<ITreatmentField?>(), It.IsAny<ITreatmentField>()),
            Times.Never);
    }

    [Test]
    public async Task UpsertPersistedField_SubmitsUpdateNotCreate()
    {
        var persistedPlan = CreatePlan(50, TargetType.TargetType_30mm_SSD_7_Fields);
        var persistedField = CreateField(TreatmentFieldName.PlusC, id: 60);
        var fixture = CreateFixture(
            TargetType.TargetType_30mm_SSD_7_Fields,
            persistedPlan,
            [persistedField]);
        await fixture.Model.OnUpdatePrescriptionAsync(fixture.Prescription);
        fixture.Model.AddOrUpdateTreatmentField(fixture.Prescription);
        fixture.PlanRepository
            .Setup(repository => repository.UpdateTreatmentFieldAsync(null, It.IsAny<ITreatmentField>()))
            .ReturnsAsync((ITreatmentField? _, ITreatmentField field) => new TreatmentField(field));

        await fixture.Model.SubmitAsync();

        fixture.PlanRepository.Verify(
            repository => repository.UpdateTreatmentFieldAsync(null, It.Is<ITreatmentField>(field =>
                field.Id == 60 && field.Name == TreatmentFieldName.PlusC)),
            Times.Once);
        fixture.PlanRepository.Verify(
            repository => repository.CreateTreatmentFieldAsync(It.IsAny<ITreatmentField>()),
            Times.Never);
    }

    [TestCase(InvalidShape.Empty)]
    [TestCase(InvalidShape.NonCenter)]
    [TestCase(InvalidShape.Multiple)]
    public void FetchInvalidShape_RejectsExactMessage(InvalidShape shape)
    {
        var plan = CreatePlan(50, TargetType.TargetType_30mm_SSD_7_Fields);
        var fixture = CreateFixture(
            TargetType.TargetType_30mm_SSD_7_Fields,
            plan,
            CreateShape(shape));

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Model.OnUpdatePrescriptionAsync(fixture.Prescription));

        Assert.That(exception!.Message, Is.EqualTo(TreatmentPlanFieldRules.InvalidShapeMessage));
        VerifyNoPersistence(fixture.PlanRepository);
    }

    [TestCase(InvalidShape.Empty)]
    [TestCase(InvalidShape.NonCenter)]
    [TestCase(InvalidShape.Multiple)]
    public async Task InvalidShape_FailsValidationSubmissionAndApprovalBeforePersistence(InvalidShape shape)
    {
        var fixture = CreateFixture(TargetType.TargetType_30mm_SSD_7_Fields);
        await fixture.Model.OnUpdatePrescriptionAsync(fixture.Prescription);
        ReplaceBackingFields(fixture.Model, CreateShape(shape));

        var entries = fixture.Model.TreatmentFields;
        Assert.That(fixture.Model.ValidateDosesAndEmissionCurrent(entries), Is.False);

        var submitException = Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Model.SubmitAsync());
        var approvalException = Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Model.ChangeStatusAsync("approver", "password", PlanStatus.APPROVED));

        Assert.Multiple(() =>
        {
            Assert.That(submitException!.Message, Is.EqualTo(TreatmentPlanFieldRules.InvalidShapeMessage));
            Assert.That(approvalException!.Message, Is.EqualTo(TreatmentPlanFieldRules.InvalidShapeMessage));
        });
        VerifyNoPersistence(fixture.PlanRepository);
    }

    [Test]
    public async Task FetchOnePlusC_Succeeds()
    {
        var plan = CreatePlan(50, TargetType.TargetType_30mm_SSD_7_Fields);
        var fixture = CreateFixture(
            TargetType.TargetType_30mm_SSD_7_Fields,
            plan,
            [CreateField(TreatmentFieldName.PlusC, id: 60)]);

        await fixture.Model.OnUpdatePrescriptionAsync(fixture.Prescription);

        Assert.Multiple(() =>
        {
            Assert.That(fixture.Model.TreatmentFields, Has.Count.EqualTo(1));
            Assert.That(fixture.Model.TreatmentField.Name, Is.EqualTo(TreatmentFieldName.PlusC));
            Assert.That(fixture.Model.TreatmentField.Id, Is.EqualTo(60));
        });
    }

    private static IEnumerable<TargetType> SupportedClinicalTargets()
    {
        yield return TargetType.TargetType_30mm_SSD_7_Fields;
        yield return TargetType.TargetType_50mm_SSD_13_Fields;
        yield return TargetType.TargetType_50mm_SSD_15mm_Field;
        yield return TargetType.TargetType_50mm_SSD_20mm_Field;
        yield return TargetType.TargetType_50mm_SSD_30mm_Field;
        yield return TargetType.TargetType_50mm_SSD_40mm_Field;
        yield return TargetType.TargetType_50mm_SSD_50mm_Field;
    }

    private static Fixture CreateFixture(
        TargetType targetType,
        IPlan? fetchedPlan = null,
        ICollection<ITreatmentField>? fetchedFields = null)
    {
        var store = new TreatmentInfoStore
        {
            Simulation = new Simulation
            {
                Id = 11,
                TargetType = targetType
            }
        };
        var prescription = new Prescription
        {
            Id = 12,
            SimulationId = 11,
            Energy = Energy.Energy_100,
            DwellTime = 4,
            DailyDose = 20
        };
        var collimatorConfiguration = Mock.Of<ICollimatorConfiguration>();
        var collimatorModel = new Mock<ICollimatorModel>();
        collimatorModel
            .Setup(model => model.FindConfigurationByType(targetType, prescription.Energy))
            .Returns(collimatorConfiguration);
        var doseCalculation = new Mock<ITreatmentDoseCalculation>();
        doseCalculation
            .Setup(calculation => calculation.CalculateDose(
                It.IsAny<TreatmentFieldName>(),
                collimatorConfiguration,
                It.IsAny<double>()))
            .Returns(20);
        var planRepository = new Mock<IPlanRepository>();
        planRepository
            .Setup(repository => repository.FetchLatestPlanAsync(prescription.Id))
            .ReturnsAsync(fetchedPlan);
        if (fetchedPlan is not null)
        {
            planRepository
                .Setup(repository => repository.FetchOrderedTreatmentFieldsAsync(fetchedPlan.Id))
                .ReturnsAsync(fetchedFields ?? Array.Empty<ITreatmentField>());
        }

        var user = new Mock<IUser>();
        user.SetupGet(value => value.EmailAddress).Returns("approver@example.com");
        var authorizedUserStore = new Mock<IAuthorizedUserStore>();
        authorizedUserStore.SetupGet(value => value.AuthorizedUser).Returns(user.Object);

        var model = new PlanModel(
            store,
            collimatorModel.Object,
            Mock.Of<IAppGlobals>(),
            Mock.Of<ILogWriter>(),
            Mock.Of<IDialogService>(),
            Mock.Of<IActionAuditService>(),
            authorizedUserStore.Object,
            Mock.Of<ISimulationRepository>(),
            doseCalculation.Object,
            Mock.Of<IPrescriptionRepository>(),
            planRepository.Object,
            Mock.Of<IEventAggregator>());

        return new Fixture(model, prescription, planRepository);
    }

    private static IPlan CreatePlan(long id, TargetType targetType)
    {
        return new Plan
        {
            Id = id,
            PrescriptionId = 12,
            Status = PlanStatus.PENDING_APPROVAL,
            CollimatorType = targetType
        };
    }

    private static TreatmentField CreateField(
        TreatmentFieldName name,
        long id = -1)
    {
        return new TreatmentField
        {
            Id = id,
            Name = name,
            Energy = Energy.Energy_100,
            DwellTime = 4,
            CalculatedDose = 20,
            Current = 0.1
        };
    }

    private static ICollection<ITreatmentField> CreateShape(InvalidShape shape)
    {
        return shape switch
        {
            InvalidShape.Empty => Array.Empty<ITreatmentField>(),
            InvalidShape.NonCenter => [CreateField(TreatmentFieldName.Plus1L1)],
            InvalidShape.Multiple =>
            [
                CreateField(TreatmentFieldName.PlusC),
                CreateField(TreatmentFieldName.Plus1L1)
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(shape))
        };
    }

    private static void ReplaceBackingFields(
        PlanModel model,
        IEnumerable<ITreatmentField> fields)
    {
        var backingField = typeof(PlanModel).GetField(
            "_treatmentFields",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var collection = (TreatmentFieldEntryObservableCollection)backingField.GetValue(model)!;
        collection.Clear();
        foreach (var field in fields)
        {
            collection.Add(new TreatmentFieldEntry(field, 0));
        }
    }

    private static void VerifyNoPersistence(Mock<IPlanRepository> repository)
    {
        repository.Verify(value => value.CreatePlanAsync(It.IsAny<IPlan>()), Times.Never);
        repository.Verify(value => value.CreateTreatmentFieldAsync(It.IsAny<ITreatmentField>()), Times.Never);
        repository.Verify(
            value => value.UpdateTreatmentFieldAsync(It.IsAny<ITreatmentField?>(), It.IsAny<ITreatmentField>()),
            Times.Never);
        repository.Verify(
            value => value.UpdateStatusAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<PlanStatus>()),
            Times.Never);
    }

    public enum InvalidShape
    {
        Empty,
        NonCenter,
        Multiple
    }

    private sealed record Fixture(
        PlanModel Model,
        Prescription Prescription,
        Mock<IPlanRepository> PlanRepository);
}
