using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using Prism.Mvvm;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models.RDBMS;

namespace Heracles.Indoor.Models
{
    public class PatientListModel : BindableBase, IPatientListModel
    {
        public IPatientRepository PatientRepository { get; }
        public PatientListModel(
            IPatientRepository patientRepository,
            ILogRepository logWriter)
        {
            LogWriter = logWriter;
            Patients = new ObservableCollection<IPatient>();
            PatientRepository = patientRepository;
        }

        private ObservableCollection<IPatient> _patients;
        public ObservableCollection<IPatient> Patients
        {
            get => _patients;
            private set => SetProperty(ref _patients, value);
        }
        public Task PatientRequestTask { get; private set; }
        public ILogRepository LogWriter { get; }

        /// <summary>
        /// Runs a new query if there is no current on, 
        /// otherwise returns ongoing task
        /// </summary>
        /// <returns></returns>
        public async Task QueryPatientsAsync()
        {
            if (PatientRequestTask != null && !PatientRequestTask.IsCompleted)
            {
                await PatientRequestTask;
            }
            else
            {
                PatientRequestTask = FetchPatientsAsync();
                await PatientRequestTask;
                PatientRequestTask = null;
            }

        }


        /// <summary>
        /// Fetches patients data from DB. 
        /// Now it's the only way to update model's Patients list
        /// </summary>
        /// <returns></returns>
        private async Task FetchPatientsAsync()
        {
            Patients = new ObservableCollection<IPatient>(await PatientRepository.FetchAllPatientsAsync());
        }

        
        private void OnPatientEntryChange(IPatient patient, CRUDEntryChangedAction action)
        {
            switch (action)
            {
                case CRUDEntryChangedAction.Create:
                    Patients.Add(patient);
                    break;
                case CRUDEntryChangedAction.ChangeData:
                    UpdateLocalPatientData(patient);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("The action {0} is not supported on patient entries", action));
            }
        }

        private IPatient UpdateLocalPatientData(IPatient patient)
        {
            IPatient patientToUpdate = Patients.FirstOrDefault(p => p.Id == patient.Id);
            if (patientToUpdate is null)
            {
                throw new InvalidOperationException(String.Format("Invalid patient Id={0}: no such patient in the list", patient.Id));
            }
            // Replace patient record with a new one
            return Patients[Patients.IndexOf(patientToUpdate)] = patient;
        }

        public async Task<IPatient> SavePatientAsync(IPatient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            if (BaseEntry.IsBlankEntry(patient))
            {
                var createdPatient = await PatientRepository.CreateAsync(patient);
                OnPatientEntryChange(createdPatient, CRUDEntryChangedAction.Create);
                return createdPatient;
            }
            else {
                var savedPatient = await PatientRepository.UpdateAsync(GetPatientById(patient.Id), patient);
                OnPatientEntryChange(savedPatient, CRUDEntryChangedAction.ChangeData);
                return savedPatient;
            }
        }

        public async Task<IVisit> GetSameDayVisitAsync(IPatient? patient, VisitType visitTypeToBeCreated)
        {
            return await PatientRepository.GetSameDayVisitAsync(patient, DateTime.Now, visitTypeToBeCreated);
        }

        private bool LookForExistingPatientMatch(IPatient newPatient)
        {
            return null != Patients.FirstOrDefault(
                (existsingPatient) =>
                    existsingPatient.Id != newPatient.Id &&
                    existsingPatient.MRN != null &&
                    existsingPatient.MRN.Equals(newPatient.MRN)
                );
        }

        public IPatient GetPatientById(long id)
        {
            return Patients.First(p => p.Id == id);
        }
    }
}
