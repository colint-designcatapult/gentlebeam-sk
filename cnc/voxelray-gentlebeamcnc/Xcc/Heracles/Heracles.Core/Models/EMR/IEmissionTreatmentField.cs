using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IEmissionTreatmentField : IEntry
    {
        long ActualTreatmentFieldId { get; set; }
        DateTime CreationDate { get; set; }
        double ActualDwellTime { get; set; }
    }
}
