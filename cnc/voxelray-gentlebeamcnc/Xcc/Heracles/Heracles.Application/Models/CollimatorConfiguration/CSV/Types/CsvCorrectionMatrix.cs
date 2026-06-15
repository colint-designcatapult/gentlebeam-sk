using Xcc.Core.Domain.DataManagement.System;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for CorrectionMatrix serialization into/from custom CSV format
    /// </summary>
    public class CsvCorrectionMatrix
    {
        public CsvMagnetometerType MagnetometerType { get; } = new(Xcc.Core.Enums.MagnetometerType.Front);
        public double CM11 { get; set; } = 0;
        public double CM12 { get; set; } = 0;
        public double CM13 { get; set; } = 0;
        public double CM21 { get; set; } = 0;
        public double CM22 { get; set; } = 0;
        public double CM23 { get; set; } = 0;

        public CsvCorrectionMatrix()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matrix"></param>
        /// <throws>NullReferenceException</throws>
        public CsvCorrectionMatrix(ICorrectionMatrix matrix)
        {
            MagnetometerType = new(matrix.MagnetometerType);
            CM11 = matrix.Cm11;
            CM12 = matrix.Cm12;
            CM13 = matrix.Cm13;
            CM21 = matrix.Cm21;
            CM22 = matrix.Cm22;
            CM23 = matrix.Cm23;
        }
    }

}
