using Heracles.Application.Models;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.ViewModels;

namespace Heracles.Indoor.Test.ViewModels.PatientsViewModel
{
    internal class PatientProfileFormTests
    {

        private PatientProfileForm form;
        private IPatient patient;

        [SetUp]
        public void Setup()
        {
            form = new PatientProfileForm();
            patient = new Patient()
            {
                FirstName = "John",
                LastName = "Doe",
                MRN = "123123",
                DOB = DateOnly.FromDateTime(DateTime.Now),
                ProviderId = "some@email.com",
                Sex = Xcc.Core.Enums.Sex.Male,
            };
        }

        [Test]
        public void CheckInitialState()
        {
            var newForm = new PatientProfileForm();
            Assert.That(newForm.FormData, Is.Null);
            Assert.That(newForm.IsCalendarVisible, Is.False);
        }

        [Test]
        public void ShowFormAsync()
        {
            form.ShowForm(patient);
            Assert.That(form.FormData, Is.Not.Null);
            Assert.That(form.IsCalendarVisible, Is.False);
        }

        //[Test]
        //public void ValidateFormData()
        //{
        //    form.ShowFormAsync(patient).GetAwaiter().GetResult();

        //    Assert.True(form.IsDataValid);

        //    // Empty the first name field to invalidate the data:
        //    form.FormData.FirstName = "";

        //    Assert.False(form.IsDataValid);
        //}

        [Test]
        public void HideForm()
        {
            form.ShowForm(patient);
            form.IsCalendarVisible = true;
            form.HideForm();

            Assert.That(form.FormData, Is.Null);
            Assert.That(form.IsCalendarVisible, Is.False);
        }
    }
}
