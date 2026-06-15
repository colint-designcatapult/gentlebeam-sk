using Heracles.Core.Enums;

namespace Heracles.Core.Constants
{
    public static class Mappings
    {        
        /// <summary>
        /// According to https://empyreanmed.atlassian.net/browse/H10SG-99
        /// </summary>
        public static IDictionary<int, TreatmentFieldName> TargetType_13CellsCentralLarge = new Dictionary<int, TreatmentFieldName>()
        {
            {1, TreatmentFieldName.Plus2L1},
            {2, TreatmentFieldName.Plus2C},
            {3, TreatmentFieldName.Plus2R1},
            {4, TreatmentFieldName.Plus1L2},
            {5, TreatmentFieldName.Plus1R2},
            {6, TreatmentFieldName.Plus0L2},
            {7, TreatmentFieldName.PlusC},
            {8, TreatmentFieldName.Plus0R2},
            {9, TreatmentFieldName.Minus1L2},
            {10, TreatmentFieldName.Minus1R2},
            {11, TreatmentFieldName.Minus2L1},
            {12, TreatmentFieldName.Minus2C},
            {13, TreatmentFieldName.Minus2R1},
        };

        /// <summary>
        /// According to https://empyreanmed.atlassian.net/browse/H10SG-99
        /// </summary>
        public static IDictionary<int, TreatmentFieldName> TargetType_30mmSsd7Fields = new Dictionary<int, TreatmentFieldName>()
        {
            {1, TreatmentFieldName.Plus1L1},
            {2, TreatmentFieldName.Plus1R1},
            {3, TreatmentFieldName.Plus0L1},
            {4, TreatmentFieldName.PlusC},
            {5, TreatmentFieldName.Plus0R1},
            {6, TreatmentFieldName.Minus1L1},
            {7, TreatmentFieldName.Minus1R1},
        };
        
        public static IDictionary<int, TreatmentFieldName> TargetType_CircularCell = new Dictionary<int, TreatmentFieldName>()
        {
            {1, TreatmentFieldName.PlusC},
        };

        /// <summary>
        /// According to https://empyreanmed.atlassian.net/browse/H10SG-99, 1 to 1 name correspondence for the '61 Head' collimator
        /// </summary>
        public static IDictionary<int, TreatmentFieldName> TargetType_61Head =
            Enum.GetValues<TreatmentFieldName>().ToDictionary(x => (int)x, x => x);


        public static IDictionary<int, TreatmentFieldName> TargetType_None = new Dictionary<int, TreatmentFieldName>();
    }
}
