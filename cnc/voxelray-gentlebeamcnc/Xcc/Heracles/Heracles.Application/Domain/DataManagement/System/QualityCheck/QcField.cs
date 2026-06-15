using Heracles.Core.Enums;
using System.Collections.Generic;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public struct QcField(
        TreatmentFieldName name,
        ICollection<double?> values)
    {
        public readonly TreatmentFieldName FieldName => name;
        public readonly ICollection<double?> Values => values;
    }
}
