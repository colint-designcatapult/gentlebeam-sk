using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Core.Common;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public class CollimatorConfiguration : BaseEntry, ICollimatorConfiguration
    {
        public DateTime CreationDate  { get; set; } = DateTime.Now;
        public TargetType Type  { get; set; }
        public Energy Energy  { get; set; }
        /// <summary>
        /// Source to skin distance
        /// </summary>
        public SsdType SsdType  { get; set; }
        public double ReferencedDoseRate { get; set; } = 0;
        public IList<IPresetConfiguration> Presets { get; private set; } = new List<IPresetConfiguration>();
        public IPresetConfiguration? DefaultPreset { get; private set; }

        public CollimatorConfiguration()
        {            
        }

        public CollimatorConfiguration(ICollimatorConfiguration collimatorConfiguration, IList<IPresetConfiguration> presets = null)
        {
            collimatorConfiguration?.CopyProperties(this);
                
            if (presets != null)
            {
                Presets = presets;
            }
            TryChooseDefaultPreset();
        }

        public bool IsSame(ICollimatorConfiguration other)
        {
            return Type == other.Type
                && Energy == other.Energy
                && SsdType == other.SsdType;
        }
        public void AddPreset(IPresetConfiguration preset)
        {
            Presets.Add(preset);

            TryChooseDefaultPreset();
        }

        private void TryChooseDefaultPreset()
        {
            DefaultPreset = Presets.FirstOrDefault(p => p.IsDefault && p.IsActive);
        }

        public void SetPresets(IEnumerable<IPresetConfiguration> presets)
        {
            Presets = presets.ToList();
            TryChooseDefaultPreset();
        }
    }
}