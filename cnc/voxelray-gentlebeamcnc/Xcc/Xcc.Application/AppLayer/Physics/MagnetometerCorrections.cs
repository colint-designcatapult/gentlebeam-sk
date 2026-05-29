using Xcc.Application.Helpers;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;

namespace Xcc.Application.AppLayer.Physics
{
    public class MagnetometerCorrections : DirtyFlaggedBindableBase
    {
        public MagnetometerCorrections(long presetId)
        {
            FrontMatrix.PresetConfigurationId = presetId;
            BackMatrix.PresetConfigurationId = presetId;
            FrontReferenceField.PresetConfigurationId = presetId;
            BackReferenceField.PresetConfigurationId = presetId;
            
            AcceptChanges();
        }
        public MagnetometerCorrections() : this(BaseEntry.NEW_ENTRY_ID)
        {
            IsValid = false;
            AcceptChanges();
        }

        public CorrectionMatrixForm FrontMatrix { get; set; } = new() { MagnetometerType = MagnetometerType.Front };
        public CorrectionMatrixForm BackMatrix { get; set; } = new() { MagnetometerType = MagnetometerType.Back };

        public ReferenceFieldForm FrontReferenceField { get; set; } = new() { MagnetometerType = MagnetometerType.Front };
        public ReferenceFieldForm BackReferenceField { get; set; } = new() { MagnetometerType = MagnetometerType.Back };

        public sealed override void AcceptChanges()
        {
            FrontMatrix.AcceptChanges();
            BackMatrix.AcceptChanges();
            FrontReferenceField.AcceptChanges();
            BackReferenceField.AcceptChanges();
            IsModified = false;
        }
    };
}
