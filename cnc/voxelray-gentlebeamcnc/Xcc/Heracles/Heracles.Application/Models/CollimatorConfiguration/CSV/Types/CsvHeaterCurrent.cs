namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for HeaterCurrent serialization into/from custom CSV format
    /// </summary>
    public class CsvHeaterCurrent
    {
        public double HeaterCurrent { get; set; } = 0;

        public CsvHeaterCurrent() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="heaterCurrent"></param>
        /// <throws>NullReferenceException</throws>
        public CsvHeaterCurrent(double heaterCurrent)
        {
            HeaterCurrent = heaterCurrent;
        }
    }
}
