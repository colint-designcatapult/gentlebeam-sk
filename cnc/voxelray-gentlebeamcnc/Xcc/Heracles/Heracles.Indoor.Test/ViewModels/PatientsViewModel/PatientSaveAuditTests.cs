using System.Collections.ObjectModel;
using System.Threading;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Moq;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using PatientViewModel = Heracles.Indoor.ViewModels.PatientsViewModel;

namespace Heracles.Indoor.Test.ViewModels.PatientsViewModel;

[Apartment(ApartmentState.STA)]
internal sealed class PatientSaveAuditTests
{
    [Test]
    public async Task Save_RecordsOneChangeOnlyAfterPersistenceCompletes()
    {
        var fixture = CreateFixture();
        var completion = new TaskCompletionSource<IPatient>();
        fixture.Patients.Setup(model => model.SavePatientAsync(It.IsAny<IPatient>())).Returns(completion.Task);
        fixture.ViewModel.PatientProfileForm!.FormData!.FirstName = "Changed";

        fixture.ViewModel.SavePatientCommand.Execute();
        Assert.That(fixture.AuditRecords, Is.Empty);
        completion.SetResult(new Patient(fixture.Patient) { FirstName = "Changed" });
        await fixture.ViewModel.SavePatientTask!.Task;

        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(1));
        Assert.That(fixture.AuditRecords[0], Does.Contain("id=23"));
    }

    [Test]
    public async Task Save_FailureDoesNotAuditAttempt()
    {
        var fixture = CreateFixture();
        fixture.Patients.Setup(model => model.SavePatientAsync(It.IsAny<IPatient>()))
            .ThrowsAsync(new InvalidOperationException());
        fixture.ViewModel.PatientProfileForm!.FormData!.FirstName = "Changed";

        fixture.ViewModel.SavePatientCommand.Execute();
        await fixture.ViewModel.SavePatientTask!.Task;

        Assert.That(fixture.AuditRecords, Is.Empty);
        Assert.That(fixture.ViewModel.PatientProfileForm.FormData, Is.Not.Null);
    }

    [Test]
    public async Task Save_UnchangedOrRevertedProfileDoesNotAudit()
    {
        var fixture = CreateFixture();
        fixture.ViewModel.PatientProfileForm!.FormData!.FirstName = "Changed";
        fixture.ViewModel.PatientProfileForm.FormData.FirstName = fixture.Patient.FirstName;

        fixture.ViewModel.SavePatientCommand.Execute();
        await fixture.ViewModel.SavePatientTask!.Task;

        Assert.That(fixture.AuditRecords, Is.Empty);
    }

    [Test]
    public async Task Save_UnappliedChangeDoesNotAudit()
    {
        var fixture = CreateFixture();
        fixture.Patients.Setup(model => model.SavePatientAsync(It.IsAny<IPatient>()))
            .ReturnsAsync(new Patient(fixture.Patient));
        fixture.ViewModel.PatientProfileForm!.FormData!.FirstName = "Changed";

        fixture.ViewModel.SavePatientCommand.Execute();
        await fixture.ViewModel.SavePatientTask!.Task;

        Assert.That(fixture.AuditRecords, Is.Empty);
    }

    [Test]
    public async Task Save_NewPatientRecordsSuccessfulCreation()
    {
        var fixture = CreateFixture();
        fixture.ViewModel.CancelEditPatientCommand.Execute();
        fixture.ViewModel.NewPatientCommand.Execute();
        var form = fixture.ViewModel.PatientProfileForm!.FormData!;
        form.FirstName = "New";
        form.LastName = "Patient";
        form.DOB = new DateOnly(1980, 1, 1);
        fixture.Patients.Setup(model => model.SavePatientAsync(It.IsAny<IPatient>()))
            .ReturnsAsync((IPatient patient) => new Patient(patient) { Id = 24 });

        fixture.ViewModel.SavePatientCommand.Execute();
        await fixture.ViewModel.SavePatientTask!.Task;

        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(1));
        Assert.That(fixture.AuditRecords[0], Does.Contain("id=24"));
    }

    [Test]
    public void CachedList_RecordsEachVisitButNotRefreshNotifications()
    {
        var fixture = CreateFixture(false);
        Assert.That(fixture.AuditRecords, Is.Empty);

        fixture.ViewModel.SetPatientListVisible(true);
        fixture.ViewModel.SetPatientListVisible(true);
        fixture.Patients.Raise(model => model.PropertyChanged += null,
            new System.ComponentModel.PropertyChangedEventArgs(nameof(IPatientListModel.Patients)));
        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(1));
        Assert.That(fixture.AuditRecords[0], Does.Contain("patient id=23").And.Contain("user id=17"));

        fixture.ViewModel.SetPatientListVisible(false);
        fixture.ViewModel.SetPatientListVisible(true);
        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(2));
        Assert.That(fixture.AuditRecords.All(record =>
            !record.Contains(fixture.Patient.FirstName) && !record.Contains(fixture.Patient.MRN)), Is.True);
    }

    [Test]
    public void CachedProfile_RecordsRepeatedEditViewsWithoutRecordingSelection()
    {
        var fixture = CreateFixture(false);
        fixture.ViewModel.SelectedPatient = fixture.Patient;
        Assert.That(fixture.AuditRecords, Is.Empty);

        fixture.ViewModel.EditPatientCommand.Execute();
        fixture.ViewModel.CancelEditPatientCommand.Execute();
        fixture.ViewModel.EditPatientCommand.Execute();

        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(2));
        Assert.That(fixture.AuditRecords.All(record => record.Contains("patient id=23")), Is.True);
    }

    [Test]
    public void Search_RecordsOnlyDisplayedPatientsAndNeverTheSearchPhrase()
    {
        var fixture = CreateFixture(false);
        fixture.ViewModel.SetPatientListVisible(true);
        fixture.AuditRecords.Clear();

        fixture.ViewModel.SearchPhrase = fixture.Patient.MRN;
        fixture.ViewModel.SearchPhrase = fixture.Patient.MRN;
        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(1));
        Assert.That(fixture.AuditRecords[0], Does.Not.Contain(fixture.Patient.MRN));

        fixture.ViewModel.SearchPhrase = "no matching records";
        Assert.That(fixture.AuditRecords, Has.Count.EqualTo(1));
    }

    [Test]
    public void NoAuthorizedUser_DoesNotRecordListOrProfileViews()
    {
        var fixture = CreateFixture(false);
        fixture.ViewModel.AuthorizedUserStore.AuthorizedUser = null;

        fixture.ViewModel.SetPatientListVisible(true);
        fixture.ViewModel.EditPatientCommand.Execute();

        Assert.That(fixture.AuditRecords, Is.Empty);
    }

    [Test]
    public void FailedListRead_DoesNotRecordView()
    {
        var fixture = CreateFixture(false);
        fixture.Patients.Object.Patients.Clear();
        fixture.Patients.Setup(model => model.QueryPatientsAsync()).ThrowsAsync(new InvalidOperationException());

        fixture.ViewModel.RetryFetchPatientListCommand!.Execute();
        fixture.ViewModel.SetPatientListVisible(true);

        Assert.That(fixture.ViewModel.FetchPatientListTask!.Task.IsFaulted, Is.True);
        Assert.That(fixture.AuditRecords, Is.Empty);
    }

    private static Fixture CreateFixture(bool openForEditing = true)
    {
        var patient = new Patient
        {
            Id = 23,
            FirstName = "Existing",
            LastName = "Patient",
            ProviderId = "provider@example.com",
            MRN = "MRN23",
            DOB = new DateOnly(1980, 1, 1)
        };
        var patients = new Mock<IPatientListModel>();
        patients.SetupGet(model => model.Patients).Returns(new ObservableCollection<IPatient> { patient });
        patients.Setup(model => model.GetPatientById(It.IsAny<long>())).Returns(patient);
        patients.Setup(model => model.SavePatientAsync(It.IsAny<IPatient>()))
            .ReturnsAsync((IPatient value) => new Patient(value));
        var records = new List<string>();
        var log = new Mock<ILogRepository>();
        log.Setup(writer => writer.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()))
            .Callback<string, LogRecordSeverity, LogRecordType>((message, _, type) =>
            {
                if (type == LogRecordType.User)
                    records.Add(message);
            })
            .Returns(Task.CompletedTask);
        var users = new AuthorizedUserStore
        {
            AuthorizedUser = new User { Id = 17, Username = "operator" }
        };
        var controller = new Mock<ITreatmentInfoStoreController>();
        controller.Setup(value => value.TryPrepareLoadedPlanAsync()).ReturnsAsync((IPlan)null!);
        var viewModel = new PatientViewModel(
            Mock.Of<IRegionManager>(), new EventAggregator(), Mock.Of<ITreatmentInfoStore>(),
            controller.Object, Mock.Of<IEmrPlanCommands>(), log.Object,
            new ActionAuditService(log.Object, users), patients.Object, Mock.Of<IDialogService>(), users)
        {
            SelectedPatient = patient
        };
        if (openForEditing)
        {
            viewModel.EditPatientCommand.Execute();
            records.Clear();
        }
        return new Fixture(viewModel, patients, patient, records);
    }

    private sealed record Fixture(
        PatientViewModel ViewModel,
        Mock<IPatientListModel> Patients,
        Patient Patient,
        List<string> AuditRecords);
}
