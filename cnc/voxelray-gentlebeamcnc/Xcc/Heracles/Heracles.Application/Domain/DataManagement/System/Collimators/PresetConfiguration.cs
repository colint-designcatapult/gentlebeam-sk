using Empyrean.Common.Core.Domain.DataManagement.Common;
using System;
using Xcc.Core.Common;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public class PresetConfiguration : BaseEntry, IPresetConfiguration
    {
        public PresetConfiguration()
        {            
        }
        public PresetConfiguration(IPresetConfiguration entry)
        {
            entry.CopyProperties(this);
        }

        public DateTime CreationDate{ get; set; } = DateTime.Now;
        public string PresetName { get; set; } = string.Empty;
        public long CollimatorConfigurationId { get; set; } = NewEntryId;
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public string ApprovedBy { get; set; } = string.Empty;
        public bool IsApproved => !string.IsNullOrEmpty(ApprovedBy);
    }
}
