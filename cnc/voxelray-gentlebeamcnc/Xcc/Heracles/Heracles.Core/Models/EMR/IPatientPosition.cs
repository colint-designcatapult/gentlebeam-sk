using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IPatientPosition : IEntry
    {
        DateTime CreationDate { get; set; }
        long SimulationId { get; set; }
        PatientPosition Position { get; set; }
    }
}
