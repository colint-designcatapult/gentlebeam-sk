using Heracles.Core.Enums;
using Heracles.Core.Models;

using System;
using System.ComponentModel.DataAnnotations;

using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Models;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public interface IOutputFactorEntry : IOutputFactor, IDirtyFlaggedBindableBase
    {
        public string DisplayName { get; set; }
    }

    public class OutputFactorEntry : DirtyFlaggedBindableBase, IOutputFactorEntry
    {
        public string DisplayName { get; set; }
        public DateTime CreationDate { get; set; }
        public long PresetConfigurationId { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public TreatmentFieldName FieldName { get; set; }

        private double? _factor;
        [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
        [NumericRange(PhysicsValueRange.OutputFactorMin, PhysicsValueRange.OutputFactorMax)]
        public double? Factor
        {
            get => _factor;
            set
            {
                SetPropertyWithDirtyFlag(ref _factor, value);
                Validate(value);
            }
        }

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public OutputFactorEntry(IOutputFactor entry = null) 
        {
            entry?.CopyProperties(this);
        }
    };
}
