using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Application.Common;

namespace Heracles.Application.Models.Treatment
{
    public interface IPrescriptionRepository
    {
        Task<IPrescription> FetchLatestPrescriptionAsync(long simulationId);
        Task<IPrescription> FetchAsync(long prescriptionId);
        Task<IPrescription> SubmitAsync(IPrescription prescription, IPrescription initialState = null);
    }

    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly IEmrPrescriptionCommands emrPrescriptionCommands;

        public async Task<IPrescription> FetchLatestPrescriptionAsync(long simulationId)
        {
            var prescriptions = await emrPrescriptionCommands.ReadListAsync(simulationId);
            var latestPrescription = prescriptions?.OrderBy(p => p.Id).LastOrDefault();
            return latestPrescription;
        }

        public Task<IPrescription> FetchAsync(long prescriptionId)
        {
            return emrPrescriptionCommands.ReadAsync(prescriptionId);
        }

        public Task<IPrescription> SubmitAsync(IPrescription prescription, IPrescription initialPrescriptionState)
        {
            if (prescription == null)
            {
                throw new ArgumentNullException(nameof(prescription), StringConstants.EMR.PrescriptionRepositoryPrescriptionCantBeNull);
            }

            if (prescription.Id == BaseEntry.NewEntryId)
            {
                return emrPrescriptionCommands.CreateAsync(prescription);
            }

            return emrPrescriptionCommands.UpdateAsync(initialPrescriptionState, prescription);
        }
        
        public PrescriptionRepository(
            IEmrPrescriptionCommands emrPrescriptionCommands)
        {
            this.emrPrescriptionCommands = emrPrescriptionCommands;
        }
    }
}
