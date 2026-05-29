using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.System;

namespace Heracles.Core.Models.RDBMS
{
    public interface ICoilConfigurationEntry : ISystemPresetEntry
    {
        TreatmentFieldName FieldName { get; set; }
        double XDeflectionCurrent { get; set; }
        double YDeflectionCurrent { get; set; }
        double FocusCurrent { get; set; }
    }    
}
