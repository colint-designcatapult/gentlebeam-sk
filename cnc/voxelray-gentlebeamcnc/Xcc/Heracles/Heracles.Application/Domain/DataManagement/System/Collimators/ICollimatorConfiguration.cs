using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public interface ICollimatorConfiguration : IEntry
    {
        DateTime CreationDate { get; set; }
        TargetType Type { get; set; }
        Energy Energy { get; set; }
        SsdType SsdType { get; set; }
        double ReferencedDoseRate { get; set; }
        IList<IPresetConfiguration> Presets { get; }  
        IPresetConfiguration? DefaultPreset { get; }
        void AddPreset(IPresetConfiguration preset);
        void SetPresets(IEnumerable<IPresetConfiguration> preset);
        bool IsSame(ICollimatorConfiguration other);
    }
}
