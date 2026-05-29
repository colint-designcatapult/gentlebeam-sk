using Heracles.Application.Domain.DataManagement.System.Collimators;
using Xcc.Infra.Persistence.CSV.Types;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for PresetConfiguration serialization into/from custom CSV format
    /// </summary>
    public class CsvPreset
    {
        public string PresetName { get; set; } = string.Empty;
        public CsvBool IsDefault { get; set; } = new(false);
        public CsvBool IsActive { get; set; } = new(false);

        public CsvPreset() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="preset"></param>
        /// <throws>NullReferenceException</throws>
        public CsvPreset(IPresetConfiguration preset)
        {
            PresetName = preset.PresetName;
            IsDefault.Value = preset.IsDefault;
            IsActive.Value = preset.IsActive;
        }
    }
}
