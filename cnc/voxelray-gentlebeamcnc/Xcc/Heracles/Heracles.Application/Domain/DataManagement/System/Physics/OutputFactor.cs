using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using System;
using Xcc.Core.Common;

namespace Heracles.Application.Domain.DataManagement.System.Physics
{
    public class OutputFactor : BaseEntry, IOutputFactor
    {
        public DateTime CreationDate { get; set; }
        public long PresetConfigurationId { get; set; }
        public TreatmentFieldName FieldName { get; set; }
        public double? Factor { get; set; }

        public OutputFactor()
        {
        }
        public OutputFactor(IOutputFactor outputFactor = null)
        {
            outputFactor?.CopyProperties(this);
        }
    }
}