using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;

namespace Heracles.Application.Models.Treatment
{
    public interface IPatientRepository
    {
        Task<ICollection<IPatient>> FetchAllPatientsAsync();
        Task<IPatient> FetchAsync(long patientId);
        Task<IPatient> CreateAsync(IPatient patient);
        Task<IPatient> UpdateAsync(IPatient oldValue, IPatient newValue);


        Task<IVisit> GetSameDayVisitAsync(IPatient? patient, DateTime dateTime, VisitType visitType);
        public Task<ICollection<IVisit>> FetchVisitsAsync(long patientId);
        public Task<IVisit> FetchLastVisitAsync(long patientId);
    }

    public class PatientRepository : IPatientRepository
    {
        private readonly IEmrPatientCommands emrPatientCommands;
        private readonly IEmrVisitCommands emrVisitCommands;

        public PatientRepository(
            IEmrPatientCommands emrPatientCommands,
            IEmrVisitCommands emrVisitCommands)
        {
            if (emrPatientCommands is null || emrVisitCommands is null)
            {
                throw new ArgumentNullException("Error: cannot initialize patient repository - commands object is null");
            }
            this.emrPatientCommands = emrPatientCommands;
            this.emrVisitCommands = emrVisitCommands;
        }

        /// <summary>
        /// Fetches patients data from DB. 
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<IPatient>> FetchAllPatientsAsync()
        {
            try
            {
                var result = await emrPatientCommands.ReadAllAsync();

                if (result != null)
                {
                    foreach (var patient in result)
                    {
                        patient.Visit = await FetchLastVisitAsync(patient.Id);
                    }
                }
                else return [];

                return result;
            }
            catch (DataServiceException ex)
            {
                throw new FetchPatientsException("Protocol error while fetching patient list", ex);
            }
            catch (Exception ex)
            {
                throw new FetchPatientsException("Error while fetching patient list", ex);
            }
        }

        public async Task<IPatient> FetchAsync(long patientId)
        {
            var patient = await emrPatientCommands.ReadAsync(patientId);
            patient.Visit = await FetchLastVisitAsync(patientId);
            return patient;
        }

        public async Task<IPatient> CreateAsync(IPatient patient)
        {
            return await emrPatientCommands.CreateAsync(patient);
        }

        public async Task<IPatient> UpdateAsync(IPatient oldValue, IPatient newValue)
        {
            var savedValue = await emrPatientCommands.UpdateAsync(oldValue, newValue);
            savedValue.CopyProperties(oldValue);

            return oldValue;
        }


        
        #region Visit model/repository
        public async Task<IVisit> GetSameDayVisitAsync(IPatient? patient, DateTime dateTime, VisitType visitTypeToBeCreated)
        {
            if (patient?.Visit is not null && patient.Visit.CreationDate.IsSameUtcDay(dateTime))
                return patient.Visit; // Do nothing, return the current patient visit
            
            var newVisit = new Visit()
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = dateTime,
                PatientId = patient.Id,
                Type = visitTypeToBeCreated
            };

            return await emrVisitCommands.CreateAsync(newVisit);
        }
        
        public async Task<ICollection<IVisit>> FetchVisitsAsync(long patientId)
        {
            return [.. (await emrVisitCommands.ReadListAsync(patientId)).OrderBy(v => v.Id)];
        }

        public async Task<IVisit> FetchLastVisitAsync(long patientId)
        {
            return (await FetchVisitsAsync(patientId)).LastOrDefault();
        }
        #endregion Visit model/repository
    }


    public class FetchPatientsException : Exception
    {

        public FetchPatientsException()
        {
        }

        public FetchPatientsException(string message) : base(message)
        {
        }

        public FetchPatientsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
