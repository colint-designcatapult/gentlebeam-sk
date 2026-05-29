using Xcc.Core.Domain.DataManagement.System;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for ReferenceField serialization into/from custom CSV format
    /// </summary>
    public class CsvReferenceField
    {
        public CsvMagnetometerType MagnetometerType { get; } = new(Xcc.Core.Enums.MagnetometerType.Front);
        public double RF11 { get; set; } = 0;
        public double RF21 { get; set; } = 0;
        public double RF31 { get; set; } = 0;

        public CsvReferenceField()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceField"></param>
        /// <throws>NullReferenceException</throws>
        public CsvReferenceField(IReferenceField referenceField)
        {
            MagnetometerType = new(referenceField.MagnetometerType);
            RF11 = referenceField.Rf11;
            RF21 = referenceField.Rf21;
            RF31 = referenceField.Rf31;
        }
    }
}
