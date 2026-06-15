using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Heracles.Application.Models.EMR
{
    public interface IPatientListModel : INotifyPropertyChanged
    {
        Task PatientRequestTask { get; }
        ObservableCollection<IPatient> Patients { get; }
        Task QueryPatientsAsync();
        Task<IVisit> GetSameDayVisitAsync(IPatient? patient, VisitType visitType);
        
        Task<IPatient> SavePatientAsync(IPatient patientToSave);
        IPatient GetPatientById(long id);
    }
}
