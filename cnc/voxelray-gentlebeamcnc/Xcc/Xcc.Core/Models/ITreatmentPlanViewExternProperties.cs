using System.Collections.ObjectModel;

namespace Xcc.Core.Models
{
    public interface ITreatmentPlanViewExternProperties
    {
        public int EmissionStepSelected { get; set; }
        public ObservableCollection<bool> SelectedTargets { get; set; }
        public int State { get; set; }
    }
}
