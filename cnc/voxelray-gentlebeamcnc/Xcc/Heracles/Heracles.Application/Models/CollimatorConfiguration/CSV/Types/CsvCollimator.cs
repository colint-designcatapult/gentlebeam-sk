using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Infra.Persistence.CSV.Types;

namespace Heracles.Application.Models.CollimatorConfiguration.CSV.Types
{
    /// <summary>
    /// Utility class for Applicator data serialization into/from custom CSV format
    /// </summary>
    public class CsvCollimator
    {
        public string Serial { get; set; } = "";
        public CsvCollimatorType Type { get; } = new(TargetType.TargetType_None);
        public double DoseRate { get; set; } = 0.0;
        public CsvEnergyType Energy { get; } = new(0);
        public CsvBool IsActive { get; } = new(false);

        public CsvCollimator()
        {
        }

        public CsvCollimator(ICollimator collimator)
        {
            Serial = collimator.Serial;
            DoseRate = collimator.Configuration.ReferencedDoseRate;
            Type.Value = collimator.Configuration.Type;
            Energy.Value = collimator.Configuration.Energy;
            IsActive.Value = collimator.IsActive;
        }
    }

    /// <summary>
    /// Utility class for Energy value serialization into/from custom CSV format
    /// </summary>
    public class CsvEnergyType : CsvMappedType<Energy>
    {
        private static IDictionary<Energy, string> map = Enum.GetValues<Energy>().ToDictionary(x => x, x => ((int)x).ToString());
        private static CsvValueMap<Energy> mapping = new(map);
        public CsvEnergyType(Energy energy = 0)
            : base(energy, mapping)
        {
        }
    }

    /// <summary>
    /// Utility class for TargetType value serialization into/from custom CSV format
    /// </summary>
    public class CsvCollimatorType : CsvMappedType<TargetType>
    {
        private static IDictionary<TargetType, string> map = new Dictionary<TargetType, string>()
        {
            {TargetType.TargetType_50mm_SSD_15mm_Field, "15mm_field"},
            {TargetType.TargetType_50mm_SSD_20mm_Field, "20mm_field"},
            {TargetType.TargetType_50mm_SSD_30mm_Field, "30mm_field"},
            {TargetType.TargetType_50mm_SSD_40mm_Field, "40mm_field"},
            {TargetType.TargetType_50mm_SSD_50mm_Field, "50mm_field"},
            {TargetType.TargetType_QC_Collimator, "qc_collimator"},
            {TargetType.TargetType_50mm_SSD_13_Fields, "13_cell_IMVB"},
            {TargetType.TargetType_30mm_SSD_7_Fields, "7_cell_IMVB"},
            {TargetType.TargetType_61_Fields, "61_cell" }
        };
        private static CsvValueMap<TargetType> mapping = new(map);

        public CsvCollimatorType(TargetType t = TargetType.TargetType_None)
            : base(t, mapping)
        {
        }
    }
}
