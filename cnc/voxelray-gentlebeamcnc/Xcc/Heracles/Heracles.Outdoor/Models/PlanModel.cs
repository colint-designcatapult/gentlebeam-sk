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
    public interface IPlanModel : INotifyPropertyChanged
    {
        IPrescription Prescription { get; }
        IPlan Plan { get; }
        TreatmentFieldEntryObservableCollection TreatmentFields { get; }
        ITreatmentFieldEntry SelectedTreatmentField { get; set; }
        double TotalDuration { get; }
        IDiagnosis Diagnosis { get; set; }
        ISimulation Simulation { get; set; }
        ICollimatorConfiguration CollimatorConfiguration { get; }
        Task<bool> UnloadFromTreatmentAsync();
        Task TreatmentLoadAcknowledgeAsync();
        void SetPlan(ITreatmentInfoStore store);
        Task<IPlan> FindPendingPlanAsync();
        Task<IPlan> FindLoadedPlanAsync();
        Task LoadPlanForTreatment(long planId, bool isPartial);

        void UpdateActualTime(GcbEmissionPlan currentPlan);
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


        private TreatmentFieldEntryObservableCollection _treatmentFields;
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
        public ICollimatorConfiguration CollimatorConfiguration { get; private set; }
        #endregion

        #region Public methods

        public void SetPlan(ITreatmentInfoStore store)
        {
            if (Plan == store.Plan)
                return;

            Diagnosis = store.Diagnosis;
            Simulation = store.Simulation;
            Prescription = store.Prescription;
            Plan = store.Plan;

            // TODO: do we need it here? 
            // We use it only to retreive its ActualDose for TF updates or its Id for Qc check
            if (Plan != null && Prescription != null)
            {
                CollimatorConfiguration = CollimatorModel.FindConfigurationByType(Plan.CollimatorType, Prescription.Energy);
            }

            SetTreatmentFields(Plan);
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

        private void CalculateTotalDuration()
        {
            if (Plan == null)
            {
                TotalDuration = 0.0;
                return;
            }

            TotalDuration = TreatmentFields.Sum(tf => tf.DwellTime);
        }

        private void ClearTreatmentFieldLists()
        {
            TreatmentFields = new TreatmentFieldEntryObservableCollection();
        }

        private void SetTreatmentFields(IPlan plan)
        {
            if (BaseEntry.IsNullOrBlankEntry(plan))
            {
                ClearTreatmentFieldLists();
                TotalDuration = 0.0;
                return;
            }


            var fetchedTreatmentFields = plan.TreatmentFields;
            // TODO: this is a workaround for inconsistent plans in the DB
            // Validate plan correctness & remove all fields for any wrong energy:
            var prescribedEnergy = Prescription?.Energy;
            if (prescribedEnergy != null)
            {
                var fields = fetchedTreatmentFields?.ToList() ?? [];
                foreach (var field in fields)
                {
                    if (field.Energy != prescribedEnergy)
                    {
                        fetchedTreatmentFields.Remove(field);
                    }
                }
            }

            fetchedTreatmentFields = fetchedTreatmentFields.OrderBy(field => field.Id).ToList();
            // TODO: workaround - if we have more than one field, we keep only first one and remove all the others
            if (fetchedTreatmentFields.Count > 1)
            {
                fetchedTreatmentFields = fetchedTreatmentFields.Take(1).ToList();
            }
            
            var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(plan?.CollimatorType ?? TargetType.TargetType_None);
            // TODO: probably we can remove this dispatcher invoke now:
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var treatmentFields = new List<TreatmentFieldEntry>(fetchedTreatmentFields.Count);

                foreach (var tf in fetchedTreatmentFields)
                {
                    var tfEntry = new TreatmentFieldEntry(
                        tf,
                        TargetTypeConverter.GetBackwardFieldNameMapping(fieldNameMapping, tf.Name)
                    );

                    treatmentFields.Add(tfEntry);
                }

                TreatmentFields = new TreatmentFieldEntryObservableCollection(treatmentFields);                
            });

            CalculateTotalDuration();
        }

        public async Task LoadPlanForTreatment(long planId, bool isPartial)
        {
            await PlanCommands.LoadForTreatmentAsync(planId, isPartial);
        }

        public void UpdateActualTime(GcbEmissionPlan currentPlan)
        {
            if (currentPlan.TotalPoints != TreatmentFields.Count) 
            {
                throw new ArgumentException("Size of the board plan differs from the plan loaded for treatment");
            }
            for (int i = 0; i < TreatmentFields.Count; i++)
            {
                TreatmentFields[i].Actual = currentPlan[i].ActualDuration;
            }
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
