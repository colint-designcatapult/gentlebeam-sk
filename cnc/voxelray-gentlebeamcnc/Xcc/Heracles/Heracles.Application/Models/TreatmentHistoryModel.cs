using Heracles.Application.Helpers;
using Heracles.Core.Commands;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;

using Prism.Mvvm;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Heracles.Application.Models
{
    public interface ITreatmentHistoryModel : INotifyPropertyChanged
    {
        ObservableCollection<ITreatmentBindable> Treatments { get; }
        ITreatmentBindable? SelectedTreatment { get; set; }

        void SetContext(IDiagnosis diagnosis, IPrescription prescription, IPlan plan);
        Task FetchTreatmentsAsync();
        Task<bool> UpdateTreatmentsAsync();

        IPlan? Plan { get; }
        ITreatmentSummary? Summary { get; }
    }
    public class TreatmentHistoryModel : BindableBase, ITreatmentHistoryModel
    {
        private ObservableCollection<ITreatmentBindable> treatments = new();
        private ITreatmentBindable? selectedTreatment = null;
        private ITreatmentSummary? summary;

        #region Properties
        public ObservableCollection<ITreatmentBindable> Treatments
        {
            get => treatments;
            set
            {
                if (SetProperty(ref treatments, value))
                {
                    SelectedTreatment = null;
                }
            }
        }
        public ITreatmentBindable? SelectedTreatment { get => selectedTreatment; set => SetProperty(ref selectedTreatment, value); }

        public IEmrTreatmentCommands EmrTreatmentCommands { get; }
        public IEmrActualTreatmentFieldCommands EmrActualTreatmentFieldCommands { get; }
        public IDiagnosis? Diagnosis { get; private set; }
        public IPrescription? Prescription { get; private set; }
        public IPlan? Plan { get; private set; }
        public ITreatmentSummary? Summary { get => summary; private set => SetProperty(ref summary, value); }

        #endregion Properties

        public async Task FetchTreatmentsAsync()
        {
            ObservableCollection<ITreatmentBindable> treatments = await QueryTreatmentsListAsync();
            Treatments = treatments;
            Summary = MakeSummary(Treatments);
        }

        private async Task<ObservableCollection<ITreatmentBindable>> QueryTreatmentsListAsync()
        {
            var treatments = new ObservableCollection<ITreatmentBindable>();
            if (Diagnosis is null || Prescription is null || Plan is null)
            {
                return treatments; // Just empty fields, as we have nothing to load
            }

            // Just keep it here, as it may be rewritten while we load the fields asynchronously:
            var prescription = Prescription;
            var plan = Plan;

            var userData = new User { FirstName = "Replace", LastName = "This" };
            var treatmentFieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(plan.CollimatorType);

            IEnumerable<ITreatment> treatmentRawData = await EmrTreatmentCommands.ReadListAsync(plan.Id);
            treatmentRawData = treatmentRawData.OrderBy(t => t.Id);

            foreach (var t in treatmentRawData)
            {
                var actualTreatmentFields = await EmrActualTreatmentFieldCommands.ReadListAsync(t.Id);
                var rawTreatment = new RDBMS.EMR.Treatment(t, plan, actualTreatmentFields);
                
                var treatment = new TreatmentBindable(rawTreatment, prescription.Energy, userData);
                
                foreach (var atf in treatment.ActualTreatmentFields)
                {
                    atf.DisplayValue = TargetTypeConverter.GetBackwardFieldNameMapping(treatmentFieldNameMapping, atf.Name);
                }
                treatments.Add(treatment);
            }

            return treatments;
        }

        // TODO: refactor this later
        // The method serves now for the event of plan unloading from treatment,
        // so we need to check if there are any changes in treatment list, and reflect them (selecting a new treatment if needed)
        // Returns 'true' if some changes took place
        public async Task<bool> UpdateTreatmentsAsync()
        {
            ObservableCollection<ITreatmentBindable> newTreatmentsList = await QueryTreatmentsListAsync();

            ITreatmentBindable? prevSelectedTreatment = null!;
            if (SelectedTreatment is not null)
            {
                prevSelectedTreatment = newTreatmentsList.FirstOrDefault(t => t.Id == SelectedTreatment?.Id);
            }

            ITreatmentBindable? treatmentToSelect = (newTreatmentsList.Count > Treatments.Count) ? newTreatmentsList.Last() : prevSelectedTreatment;
            Treatments = newTreatmentsList;
            SelectedTreatment = treatmentToSelect;
            Summary = MakeSummary(Treatments);
            return true;
        }

        private ITreatmentSummary MakeSummary(ObservableCollection<ITreatmentBindable> treatments)
        {
            var lastTreatment = treatments.LastOrDefault();
            return new TreatmentSummary
            {
                LastTreatment = lastTreatment?.CreationDate,
                FieldIndex = -1,
                FieldName = Diagnosis?.SiteName ?? string.Empty,
                Pathology = Diagnosis?.Pathology ?? null,
                Provider = Diagnosis?.Referring ?? string.Empty,
                TotalFractions = treatments.Count(),
                TotalPlannedFractions = Prescription?.NumberOfFxs ?? 0,
                LastDeliveredDose = lastTreatment?.DailyDose ?? 0.0,
                LastDeliveredEnergy = lastTreatment?.Energy,
                TotalDeliveredDose = lastTreatment?.CumulativeDose ?? 0.0,
                TotalPlannedDose = Prescription?.TotalDose ?? 0.0
            };
        }

        public void SetContext(IDiagnosis diagnosis, IPrescription prescription, IPlan plan)
        {
            Treatments.Clear();
            Diagnosis = diagnosis;
            Prescription = prescription;
            Plan = plan;
            Summary = (diagnosis is null || prescription is null) ? null : MakeSummary(Treatments);
        }

        public TreatmentHistoryModel(
            IEmrTreatmentCommands emrTreatmentCommands,
            IEmrActualTreatmentFieldCommands emrActualTreatmentFieldCommands)
        {
            EmrTreatmentCommands = emrTreatmentCommands;
            EmrActualTreatmentFieldCommands = emrActualTreatmentFieldCommands;
        }
    }
}
