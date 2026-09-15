using System.Collections.ObjectModel;
using Heracles.Application.AppLayer.Patient;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models.UseCases;
using Heracles.Indoor.ViewModels;
using Moq;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Diagnosis = Heracles.Application.Models.RDBMS.EMR.Diagnosis;
using Simulation = Heracles.Application.Models.RDBMS.EMR.Simulation;

namespace Heracles.Indoor.Test.ViewModels.PatientsViewModel;

internal sealed class ClinicalReadAuditTests
{
    [Test]
    public async Task SelectSite_RecordsSimulationOnlyAfterSuccessfulRead()
    {
        var fixture = CreateFixture();
        var completed = new TaskCompletionSource<ISimulation?>();
        fixture.Repository.Setup(repository => repository.FetchLatestSimulationAsync(31)).Returns(completed.Task);

        fixture.Sites.ViewSiteCommand.Execute(fixture.Diagnosis);
        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("diagnosis").And.Contain("record id=31"));
        completed.SetResult(new Simulation { Id = 41, DiagnosisId = 31 });
        await fixture.Simulation.CurrentSimulationTask.Task;

        Assert.That(fixture.Simulation.SimulationForm.Id, Is.EqualTo(41));
        Assert.That(fixture.Records, Has.Count.EqualTo(2));
        Assert.That(fixture.Records[1], Does.Contain("simulation").And.Contain("record id=41").And.Contain("user id=17"));
    }

    [Test]
    public async Task ProgrammaticDiagnosisLoadAndRefresh_DoNotCreateUserReadRecords()
    {
        var fixture = CreateFixture();
        fixture.Store.Diagnosis = fixture.Diagnosis;
        await fixture.Simulation.CurrentSimulationTask.Task;
        fixture.Store.Diagnosis = new Diagnosis(fixture.Diagnosis);
        await fixture.Simulation.CurrentSimulationTask.Task;

        Assert.That(fixture.Simulation.SimulationForm.Id, Is.EqualTo(41));
        Assert.That(fixture.Records, Is.Empty);
    }

    [Test]
    public void FailedSimulationRead_OnlyAuditsTheDiagnosisActuallyDisplayed()
    {
        var fixture = CreateFixture();
        fixture.Repository.Setup(repository => repository.FetchLatestSimulationAsync(31))
            .ThrowsAsync(new InvalidOperationException());

        fixture.Sites.ViewSiteCommand.Execute(fixture.Diagnosis);

        Assert.That(fixture.Simulation.CurrentSimulationTask.Task.IsFaulted, Is.True);
        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("diagnosis").And.Not.Contain("simulation"));
    }

    [Test]
    public async Task CachedClinicalTab_RevisitsRecordViewsButAutomaticTabChangesDoNot()
    {
        var fixture = CreateFixture();
        fixture.Store.Diagnosis = fixture.Diagnosis;
        await fixture.Simulation.CurrentSimulationTask.Task;
        var tabs = new ClinicalDataTabsViewModel(Mock.Of<IRegionManager>(), fixture.Simulation.EventAggregator,
            fixture.Store, fixture.Fields, fixture.ReadAudit);

        tabs.SelectedTabIndex = 1;
        tabs.SelectedTabIndex = 0;
        Assert.That(fixture.Records, Is.Empty);
        tabs.ViewSelectedTabCommand.Execute(0);
        tabs.ViewSelectedTabCommand.Execute(1);
        tabs.ViewSelectedTabCommand.Execute(0);

        Assert.That(fixture.Records.Count(record => record.Contains("simulation")), Is.EqualTo(2));
        Assert.That(fixture.Records.All(record => record.Contains("patient id=23")), Is.True);
    }

    [Test]
    public async Task CompletingReadAfterUserChange_DoesNotAttributeItToTheNewUser()
    {
        var fixture = CreateFixture();
        var completed = new TaskCompletionSource<ISimulation?>();
        fixture.Repository.Setup(repository => repository.FetchLatestSimulationAsync(31)).Returns(completed.Task);
        fixture.Sites.ViewSiteCommand.Execute(fixture.Diagnosis);
        fixture.Users.AuthorizedUser = new User { Id = 18, Username = "other" };
        completed.SetResult(new Simulation { Id = 41, DiagnosisId = 31 });
        await fixture.Simulation.CurrentSimulationTask.Task;

        Assert.That(fixture.Records, Has.Count.EqualTo(1));
        Assert.That(fixture.Records[0], Does.Contain("user id=17"));
    }

    [Test]
    public async Task CameraJournalReturn_AuditsEachRestoredViewWithoutAuditingAutomaticNavigation()
    {
        var fixture = CreateFixture();
        fixture.Store.Diagnosis = fixture.Diagnosis;
        await fixture.Simulation.CurrentSimulationTask.Task;
        var regions = new Mock<IRegionManager>();
        var region = new Mock<IRegion>();
        var navigation = new Mock<IRegionNavigationService>();
        var journal = new Mock<IRegionNavigationJournal>();
        region.SetupGet(value => value.Name).Returns("Clinical");
        region.SetupGet(value => value.NavigationService).Returns(navigation.Object);
        regions.SetupGet(value => value.Regions["Clinical"]).Returns(region.Object);
        navigation.SetupGet(value => value.Region).Returns(region.Object);
        navigation.SetupGet(value => value.Journal).Returns(journal.Object);
        var tabs = new ClinicalDataTabsViewModel(regions.Object, fixture.Simulation.EventAggregator,
            fixture.Store, fixture.Fields, fixture.ReadAudit);
        var clinicalContext = new NavigationContext(navigation.Object, new Uri("ClinicalDataTabsView", UriKind.Relative));
        clinicalContext.Parameters.Add("UserPatientView", false);
        var camera = new CameraViewModel(regions.Object, Mock.Of<IDialogService>(),
            Mock.Of<IHeraclesMainSettings>(), Mock.Of<ILogRepository>(), Mock.Of<IEmrPhotoCommands>(),
            Mock.Of<IPatientListModel>(), fixture.Store, fixture.Simulation.EventAggregator, fixture.ReadAudit);
        camera.OnNavigatedTo(new NavigationContext(navigation.Object, new Uri("CameraView", UriKind.Relative)));
        journal.Setup(value => value.GoBack()).Callback(() => tabs.OnNavigatedTo(clinicalContext));

        tabs.OnNavigatedTo(clinicalContext);
        Assert.That(fixture.Records, Is.Empty);
        tabs.OnNavigatedFrom(clinicalContext);
        camera.ExitCommand!.Execute();
        tabs.OnNavigatedFrom(clinicalContext);
        camera.ExitCommand.Execute();

        Assert.That(fixture.Records.Count(record => record.Contains("simulation")), Is.EqualTo(2));
        Assert.That(fixture.Records.All(record => record.Contains("patient id=23") && record.Contains("user id=17")), Is.True);
        var count = fixture.Records.Count;
        journal.Setup(value => value.GoBack()).Throws(new InvalidOperationException("Navigation failed"));
        Assert.Throws<InvalidOperationException>(() => camera.ExitCommand.Execute());
        tabs.OnNavigatedTo(clinicalContext);
        Assert.That(fixture.Records, Has.Count.EqualTo(count));
    }

    private static Fixture CreateFixture()
    {
        var records = new List<string>();
        var log = new Mock<ILogRepository>();
        log.Setup(writer => writer.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()))
            .Callback<string, LogRecordSeverity, LogRecordType>((message, _, type) =>
            {
                if (type == LogRecordType.User)
                    records.Add(message);
            }).Returns(Task.CompletedTask);
        var users = new AuthorizedUserStore { AuthorizedUser = new User { Id = 17, Username = "operator" } };
        var store = new TreatmentInfoStore { Patient = new Patient { Id = 23 } };
        var readAudit = new PatientRecordReadAudit(new ActionAuditService(log.Object, users), users, store);
        var events = new EventAggregator();
        events.GetEvent<UnloadFromTreatmentEvent>().SynchronizationContext = new SynchronizationContext();
        var repository = new Mock<ISimulationRepository>();
        repository.Setup(value => value.FetchLatestSimulationAsync(It.IsAny<long>()))
            .ReturnsAsync((long id) => new Simulation { Id = id + 10, DiagnosisId = id });
        repository.Setup(value => value.FetchTreatmentDevicesAsync(It.IsAny<long>()))
            .ReturnsAsync(Array.Empty<ITreatmentDevice>());
        repository.Setup(value => value.FetchPatientPositionsAsync(It.IsAny<long>()))
            .ReturnsAsync(Array.Empty<IPatientPosition>());
        var photos = new Mock<IPhotoService>();
        photos.Setup(value => value.GetPhotosAsync(It.IsAny<long>()))
            .ReturnsAsync((new ObservableCollection<IPhoto>(), (CancellationTokenSource)null!));
        var simulation = new SimulationViewModel(Mock.Of<IRegionManager>(), log.Object,
            Mock.Of<IDialogService>(), repository.Object, store, events, new DisruptiveActionGuard(),
            Mock.Of<IAcquisitionResultStore>(), users, Mock.Of<IPlanModel>(), photos.Object, readAudit);
        var fields = new FieldModel(Mock.Of<IEmrDiagnosisCommands>(), store, log.Object);
        var sites = new SitesViewModel(store, fields, Mock.Of<IRegionManager>(), Mock.Of<IDialogService>(),
            log.Object, events, Mock.Of<IPopUpService>(), readAudit);
        return new Fixture(store, users, new Diagnosis { Id = 31, PatientId = 23 }, repository,
            simulation, sites, fields, readAudit, records);
    }

    private sealed record Fixture(TreatmentInfoStore Store, AuthorizedUserStore Users, Diagnosis Diagnosis,
        Mock<ISimulationRepository> Repository, SimulationViewModel Simulation, SitesViewModel Sites,
        FieldModel Fields, PatientRecordReadAudit ReadAudit, List<string> Records);
}
