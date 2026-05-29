using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IVisit : IEntry
    {
        public DateTime CreationDate { get; set; }
        public long PatientId { get; set; }
        public VisitType Type { get; set; }
    }
}
