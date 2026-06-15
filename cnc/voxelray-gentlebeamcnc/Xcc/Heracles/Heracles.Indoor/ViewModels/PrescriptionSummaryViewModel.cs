using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;

namespace Heracles.Indoor.ViewModels;

public class PrescriptionSummaryViewModel : BindableBase
{
    public PrescriptionSummaryViewModel(
        ITreatmentInfoStore treatmentInfoStore,
        ITreatmentDoseCalculation treatmentDoseCalculation,
        ICollimatorModel collimatorModel)
    {
        TreatmentInfoStore = treatmentInfoStore;
        TreatmentDoseCalculation = treatmentDoseCalculation;
        CollimatorModel = collimatorModel;
        treatmentInfoStore.PrescriptionChanged += (_, p) => RecalculateActualDose(p);
        treatmentDoseCalculation.OutputFactorsChanged += (_,_) => RecalculateActualDose(treatmentInfoStore.Prescription);
    }

    public ITreatmentInfoStore TreatmentInfoStore { get; }
    public ITreatmentDoseCalculation TreatmentDoseCalculation { get; }
    public ICollimatorModel CollimatorModel { get; }

    private double? _actualDose;
    public double? ActualDose
    {
        get => _actualDose;
        set => SetProperty(ref _actualDose, value);
    }

    private void RecalculateActualDose(IPrescription? prescription)
    {
        try
        {
            var energy = prescription?.Energy;
            var targetType = TreatmentInfoStore.Simulation?.TargetType;

            ActualDose = null;

            if (energy is not null &&
                targetType is not null)
            {
                var collimatorConfig = CollimatorModel.FindConfigurationByType(
                    targetType.Value,
                    energy.Value);

                if (collimatorConfig is not null)
                    ActualDose = TreatmentDoseCalculation.CalculateDose(
                        Application.Models.PlanModel.DefaultTreatmentFieldName,
                        collimatorConfig,
                        prescription!.DwellTime);
            }
        }
        catch 
        {
            // ignore
        }
    }

}
