using Heracles.Application.Models;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models;
using Moq;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.Test.Models
{
    public class PatientListModelTests
    {
        IPatient patientObject;
        ICollection<IPatient> patientsToReturn;
        Mock<IPatientRepository> fakePatientRepository;
        Mock<ILogRepository> fakeLogService;

        private PatientListModel MakePatientListModel()
        {
            return new PatientListModel(
                            fakePatientRepository.Object,
                            fakeLogService.Object);
        }

        [SetUp]
        public void Setup()
        {
            patientsToReturn = new List<IPatient>();
            patientObject = new Patient() { Id = 0 };
            patientsToReturn.Add(patientObject);

            fakePatientRepository = new();
            fakeLogService = new();
        }

        
        [Test]
        public void PassesDataFromRepository()
        {
            fakePatientRepository.Setup(
                cmd => cmd.FetchAllPatientsAsync()
            ).Returns(Task.FromResult(patientsToReturn));

            var model = MakePatientListModel();

            // Ensure proper list load
            model.QueryPatientsAsync().Wait();

            Assert.That(model.Patients, Is.Not.Empty);
        }
        
        [Test]
        public void SavePatientAsync_ThrowsOnNull()
        {
            var model = new PatientListModel(null, null);

            Assert.Throws<ArgumentNullException>(() => model.SavePatientAsync(null).GetAwaiter().GetResult());
        }

        [Test]
        public void SavePatientAsync_ThrowsPatientExist()
        {
            // Provide patient list:
            fakePatientRepository.Setup(
                cmd => cmd.FetchAllPatientsAsync()
                ).Returns(Task.FromResult(patientsToReturn));


            var model = MakePatientListModel();

            // Ensure proper list load
            model.QueryPatientsAsync().Wait();

            IPatient duplicatePatient = new Patient(patientObject) { Id = BaseEntry.NEW_ENTRY_ID };

            //throw exception if patient already exists
            fakePatientRepository.Setup(cmd => cmd.CreateAsync(duplicatePatient)).Throws<PatientExistsException>();

            // This patient object must already be in the list:
            Assert.Throws<PatientExistsException>(() => model.SavePatientAsync(duplicatePatient).GetAwaiter().GetResult());
        }


        [Test]
        public void SavePatientAsync_ValidNewPatient()
        {
            // Provide patient list and create action:
            fakePatientRepository.Setup(
                cmd => cmd.FetchAllPatientsAsync()
                ).Returns(Task.FromResult(patientsToReturn));

            fakePatientRepository.Setup(x => x.CreateAsync(It.IsAny<IPatient>()))
                .Returns(async (IPatient p) => await Task.FromResult(new Patient(p) { Id = patientObject.Id + 1 }));

            var model = MakePatientListModel();

            // Ensure proper list load
            model.QueryPatientsAsync().Wait();

            IPatient newPatient = new Patient()
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                FirstName = "John",
                LastName = "Doe",
                MRN = patientObject.MRN + "1", // MRN mustn't duplicate
                DOB = DateOnly.FromDateTime(DateTime.Now)
            };

            // This patient object must already be in the list:
            IPatient? savedPatient = null;
            Assert.DoesNotThrow(() => savedPatient = model.SavePatientAsync(newPatient).GetAwaiter().GetResult());
            Assert.That(savedPatient, Is.Not.Null);
            Assert.That(savedPatient.Id, Is.Not.EqualTo(BaseEntry.NEW_ENTRY_ID));

            fakePatientRepository.Verify(x => x.CreateAsync(It.IsAny<IPatient>()), Times.Once());
        }

        [Test]
        public void SavePatientAsync_UpdatesExistingPatient()
        {
            // Provide patient list and create action:
            fakePatientRepository.Setup(
                cmd => cmd.FetchAllPatientsAsync()
                ).Returns(Task.FromResult(patientsToReturn));

            fakePatientRepository.Setup(x => x.UpdateAsync(It.IsAny<IPatient>(), It.IsAny<IPatient>()))
                .Returns(async (IPatient p, IPatient p2) => await Task.FromResult(new Patient(p2)));

            var model = MakePatientListModel();

            // Ensure proper list load
            model.QueryPatientsAsync().Wait();

            IPatient updatedPatient = new Patient(patientObject) { LastName = "UnDoe" };

            // This patient object must already be in the list:
            IPatient? savedPatient = null;
            Assert.DoesNotThrow(() => savedPatient = model.SavePatientAsync(updatedPatient).GetAwaiter().GetResult());
            Assert.That(savedPatient, Is.Not.Null);
            Assert.That(updatedPatient.Id, Is.EqualTo(savedPatient.Id));

            fakePatientRepository.Verify(x => x.UpdateAsync(It.IsAny<IPatient>(), It.IsAny<IPatient>()), Times.Once());
        }

        [Test]
        public void GetPatientById()
        {
            // Provide patient list:
            fakePatientRepository.Setup(
                cmd => cmd.FetchAllPatientsAsync()
                ).Returns(Task.FromResult(patientsToReturn));

            var model = MakePatientListModel();

            // Ensure proper list load
            model.QueryPatientsAsync().Wait();

            IPatient? existingPatient = null;
            Assert.DoesNotThrow(() => existingPatient = model.GetPatientById(patientObject.Id));
            Assert.That(existingPatient, Is.Not.Null);

            Assert.Throws<InvalidOperationException>(() => model.GetPatientById(patientObject.Id + 1));
        }

        [Test]
        public void GetSameDayVisitAsync()
        {
            // Provide patient list:
            fakePatientRepository.Setup(
                cmd => cmd.FetchAllPatientsAsync()
                ).Returns(Task.FromResult(patientsToReturn));

            // Return a patient with new visit in it:
            fakePatientRepository
                .Setup(cmd => cmd.GetSameDayVisitAsync(It.IsAny<IPatient>(), It.IsAny<DateTime>(), It.IsAny<VisitType>()))
                .Returns((IPatient patient, DateTime dateTime, VisitType visitType) 
                    => Task.FromResult((IVisit)new Visit() { Type = visitType }));

            var model = MakePatientListModel();

            // Ensure proper list load
            model.QueryPatientsAsync().Wait();

            IVisit? lastVisit = null;
            Assert.DoesNotThrow(
                () => lastVisit = model.GetSameDayVisitAsync(patientObject, visitTypeToBeCreated: Core.Enums.VisitType.Simulation).GetAwaiter().GetResult());
            Assert.That(lastVisit, Is.Not.Null);
        }
    }
}
