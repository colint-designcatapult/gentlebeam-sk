using System.Collections.ObjectModel;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Heracles.External.AppServices;
using Heracles.External.AppServices.Plan;
using Heracles.External.AppServices.System;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Heracles.Outdoor.Test.ViewModels;

[Apartment(ApartmentState.STA)]
[NonParallelizable]
internal class TreatmentSingleEmissionTests
{
    [TestCaseSource(nameof(InvalidFieldCollections))]
    public void SetPlan_InvalidTreatmentFieldShape_RejectsPlan(
        ICollection<ITreatmentField> fields)
    {
        var store = CreateTreatmentInfoStore(fields);
        var model = CreatePlanModel();

        var exception = Assert.Throws<InvalidOperationException>(
            () => model.SetPlan(store.Object));

        Assert.That(exception!.Message, Is.EqualTo(TreatmentPlanFieldRules.InvalidShapeMessage));
    }

    [Test]
    public void SetPlan_SolePlusC_LoadsUnchanged()
    {
        var field = CreateField(id: 20, TreatmentFieldName.PlusC, dwellTime: 3.0);
        var store = CreateTreatmentInfoStore([field]);
        var model = CreatePlanModel();

        model.SetPlan(store.Object);

        Assert.Multiple(() =>
        {
            Assert.That(model.TreatmentFields, Has.Count.EqualTo(1));
            Assert.That(model.TreatmentFields[0].Id, Is.EqualTo(20));
            Assert.That(model.TreatmentFields[0].Name, Is.EqualTo(TreatmentFieldName.PlusC));
            Assert.That(model.TotalDuration, Is.EqualTo(3.0));
        });
    }

    [Test]
    public void SetPlan_SolePlusCWithWrongEnergy_RejectsDedicatedMessage()
    {
        var fields = new List<ITreatmentField>
        {
            CreateField(
                id: 10,
                TreatmentFieldName.PlusC,
                dwellTime: 3.0,
                energy: Energy.Energy_70)
        };
        var store = CreateTreatmentInfoStore(fields);
        var model = CreatePlanModel();

        var exception = Assert.Throws<InvalidOperationException>(
            () => model.SetPlan(store.Object));

        Assert.That(
            exception!.Message,
            Is.EqualTo("The PlusC treatment field energy must match the prescription energy."));
    }

