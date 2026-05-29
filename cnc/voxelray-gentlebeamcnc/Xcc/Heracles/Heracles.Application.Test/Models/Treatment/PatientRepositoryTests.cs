using Heracles.Application.Models;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;

using Moq;

using Xcc.Core.Exceptions;

namespace Heracles.Application.Test.Models.Treatment
{
    internal class PatientRepositoryTests
    {
        private Mock<IEmrPatientCommands> fakeEmrPatientCommands;
        private Mock<IEmrVisitCommands> fakeEmrVisitCommands;
        ICollection<IPatient> patientsToReturn = [ new Patient() { Id = 0} ];

        [SetUp]
        public void Setup()
        {
            fakeEmrPatientCommands = new Mock<IEmrPatientCommands>();
            fakeEmrVisitCommands = new Mock<IEmrVisitCommands>();
            fakeEmrVisitCommands
                .Setup(cmd => cmd.ReadListAsync(It.IsAny<long>()))
                .Returns(Task.FromResult((ICollection<IVisit>)[]));
        }

        private IPatientRepository MakeRepository()
        {
            return new PatientRepository(fakeEmrPatientCommands.Object, fakeEmrVisitCommands.Object);
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => MakeRepository());
            Assert.Throws<ArgumentNullException>(() => new PatientRepository(null, fakeEmrVisitCommands.Object));
            Assert.Throws<ArgumentNullException>(() => new PatientRepository(fakeEmrPatientCommands.Object, null));
        }

        [Test]
        public void FetchAllPatientsAsync_ReturnsEmptyListIfNoPatients()
        {
            ICollection<IPatient>? nullRef = null;
            fakeEmrPatientCommands.Setup(
                cmd => cmd.ReadAllAsync()
            ).Returns(Task.FromResult(nullRef));

            var repo = MakeRepository();
            
            ICollection<IPatient>? patients = null;
            Assert.DoesNotThrow(() => patients = repo.FetchAllPatientsAsync().GetAwaiter().GetResult());
            Assert.That(patients, Is.Not.Null);
            Assert.That(patients, Is.Empty);
        }

        [Test]
        public void FetchAllPatientsAsync_PassesDataFromRepository()
        {
            fakeEmrPatientCommands.Setup(
                cmd => cmd.ReadAllAsync()
            ).Returns(Task.FromResult(patientsToReturn));

            var repo = MakeRepository();

            ICollection<IPatient>? patients = null;
            Assert.DoesNotThrow(() => patients = repo.FetchAllPatientsAsync().GetAwaiter().GetResult());
            Assert.That(patients, Is.Not.Null);
            Assert.That(patients.Count, Is.EqualTo(patientsToReturn.Count));
        }

        [Test]
        public void CreateAsync_ThrowsPatientExist()
        {
            // Provide patient list:
            fakeEmrPatientCommands.Setup(
                cmd => cmd.ReadAllAsync()
                ).Returns(Task.FromResult(patientsToReturn));


            var repo = MakeRepository();

            //throw exception if patient already exists
            var duplicatedPatient = patientsToReturn.First();
            fakeEmrPatientCommands.Setup(cmd => cmd.CreateAsync(duplicatedPatient)).Throws<PatientExistsException>();

            // This patient object must already be in the list:
            Assert.Throws<PatientExistsException>(() => repo.CreateAsync(duplicatedPatient).GetAwaiter().GetResult());
        }

        [Test]
        public void TryAddNewVisitAsync()
        {
            // Provide patient list:
            fakeEmrPatientCommands.Setup(
                cmd => cmd.ReadAllAsync()
                ).Returns(Task.FromResult(patientsToReturn));

            // Return the same visit object:
            fakeEmrVisitCommands
                .Setup(cmd => cmd.CreateAsync(It.IsAny<IVisit>()))
                .Returns((IVisit visit) => Task.FromResult(visit));

            var repo = MakeRepository();

            IPatient initialPatient = patientsToReturn.First();
            IVisit? lastVisit = null;
            var currentTime = DateTime.Now;

            Assert.DoesNotThrow(
                () => lastVisit = repo.GetSameDayVisitAsync(
                    initialPatient, currentTime, visitType: Core.Enums.VisitType.Simulation).GetAwaiter().GetResult());
            Assert.That(lastVisit, Is.Not.Null);

            initialPatient.Visit = lastVisit;

            // Second attempt gives the same patient with same visits
            Assert.DoesNotThrow(
                () => lastVisit = repo.GetSameDayVisitAsync(
                    initialPatient, currentTime, visitType: Core.Enums.VisitType.Simulation).GetAwaiter().GetResult());
            Assert.That(lastVisit, Is.Not.Null);
        }
    }
}
