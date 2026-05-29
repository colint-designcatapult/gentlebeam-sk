using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Core.Enums;
using Xcc.Infra.Persistence.CSV.Types;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for MagnetometerType value serialization into/from custom CSV format
    /// </summary>
    public class CsvMagnetometerType : CsvMappedType<MagnetometerType>
    {
        private static IDictionary<MagnetometerType, string> map =
            Enum.GetValues<MagnetometerType>().ToDictionary(
                x => x,
                x => "MAGNETOMETERTYPE_" + x.ToString().ToUpper()
                );
        private static CsvValueMap<MagnetometerType> mapping = new(map);

        public CsvMagnetometerType(MagnetometerType type)
            : base(type, mapping)
        {
        }
    }
}