    [Test]
    public void SetPlan_InvalidReplacement_LeavesValidModelIntact()
    {
        var validStore = CreateTreatmentInfoStore(
            [CreateField(id: 20, TreatmentFieldName.PlusC, dwellTime: 3.0)],
            planId: 4);
        var invalidStore = CreateTreatmentInfoStore(
            [
                CreateField(id: 1, TreatmentFieldName.Plus1L1, dwellTime: 8.0),
                CreateField(id: 2, TreatmentFieldName.PlusC, dwellTime: 9.0)
            ],
            planId: 5);
        var model = CreatePlanModel();
        model.SetPlan(validStore.Object);
        var originalPlan = model.Plan;
        var originalField = model.TreatmentFields[0];

        Assert.Throws<InvalidOperationException>(() => model.SetPlan(invalidStore.Object));

        Assert.Multiple(() =>
        {
            Assert.That(model.Plan, Is.SameAs(originalPlan));
            Assert.That(model.TreatmentFields, Has.Count.EqualTo(1));
            Assert.That(model.TreatmentFields[0], Is.SameAs(originalField));
            Assert.That(model.TreatmentFields[0].Id, Is.EqualTo(20));
            Assert.That(model.TotalDuration, Is.EqualTo(3.0));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task PrepareTreatment_UsesRepositoryHydratedPlusCInsteadOfIncomingMetadata(
        bool hasPreviousTreatment)
    {
        _ = System.Windows.Application.Current ?? new System.Windows.Application();

        const long planId = 4;
        const long prescriptionId = 5;
        const long simulationId = 6;
        const long diagnosisId = 7;
        const long patientId = 8;
        var targetType = TargetType.TargetType_30mm_SSD_7_Fields;
        var incomingPlan = new Plan
        {
            Id = planId,
            PrescriptionId = prescriptionId,
            CollimatorType = targetType
        };
        incomingPlan.TreatmentFields.Add(
            CreateField(id: 1, TreatmentFieldName.Plus1L1, dwellTime: 99));
        var hydratedField = CreateField(
            id: 99,
            TreatmentFieldName.PlusC,
            dwellTime: 7);
        var prescription = new Prescription
        {
            Id = prescriptionId,
            SimulationId = simulationId,
            Energy = Energy.Energy_100,
            DailyDose = 2
        };
        var simulation = new Simulation
        {
            Id = simulationId,
            DiagnosisId = diagnosisId,
            TargetType = targetType,
            LesionDepth = 1
        };
        var diagnosis = new Diagnosis { Id = diagnosisId, PatientId = patientId };
        var patient = Mock.Of<IPatient>();
        var store = new TreatmentInfoStore();
        var planRepository = new Mock<IPlanRepository>();
        planRepository
            .Setup(repository => repository.FetchTreatmentFieldsAsync(planId, targetType))
            .ReturnsAsync([hydratedField]);
        var prescriptionCommands = new Mock<IEmrPrescriptionCommands>();
        prescriptionCommands
            .Setup(commands => commands.ReadAsync(prescriptionId))
            .ReturnsAsync(prescription);
        var simulationCommands = new Mock<IEmrSimulationCommands>();
        simulationCommands
            .Setup(commands => commands.ReadAsync(simulationId))
            .ReturnsAsync(simulation);
        var diagnosisCommands = new Mock<IEmrDiagnosisCommands>();
        diagnosisCommands
            .Setup(commands => commands.ReadAsync(diagnosisId))
            .ReturnsAsync(diagnosis);
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repository => repository.FetchAsync(patientId))
            .ReturnsAsync(patient);
        var planLoadingService = new PlanLoadingService(
            store,
            patientRepository.Object,
            diagnosisCommands.Object,
            simulationCommands.Object,
            prescriptionCommands.Object,
            planRepository.Object);

        var collimatorConfiguration = new Mock<ICollimatorConfiguration>();
        collimatorConfiguration.SetupGet(value => value.Id).Returns(77);
        collimatorConfiguration.SetupGet(value => value.Type).Returns(targetType);
        collimatorConfiguration.SetupGet(value => value.Energy).Returns(Energy.Energy_100);
        var collimatorModel = new Mock<ICollimatorModel>();
        collimatorModel
            .SetupGet(value => value.CollimatorConfigurations)
            .Returns(new ObservableCollection<ICollimatorConfiguration>
            {
                collimatorConfiguration.Object
            });
        collimatorModel
            .Setup(value => value.FindConfigurationByType(targetType, Energy.Energy_100))
            .Returns(collimatorConfiguration.Object);
        var calibrationInfo = new Mock<ICollimatorCalibrationInfo>();
        calibrationInfo
            .SetupGet(value => value.CollimatorConfiguration)
            .Returns(collimatorConfiguration.Object);
        calibrationInfo.SetupGet(value => value.HeaterCurrent).Returns(2);
        calibrationInfo
            .Setup(value => value.GetCoilConfiguration(It.IsAny<TreatmentFieldName>()))
            .Returns(new CoilConfigurationInfo());
        calibrationInfo
            .Setup(value => value.GetOutputFactor(It.IsAny<TreatmentFieldName>()))
            .Returns(new OutputFactorInfo(1, 1));
        var calibrationStore = new CollimatorCalibrationInfoStore();
        calibrationStore[77] = calibrationInfo.Object;
        var calibrationModel = new Mock<ICollimatorCalibrationModel>();
        calibrationModel
            .Setup(value => value.FetchCalibrationDataAsync(false))
            .ReturnsAsync(calibrationStore);
        var profileService = new CollimatorProfileService(
            collimatorModel.Object,
            calibrationModel.Object);
        var planModel = new PlanModel(Mock.Of<IEmrPlanCommands>(), collimatorModel.Object);

        var actualField = new ActualTreatmentField(hydratedField)
        {
            ActualDuration = 1,
            Completed = 0
        };
        var hydratedPlan = new Plan(incomingPlan, [hydratedField]);
        var previousTreatment = new Treatment(
            new Treatment
            {
                Id = 10,
                PlanId = planId,
                Fraction = 1,
                CumulativeDose = 2
            },
            hydratedPlan,
            [actualField]);
        var treatmentRepository = new Mock<ITreatmentRepository>();
        treatmentRepository
            .Setup(repository => repository.FetchLatestTreatmentByPlanAsync(It.IsAny<IPlan>()))
            .ReturnsAsync(hasPreviousTreatment ? previousTreatment : null);
        var actualTreatmentFieldModel = new Mock<IActualTreatmentFieldModel>();
        actualTreatmentFieldModel
            .Setup(model => model.FetchCollection(previousTreatment.Id))
            .ReturnsAsync(previousTreatment.ActualTreatmentFields);
        var treatmentModel = new Mock<ITreatmentModel>();
        var dialogService = new Mock<IDialogService>();
        dialogService
            .Setup(service => service.ShowDialog(
                It.IsAny<string>(),
                It.IsAny<IDialogParameters>(),
                It.IsAny<Action<IDialogResult>>()))
            .Callback<string, IDialogParameters, Action<IDialogResult>>(
                (_, _, callback) => callback(new DialogResult(ButtonResult.OK)));
        var authorizedUserStore = new Mock<IAuthorizedUserStore>();
        authorizedUserStore.SetupGet(value => value.AuthorizedUser)
            .Returns(Mock.Of<IUser>(value => value.EmailAddress == "clinician@example.test"));
        var service = new TreatmentPreparationService(
            planLoadingService,
            dialogService.Object,
            profileService,
            treatmentRepository.Object,
            planModel,
            actualTreatmentFieldModel.Object,
            treatmentModel.Object,
            authorizedUserStore.Object);

        var treatmentPlan = await service.PrepareTreatmentAsync(incomingPlan);

        Assert.Multiple(() =>
        {
            Assert.That(treatmentPlan.Fields, Has.Count.EqualTo(1));
            Assert.That(treatmentPlan.Fields.Single().Planned.Id, Is.EqualTo(99));
            Assert.That(treatmentPlan.Fields.Single().Planned.Name, Is.EqualTo(TreatmentFieldName.PlusC));
            Assert.That(treatmentPlan.Fields.Single().Actual.Name, Is.EqualTo(TreatmentFieldName.PlusC));
            Assert.That(treatmentPlan.Fields.Single().Duration, Is.EqualTo(7));
        });
    }

    private static IEnumerable<TestCaseData> InvalidFieldCollections()
    {
        yield return new TestCaseData(new List<ITreatmentField>());
        yield return new TestCaseData(new List<ITreatmentField>
        {
            CreateField(id: 10, TreatmentFieldName.Plus1L1, dwellTime: 3.0)
        });
        yield return new TestCaseData(new List<ITreatmentField>
        {
            CreateField(id: 1, TreatmentFieldName.Plus1L1, dwellTime: 2.0),
            CreateField(id: 20, TreatmentFieldName.PlusC, dwellTime: 3.0)
        });
    }

    private static PlanModel CreatePlanModel()
    {
        var collimatorModel = new Mock<ICollimatorModel>();
        collimatorModel
            .Setup(value => value.FindConfigurationByType(
                It.IsAny<TargetType>(),
                It.IsAny<Energy>()))
            .Returns(Mock.Of<ICollimatorConfiguration>());
        return new PlanModel(Mock.Of<IEmrPlanCommands>(), collimatorModel.Object);
    }

    private static Mock<ITreatmentInfoStore> CreateTreatmentInfoStore(
        ICollection<ITreatmentField> fields,
        long planId = 4)
    {
        var plan = new Mock<IPlan>();
        plan.SetupGet(value => value.Id).Returns(planId);
        plan.SetupGet(value => value.TreatmentFields).Returns(fields);
        plan.SetupGet(value => value.CollimatorType)
            .Returns(TargetType.TargetType_30mm_SSD_7_Fields);

        var prescription = new Mock<IPrescription>();
        prescription.SetupGet(value => value.Energy).Returns(Energy.Energy_100);

        var store = new Mock<ITreatmentInfoStore>();
        store.SetupGet(value => value.Plan).Returns(plan.Object);
        store.SetupGet(value => value.Prescription).Returns(prescription.Object);
        store.SetupGet(value => value.Diagnosis).Returns(Mock.Of<IDiagnosis>());
        store.SetupGet(value => value.Simulation).Returns(Mock.Of<ISimulation>());
        return store;
    }

    private static ITreatmentField CreateField(
        long id,
        TreatmentFieldName name,
        double dwellTime,
        Energy energy = Energy.Energy_100)
    {
        return new TreatmentField
        {
            Id = id,
            Name = name,
            DwellTime = dwellTime,
            Energy = energy
        };
    }
}
