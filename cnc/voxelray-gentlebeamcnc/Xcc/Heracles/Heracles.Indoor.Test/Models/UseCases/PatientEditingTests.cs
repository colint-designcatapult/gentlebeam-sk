using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models.UseCases;

using Moq;

using Xcc.Core.Exceptions;

namespace Heracles.Indoor.Test.Models.UseCases
{
    internal class PatientEditingTests
    {
        private IValidatedPatient validPatientObject;
        private Mock<IPatientProfileForm> fakePatientForm;
        private Mock<IPatientSelection> fakePatientSelection;
        private Mock<IPatientListModel> fakePatientListModel;

        private PatientEditing patientEditing;

        [SetUp]
        public void Setup()
        {
            validPatientObject = new Patient()
            {
                FirstName = "John",
                LastName = "Doe",
                MRN = "123123",
                DOB = DateOnly.FromDateTime(DateTime.Now)
            };

            fakePatientForm = new Mock<IPatientProfileForm>();
            
            fakePatientSelection = new Mock<IPatientSelection>();
            fakePatientSelection.SetupGet(x => x.SelectedPatient).Returns(validPatientObject);

            fakePatientListModel = new Mock<IPatientListModel>();
            fakePatientListModel.Setup(x => x.GetPatientById(It.IsAny<long>())).Returns(validPatientObject);
            fakePatientListModel.Setup(x => x.SavePatientAsync(It.IsAny<IPatient>())).Returns(Task.FromResult((IPatient)validPatientObject));

            patientEditing = new PatientEditing(
                fakePatientForm.Object,
                fakePatientSelection.Object,
                fakePatientListModel.Object,
                validPatientObject
                );
        }

        [Test]
        public void Constructor_ShowsForm()
        {
            fakePatientForm.Verify(form => form.ShowForm(validPatientObject), Times.Once());
        }

        [Test]
        public void SavePatientAsync()
        {
            IPatient? result = null;

            // Get valid form:
            fakePatientForm.SetupGet(form => form.FormData).Returns(validPatientObject);

            Assert.DoesNotThrow(() => result = patientEditing.SavePatientAsync().GetAwaiter().GetResult());
            Assert.That(result, Is.Not.Null);
            fakePatientForm.Verify(form => form.HideForm(), Times.Once());
        }

        [Test]
        public void SavePatientAsync_FailsOnDataServiceError()
        {
            // Get valid form:
            fakePatientForm.SetupGet(form => form.FormData).Returns(validPatientObject);

            // Get DataServiceException exception from data service
            fakePatientListModel.Setup(x => x.SavePatientAsync(It.IsAny<IPatient>())).Throws<DataServiceException>();

            Assert.Throws<DataServiceException>(() => patientEditing.SavePatientAsync().GetAwaiter().GetResult());

            fakePatientForm.Verify(form => form.HideForm(), Times.Never());
        }

        [Test]
        public void SavePatientAsync_FailsAndShowsDialogOnPatientExistError()
        {
            // Get valid form:
            fakePatientForm.SetupGet(form => form.FormData).Returns(validPatientObject);

            // Get PatientExistsException from data service
            fakePatientListModel.Setup(x => x.SavePatientAsync(It.IsAny<IPatient>())).Throws<PatientExistsException>();

            Assert.Throws<PatientExistsException>(() => patientEditing.SavePatientAsync().GetAwaiter().GetResult());

            fakePatientForm.Verify(form => form.HideForm(), Times.Never());
        }

        [Test]
        public void SavePatientAsync_FailsOnSomeOtherError()
        {
            // Get valid form:
            fakePatientForm.SetupGet(form => form.FormData).Returns(validPatientObject);

            // Get plain exception from data service
            fakePatientListModel.Setup(x => x.SavePatientAsync(It.IsAny<IPatient>())).Throws<Exception>();

            Assert.Throws<Exception>(() => patientEditing.SavePatientAsync().GetAwaiter().GetResult());
            fakePatientForm.Verify(form => form.HideForm(), Times.Never());
        }


        [Test]
        public void Cancel_ClosesForm()
        {
            patientEditing.Cancel();

            fakePatientForm.Verify(form => form.HideForm(), Times.Once());
        }

        [Test]
        public void Cancel_RestoresSelection()
        {
            patientEditing.Cancel();

            fakePatientSelection.VerifySet(x => x.SelectedPatient = validPatientObject, Times.Once());
        }
    }
}
