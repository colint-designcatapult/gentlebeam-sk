using Heracles.Core.Models.RDBMS;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for CoilConfiguration serialization into/from custom CSV format
    /// </summary>
    public class CsvCoilConfiguration
    {
        public CsvTreatmentFieldName FieldName { get; } = new(0);
        public double XDeflectionCurrent { get; set; }
        public double YDeflectionCurrent { get; set; }
        public double FocusCurrent { get; set; }

        public CsvCoilConfiguration() { }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="coilConfiguration"></param>
        /// <throws>NullReferenceException</throws>
        public CsvCoilConfiguration(ICoilConfigurationEntry coilConfiguration)
        {
            FieldName.Value = coilConfiguration.FieldName;
            XDeflectionCurrent = coilConfiguration.XDeflectionCurrent;
            YDeflectionCurrent = coilConfiguration.YDeflectionCurrent;
            FocusCurrent = coilConfiguration.FocusCurrent;
        }
    }
}
