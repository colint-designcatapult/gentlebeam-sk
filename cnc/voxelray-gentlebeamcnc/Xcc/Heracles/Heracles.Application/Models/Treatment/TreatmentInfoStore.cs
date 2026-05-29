using Heracles.Core.Models.EMR;

using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Heracles.Application.Models.Treatment
{
    public interface ITreatmentInfoStore : INotifyPropertyChanged
    {
        IPatient Patient { get; set; }
        public event EventHandler<IPatient> PatientChanged;

        IDiagnosis Diagnosis { get; set; }
        public event EventHandler<IDiagnosis> DiagnosisChanged;

        ISimulation Simulation { get; set; }
        public event EventHandler<ISimulation> SimulationChanged;

        public ICollection<ITreatmentDevice> TreatmentDevices { get; set; }
        public event EventHandler<ICollection<ITreatmentDevice>> TreatmentDevicesChanged;

        public ICollection<IPatientPosition> PatientPositions { get; set; }
        public event EventHandler<ICollection<IPatientPosition>> PatientPositionsChanged;

        IPrescription Prescription { get; set; }
        public event EventHandler<IPrescription> PrescriptionChanged;
        public event EventHandler<IPrescription> PrescriptionSaved;

        IPlan Plan { get; set; }
        public event EventHandler<IPlan> PlanChanged;

        /// <summary>
        /// Checks info store state for completeness:
        /// if there's a complete hierarchy from a Patient down to a Plan
        /// </summary>
        /// <returns></returns>
        bool IsComplete();

        void Reset();

        void OnPrescriptionSaved(IPrescription? prescription);

        void SetSimulation(ISimulation simulation, ICollection<ITreatmentDevice> devices, ICollection<IPatientPosition> positions);
    }

    public class TreatmentInfoStore : BindableBase, ITreatmentInfoStore
    {
        private IPatient _patient;
        private IDiagnosis _diagnosis;
        private ISimulation _simulation;
        private IPrescription _prescription;
        private IPlan _plan;

        public IPatient Patient
        {
            get => _patient;
            set
            {
                if (SetProperty(ref _patient, value))
                { 
                    PatientChanged?.Invoke(this, Patient);
                }
            }
        }
        public event EventHandler<IPatient> PatientChanged;


        public IDiagnosis Diagnosis
        {
            get => _diagnosis; 
            set
            {
                if (SetProperty(ref _diagnosis, value) ||
                    value == null)
                {
                    DiagnosisChanged?.Invoke(this, Diagnosis);
                }
            }
        }
        public event EventHandler<IDiagnosis> DiagnosisChanged;

        public ISimulation Simulation
        {
            get => _simulation;
            set
            {
                if (SetProperty(ref _simulation, value) ||
                    value == null)
                {
                    SimulationChanged?.Invoke(this, Simulation);
                }
            }
        }
        public event EventHandler<ISimulation> SimulationChanged;
        
        public IPrescription Prescription
        {
            get => _prescription; 
            set
            {
                if (SetProperty(ref _prescription, value) ||
                    value == null)
                {
                    PrescriptionChanged?.Invoke(this, Prescription);
                }
            }
        }
        public event EventHandler<IPrescription> PrescriptionChanged;
        public event EventHandler<IPrescription> PrescriptionSaved;

        public IPlan Plan
        {
            get
            {
                //Debug.WriteLine($"TreatmentInfoStore.Plan.Get => id = {_plan?.Id}; {_plan?.Status}");
                return _plan;
            }
            set
            {
                if (SetProperty(ref _plan, value) ||
                    value == null)
                {
                    //Debug.WriteLine($"TreatmentInfoStore.Plan.Set => id = {_plan?.Id}; {_plan?.Status}");
                    PlanChanged?.Invoke(this, Plan);
                }
            }
        }
        public event EventHandler<IPlan> PlanChanged;
        
        private ICollection<ITreatmentDevice> _treatmentDevices = [];
        public ICollection<ITreatmentDevice> TreatmentDevices
        {
            get => _treatmentDevices;
            set
            {
                if (SetProperty(ref _treatmentDevices, value) || value == null)
                    TreatmentDevicesChanged?.Invoke(this, TreatmentDevices);
            }
        }
        public event EventHandler<ICollection<ITreatmentDevice>> TreatmentDevicesChanged;


        private ICollection<IPatientPosition> _patientPositions = [];
        public ICollection<IPatientPosition> PatientPositions
        {
            get => _patientPositions;
            set
            {
                if (SetProperty(ref _patientPositions, value) || value == null)
                    PatientPositionsChanged?.Invoke(this, PatientPositions);
            }
        }
        public event EventHandler<ICollection<IPatientPosition>> PatientPositionsChanged;

        public bool IsComplete()
        {
            return 
                Patient is not null
                && Diagnosis?.PatientId == Patient.Id
                && Simulation?.DiagnosisId == Diagnosis?.Id
                && Prescription?.SimulationId == Simulation?.Id
                && Plan?.PrescriptionId == Prescription?.Id;
        }

        public void Reset()
        {
            Plan = null;
            Prescription = null;
            Simulation = null;
            Diagnosis = null;
            Patient = null;
        }

        public void OnPrescriptionSaved(IPrescription? prescription)
        {
            if (prescription == null) 
                PrescriptionSaved?.Invoke(this, Prescription);
            else
                PrescriptionSaved?.Invoke(this, prescription);
        }

        public void SetSimulation(ISimulation simulation, ICollection<ITreatmentDevice> devices, ICollection<IPatientPosition> positions)
        {
            Simulation = simulation;
            TreatmentDevices = devices;
            PatientPositions = positions;
        }
    }
}
