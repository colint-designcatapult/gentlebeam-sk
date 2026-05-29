using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Infra.Persistence.CSV.Types;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for TreatmentFieldName value serialization into/from custom CSV format
    /// </summary>
    public class CsvTreatmentFieldName : CsvMappedType<TreatmentFieldName>
    {
        private static IDictionary<TreatmentFieldName, string> map = Enum.GetValues<TreatmentFieldName>().ToDictionary(x => x, x => ToCsvName(x));
        private static CsvValueMap<TreatmentFieldName> mapping = new(map);

        public CsvTreatmentFieldName(TreatmentFieldName value = 0) : base(value, mapping)
        {    
        }

        private static string ToCsvName(TreatmentFieldName x)
        {
            string name = x.ToString();
            if (name.StartsWith("Plus"))
            {
                return name.Replace("Plus", "FIELD_NAME_PLUS_");
            }
            else
            {
                return name.Replace("Minus", "FIELD_NAME_MINUS_");
            }
        }
    }
}
