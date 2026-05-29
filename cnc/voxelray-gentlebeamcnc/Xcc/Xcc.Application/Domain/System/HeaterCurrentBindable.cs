using System;
using System.ComponentModel.DataAnnotations;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System;

namespace Xcc.Application.Domain.System
{
    public class HeaterCurrentBindable : DirtyFlaggedBindableBase, IHeaterCurrentConfig
    {
        private double? _heaterCurrent = null;
        [Required(ErrorMessage = StringConstants.Physics.Validation.HeaterCurrentRequired)]
        [NumericRange(Xcc.Core.Constants.PhysicsValueRange.HeaterCurrentMin, Xcc.Core.Constants.PhysicsValueRange.HeaterCurrentMax)]
        public double? HeaterCurrent
        {
            get => _heaterCurrent;
            set
            {
                SetPropertyWithDirtyFlag(ref _heaterCurrent, value);
                Validate(value);
            }
        }

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; }
        public long PresetConfigurationId { get; set; }
        public bool CanSave => IsModified && IsValid;
        public bool IsSet => HeaterCurrent is not null;

        public HeaterCurrentBindable(IHeaterCurrentConfig? source = null)
        {
            source?.CopyProperties(this);
            AcceptChanges();
        }

        public HeaterCurrentBindable(long presetId)
        {
            PresetConfigurationId = presetId;
            AcceptChanges();
        }
    }
}
