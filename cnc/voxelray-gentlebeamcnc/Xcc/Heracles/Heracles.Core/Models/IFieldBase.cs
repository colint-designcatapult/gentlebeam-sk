using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models
{
    public interface IFieldBase : IHasTreatmentFieldName, IEntry
    {
        double DwellTime { get; set; }
        Energy Energy { get; set; }
    }
}
