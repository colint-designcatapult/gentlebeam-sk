using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Models;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;

using System.ComponentModel;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.External.Models
{
    public interface IPlanModel : INotifyPropertyChanged, IApplicatorReadinessSource
    {
        IPrescription Prescription { get; }
        IPlan Plan { get; }
        TreatmentFieldEntryObservableCollection TreatmentFields { get; }
        ITreatmentFieldEntry SelectedTreatmentField { get; set; }
        double TotalDuration { get; }
        IDiagnosis Diagnosis { get; set; }
        ISimulation Simulation { get; set; }
        Task<bool> UnloadFromTreatmentAsync();
        Task TreatmentLoadAcknowledgeAsync();
        void SetPlan(ITreatmentInfoStore store);
        Task<IPlan> FindPendingPlanAsync();
        Task<IPlan> FindLoadedPlanAsync();
        Task LoadPlanForTreatment(long planId, bool isPartial);

        void UpdateActualTime(GcbOperationalPoint currentEmission);
    }

    public class PlanModel(
        IEmrPlanCommands planCommands,
        ICollimatorModel collimatorModel) : BindableBase, IPlanModel
    {
        private CancellationTokenSource _cancellationTokenSource = null;

        #region Read-only properties
        private IEmrPlanCommands PlanCommands { get; } = planCommands;
        public ICollimatorModel CollimatorModel { get; } = collimatorModel;

        #endregion Read-only properties


        #region Properties

        private IPlan _plan;
        public IPlan Plan
        {
            get => _plan;
            private set
            {
                if (SetProperty(ref _plan, value))
                {
                    if (_plan == null)
                    {
                        TreatmentFields.Clear();
                        TotalDuration = 0.0;
                        CollimatorConfiguration = null;
                    }
                }
            }
        }

        private IPrescription _prescription;
        public IPrescription Prescription
        {
            get => _prescription;
            private set => SetProperty(ref _prescription, value);
        }

        public IDiagnosis Diagnosis { get; set; }
        public ISimulation Simulation { get; set; }


        private TreatmentFieldEntryObservableCollection _treatmentFields = new();
        public TreatmentFieldEntryObservableCollection TreatmentFields
        {
            get => _treatmentFields;
            set
            {
                if (SetProperty(ref _treatmentFields, value))
                {
                    SelectedTreatmentField = null;
                }
            }
        }

        private ITreatmentFieldEntry _selectedTreatmentField;
        public ITreatmentFieldEntry SelectedTreatmentField
        {
            get => _selectedTreatmentField;
            set
            {
                SetProperty(ref _selectedTreatmentField, value);
            }
        }

        public double TotalDuration { get; private set; }

        // TODO: do we need it here? 
        // We use it only to retreive its ActualDose for TF updates or its Id for Qc check
        private ICollimatorConfiguration? _collimatorConfiguration;
        public ICollimatorConfiguration? CollimatorConfiguration
        {
            get => _collimatorConfiguration;
            private set => SetProperty(ref _collimatorConfiguration, value);
        }
        #endregion

        #region Public methods

        public void SetPlan(ITreatmentInfoStore store)
        {
            var plan = store.Plan;
            var prescription = store.Prescription;
            var rawTreatmentFields = plan?.TreatmentFields;

            if (plan is not null)
            {
                TreatmentPlanFieldRules.EnsureValid(rawTreatmentFields);
                if (prescription is null || rawTreatmentFields.Single().Energy != prescription.Energy)
                {
                    throw new InvalidOperationException(
                        "The PlusC treatment field energy must match the prescription energy.");
                }
            }

            if (Plan == plan)
                return;

            var treatmentFields = new TreatmentFieldEntryObservableCollection();
            ICollimatorConfiguration? collimatorConfiguration = null;
            var totalDuration = 0.0;

            if (plan is not null)
            {
                var fieldNameMapping =
                    TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(plan.CollimatorType);
                var field = rawTreatmentFields.Single();
                var treatmentField = new TreatmentFieldEntry(
                    field,
                    TargetTypeConverter.GetBackwardFieldNameMapping(fieldNameMapping, field.Name));
                treatmentFields.Add(treatmentField);
                totalDuration = field.DwellTime;
                collimatorConfiguration =
                    CollimatorModel.FindConfigurationByType(plan.CollimatorType, prescription!.Energy);
            }

            Diagnosis = store.Diagnosis;
            Simulation = store.Simulation;
            Prescription = prescription;
            Plan = plan;
            CollimatorConfiguration = collimatorConfiguration;
            TreatmentFields = treatmentFields;
            TotalDuration = totalDuration;
        }

        private Task FetchTreatmentFactors()
        {
            CollimatorConfiguration = CollimatorModel.FindConfigurationByType(Plan.CollimatorType, Prescription.Energy);
            
            // TODO: we don't use output factors for 1-point applicators, as they're always equal to 1
            //await TreatmentDoseCalculation.FetchTreatmentFactorsAsync(CollimatorConfiguration, Plan.CollimatorType, Prescription.Energy.Value);
            
            return Task.CompletedTask;
        }

        public async Task TreatmentLoadAcknowledgeAsync()
        {
            if (Plan != null)
            {
                await PlanCommands.TreatmentLoadAcknowledgeAsync(Plan.Id);
                // For now, update plan state manually
                Plan.TreatmentLoadingState = TreatmentLoadingState.Loaded;
            }
        }

        public async Task<bool> UnloadFromTreatmentAsync()
        {
            if (Plan == null)
                return false;

            await PlanCommands.UnloadFromTreatmentAsync(Plan.Id);
            if (Plan != null)
            {
                // For now, update plan state manually
                Plan.TreatmentLoadingState = TreatmentLoadingState.Unloaded;

            }
            return true;
        }

        public Task<IPlan> FindPendingPlanAsync()
        {
            return PlanCommands.FindPendingPlanAsync();
        }

        public Task<IPlan> FindLoadedPlanAsync()
        {
            return PlanCommands.FindLoadedPlanAsync();
        }

        #endregion

        #region Private methods


        public async Task LoadPlanForTreatment(long planId, bool isPartial)
        {
            await PlanCommands.LoadForTreatmentAsync(planId, isPartial);
        }

        public void UpdateActualTime(GcbOperationalPoint currentEmission)
        {
            TreatmentPlanFieldRules.EnsureValid(TreatmentFields);
            TreatmentFields[0].Actual = currentEmission.ActualDuration;
        }

        //private ITreatmentField FindTreatmentField(ITreatmentFieldEntry treatmentFieldEntry)
        //{
        //    if (treatmentFieldEntry is null)
        //        return null;

        //    return TreatmentFields.FirstOrDefault(f => f.Name == treatmentFieldEntry.Name);
        //}
        #endregion
    }
}
