using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface ITreatmentDevice : IEntry
    {
        public DateTime CreationDate { get; set; }
        public long SimulationId { get; set; }
        public DeviceType DeviceName { get; set; }
    }
}