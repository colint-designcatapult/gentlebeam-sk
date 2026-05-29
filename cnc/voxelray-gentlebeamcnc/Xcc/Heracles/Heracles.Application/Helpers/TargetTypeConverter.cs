using Heracles.Core.Constants;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heracles.Application.Helpers
{
    public class TargetTypeConverter
    {
        public static IDictionary<int, TreatmentFieldName> GetIndexToTreatmentFieldNameMapping(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.TargetType_50mm_SSD_13_Fields:
                    return Mappings.TargetType_13CellsCentralLarge;

                case TargetType.TargetType_30mm_SSD_7_Fields:
                    return Mappings.TargetType_30mmSsd7Fields;

                case TargetType.TargetType_50mm_SSD_15mm_Field:
                case TargetType.TargetType_50mm_SSD_20mm_Field:
                case TargetType.TargetType_50mm_SSD_30mm_Field:
                case TargetType.TargetType_50mm_SSD_40mm_Field:
                case TargetType.TargetType_50mm_SSD_50mm_Field:
                case TargetType.TargetType_QC_Collimator:
                    return Mappings.TargetType_CircularCell;

                //case TargetType.TargetType_61_Fields:
                //    return Mappings.TargetType_61Head;

                default:
                    return null; //throw new ArgumentException($"Unsupported TargetType {nameof(targetType)}");
            }
        }

        public static int GetBackwardFieldNameMapping(IDictionary<int, TreatmentFieldName> indexToTargetTypeMapping, TreatmentFieldName treatmentFieldName)
        {
            return indexToTargetTypeMapping.First(kv => kv.Value == treatmentFieldName).Key;
        }

        public static int GetCentralCellIndex(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.TargetType_None:
                    return 0;

                case TargetType.TargetType_30mm_SSD_7_Fields:
                    return 4;

                case TargetType.TargetType_50mm_SSD_13_Fields:
                    return 7;

                case TargetType.TargetType_50mm_SSD_15mm_Field:
                case TargetType.TargetType_50mm_SSD_20mm_Field:
                case TargetType.TargetType_50mm_SSD_30mm_Field:
                case TargetType.TargetType_50mm_SSD_40mm_Field:
                case TargetType.TargetType_50mm_SSD_50mm_Field:
                case TargetType.TargetType_QC_Collimator:
                    return 1;

                default:
                    throw new ArgumentException($"Unsupported TargetType {nameof(targetType)}");
            }
        }
    }
}
