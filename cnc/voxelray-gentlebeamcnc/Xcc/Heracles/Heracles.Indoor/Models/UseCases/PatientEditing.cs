using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Core.Models.EMR;

using System;
using System.Threading.Tasks;

using Xcc.Core.Models;

namespace Heracles.Indoor.Models.UseCases
{
    public interface IPatientProfileForm : IDirtyFlaggedBindableBase
    {
        DateTime? DOB { get; set; }
        IValidatedPatient? FormData { get; }
        bool IsCalendarVisible { get; set; }

        void ShowForm(IPatient? patient);

        void HideForm();
    }

    public interface IPatientSelection
    {
        IPatient? SelectedPatient { get; set; }
    }

    public class PatientEditing
    {
        private IPatientProfileForm _patientForm;
        private IPatientSelection _patientSelection;
        private IPatientListModel _patientListModel;
        private IPatient? _prevSelectedPatient;
        private Task FormTask;

        public PatientEditing(
            IPatientProfileForm form,
            IPatientSelection listModel,
            IPatientListModel patientListModel,
            IPatient patientToEdit
            )
        {
            _patientForm = form;
            _patientSelection = listModel;
            _patientListModel = patientListModel;

            // store previous selection state:
            _prevSelectedPatient = listModel?.SelectedPatient;

            // Reset current selection for a new patient:
            if (patientToEdit == null && _patientSelection != null)
            {
                _patientSelection.SelectedPatient = null;
            }

            _patientForm.ShowForm(patientToEdit);
        }

        /// <summary>
        /// Performs "Submit" action on patient form trying to save a new/edited profile
        /// </summary>
        /// <returns></returns>
        /// <throws>PatientExistsException or DataServiceException</throws>
        public async Task<IPatient> SavePatientAsync()
        {
            IPatient patientToSave = new Patient(_patientForm.FormData);

            IPatient patient = await _patientListModel.SavePatientAsync(patientToSave);

            FinalizeEditing(patient);
            return patient;
        }

        public void Cancel()
        {
            FinalizeEditing(_prevSelectedPatient);
        }


        private void FinalizeEditing(IPatient? patientToSelect)
        {
            _patientForm.HideForm();

            if (_patientSelection != null)
            {
                IPatient? valueToSelect = (patientToSelect == null) ? null : _patientListModel.GetPatientById(patientToSelect.Id);
                _patientSelection.SelectedPatient = valueToSelect;
            }
        }
    }
}
