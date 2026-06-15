using Heracles.Application.Models.Treatment;
using Prism.Mvvm;

namespace Heracles.Indoor.ViewModels;

public class SimulationSummaryViewModel(ITreatmentInfoStore treatmentInfoStore) : BindableBase
{
    public ITreatmentInfoStore TreatmentInfoStore { get; } = treatmentInfoStore;
}