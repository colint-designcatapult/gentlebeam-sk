using System;
using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public class CoilConfigurationEntry : ICoilConfigurationEntry
    {
        public CoilConfigurationEntry()
        { }

        public CoilConfigurationEntry(ICoilConfigurationEntry? configuration)
        {
            if (configuration is not null)
            {
                configuration.CopyProperties(this);
            }
        }

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public long PresetConfigurationId { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public TreatmentFieldName FieldName { get; set; }
        public DateTime CreationDate { get; set; }
        public double XDeflectionCurrent { get; set; }
        public double YDeflectionCurrent { get; set; }
        public double FocusCurrent { get; set; }
    }
}
