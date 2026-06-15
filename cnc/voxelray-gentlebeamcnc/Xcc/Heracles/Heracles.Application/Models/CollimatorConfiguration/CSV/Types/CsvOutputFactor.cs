
using Heracles.Core.Models;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for OutputFactor serialization into/from custom CSV format
    /// </summary>
    public class CsvOutputFactor
    {
        public CsvTreatmentFieldName Field { get; } = new(0);
        public double Factor { get; set; } = 0;

        public CsvOutputFactor() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="outputFactor"></param>
        /// <throws>NullReferenceException</throws>
        public CsvOutputFactor(IOutputFactor outputFactor)
        {
            Field.Value = outputFactor.FieldName;
            Factor = outputFactor.Factor.Value;
        }
    }
}
