using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models
{
    public interface IOutputFactor : IEntry
    {
        DateTime CreationDate { get; set; }
        long PresetConfigurationId { get; set; }
        TreatmentFieldName FieldName { get; set; }
        double? Factor { get; set; }
}
}
