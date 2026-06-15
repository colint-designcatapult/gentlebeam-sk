using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.RDBMS
{
    public interface IQcSampleField : IEntry
    {
        DateTime CreationDate { get; set; }
        long QcSampleId { get; set; }
        TreatmentFieldName Name { get; set; }
    }
}
