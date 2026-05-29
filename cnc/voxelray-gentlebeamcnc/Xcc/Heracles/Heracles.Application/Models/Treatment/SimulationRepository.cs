using Grpc.Core;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Heracles.Application.Models.Treatment
{
    public interface ISimulationRepository
    {
        Task<ISimulation?> FetchLatestSimulationAsync(long diagnosisId);
        Task<ISimulation?> FetchAsync(long simulationId);
        Task<ISimulation> SubmitAsync(ISimulation simulation, ISimulation initialState = null);

        Task<ICollection<ITreatmentDevice>> FetchTreatmentDevicesAsync(long parentId);
        Task<ICollection<ITreatmentDevice>> SaveTreatmentDeviceListAsync(long parentId, ICollection<DeviceType> newDevices, ICollection<ITreatmentDevice> oldDevices);

        Task<ICollection<IPatientPosition>> FetchPatientPositionsAsync(long parentId);
        Task<ICollection<IPatientPosition>> SavePatientPositionListAsync(long parentId, ICollection<PatientPosition> newPatientPositions, ICollection<IPatientPosition> oldPatientPositions);
    }

    public class SimulationRepository(
        IEmrSimulationCommands emrSimulationCommands,
        IEmrTreatmentDeviceCommands emrTreatmentDeviceCommands,
        IEmrPatientPositionCommands emrPatientPositionCommands,
        ITreatmentInfoStore treatmentInfoStore,
        IAuthorizedUserStore authorizedUserStore,
        IPatientListModel patientListModel,
        ILogWriter logWriter) : ISimulationRepository
    {
        public async Task<ISimulation?> FetchLatestSimulationAsync(long diagnosisId)
        {
            var simulations = await emrSimulationCommands.ReadListAsync(diagnosisId);
            var simulation = simulations?.OrderBy(s => s.Id).LastOrDefault();
            return simulation;
        }

        public Task<ISimulation?> FetchAsync(long simulationId)
        {
            return emrSimulationCommands.ReadAsync(simulationId);
        }

        public async Task<ISimulation> SubmitAsync(ISimulation simulation, ISimulation initialState)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException("SimulationRepository error: simulation can't be null");
            }

            if (BaseEntry.IsBlankEntry(simulation))
            {
                var simulationToCreate = new Simulation(simulation)
                {
                    PerformedBy = authorizedUserStore.AuthorizedUser.EmailAddress,
                    DiagnosisId = treatmentInfoStore.Diagnosis.Id
                };

                // If we create a new simulation, we need to attach it to a new visit.
                // However, we don't create a visit if there's already one for the same date
                
                var lastVisit = treatmentInfoStore.Patient?.Visit;
                try
                {
                    lastVisit = await patientListModel.GetSameDayVisitAsync(treatmentInfoStore.Patient, visitType: VisitType.Simulation);
                    if (treatmentInfoStore.Patient != null)
                    {
                        treatmentInfoStore.Patient.Visit = lastVisit;
                    }
                    if (lastVisit != null && lastVisit == treatmentInfoStore.Patient?.Visit)
                    {
                        _ = logWriter.LogAsync(
                            $"Treatment: there's a same day visit with id={lastVisit.Id}, it will be applied to the new simulation",
                            Xcc.Core.Enums.LogRecordSeverity.Warn, Xcc.Core.Enums.LogRecordType.System);
                    }
                }
                catch (DataServiceException ex) when (ex.InnerException is RpcException exInner)
                {
                    if (exInner.StatusCode == StatusCode.Internal)
                    {
                        _ = logWriter.LogAsync(
                            $"Failed to create a new visit. The previous one with id={lastVisit?.Id} will be applied to the new simulation.{Environment.NewLine}Error: {exInner.Message}",
                            Xcc.Core.Enums.LogRecordSeverity.Warn, Xcc.Core.Enums.LogRecordType.System);
                    }
                    else
                        throw;
                }

                simulationToCreate.VisitId = lastVisit.Id; // Set visit Id
                return await emrSimulationCommands.CreateAsync(simulationToCreate);
            }

            return await emrSimulationCommands.UpdateAsync(initialState, simulation);
        }

        public async Task<ICollection<ITreatmentDevice>> FetchTreatmentDevicesAsync(long parentId)
        {
            return await emrTreatmentDeviceCommands.ReadListAsync(parentId);
        }

        public async Task<ICollection<ITreatmentDevice>> SaveTreatmentDeviceListAsync(long parentId, ICollection<DeviceType> newDevices, ICollection<ITreatmentDevice> oldDevices)
        {
            oldDevices ??= [];

            // First, everything that is missing in the actual state, we remove:
            var toRemove = oldDevices.ExceptBy(newDevices, o => o.DeviceName).ToList();

            foreach (var device in toRemove)
            {
                if (await emrTreatmentDeviceCommands.DeleteAsync(device.Id))
                {
                    oldDevices.Remove(device);
                }
            }

            // Now, everything that is missing in the initial state, we add:
            var devicesToCreate = newDevices.ExceptBy(oldDevices.Select(o => o.DeviceName), n => n).ToList();
            foreach (var device in devicesToCreate)
            {
                var createdDevice = await emrTreatmentDeviceCommands.CreateAsync(new TreatmentDevice(parentId, device));
                oldDevices.Add(createdDevice);
            }

            return oldDevices;
        }


        public async Task<ICollection<IPatientPosition>> FetchPatientPositionsAsync(long parentId)
        {
            return await emrPatientPositionCommands.ReadListAsync(parentId);
        }

        public async Task<ICollection<IPatientPosition>> SavePatientPositionListAsync(long parentId, ICollection<PatientPosition> newPatientPositions, ICollection<IPatientPosition> oldPatientPositions)
        {
            oldPatientPositions ??= [];

            // First, everything that is missing in the actual state, we remove:
            var toRemove = oldPatientPositions.ExceptBy(newPatientPositions, o => o.Position).ToList();

            foreach (var position in toRemove)
            {
                if (await emrPatientPositionCommands.DeleteAsync(position.Id))
                {
                    oldPatientPositions.Remove(position);
                }
            }
            
            // Now, everything that is missing in the initial state, we add:
            var positionsToCreate = newPatientPositions.ExceptBy(oldPatientPositions.Select(o => o.Position), n => n).ToList();

            foreach (var position in positionsToCreate)
            {
                var createdPosition = await emrPatientPositionCommands.CreateAsync(new PatientPositionEntry(parentId, position));
                oldPatientPositions.Add(createdPosition);
            }

            return oldPatientPositions;
        }
    }
}
