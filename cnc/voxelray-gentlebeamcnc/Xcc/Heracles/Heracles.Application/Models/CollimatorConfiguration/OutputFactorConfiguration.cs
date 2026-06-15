using Heracles.Core.Constants;
using Heracles.Core.Enums;

using System;
using System.Collections.Generic;
using System.Linq;

using Xcc.Application.Helpers;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public abstract class OutputFactorConfigurationBase : DirtyFlaggedBindableBase
    {
        private IList<IOutputFactorEntry> _outputFactors;
        public virtual IList<IOutputFactorEntry> OutputFactors
        {
            get => _outputFactors;
            set => SetPropertyWithDirtyFlag(ref _outputFactors, value);
        }

        public static OutputFactorConfigurationBase Create(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.TargetType_None:
                    return null;
                case TargetType.TargetType_61_Fields:
                    return new OutputFactorConfiguration61Cells();
                case TargetType.TargetType_50mm_SSD_13_Fields:
                    return new OutputFactorConfiguration13Cells();
                case TargetType.TargetType_30mm_SSD_7_Fields:
                    return new OutputFactorConfiguration7Cells();
                case TargetType.TargetType_50mm_SSD_15mm_Field:
                case TargetType.TargetType_50mm_SSD_20mm_Field:
                case TargetType.TargetType_50mm_SSD_30mm_Field:
                case TargetType.TargetType_50mm_SSD_40mm_Field:
                case TargetType.TargetType_50mm_SSD_50mm_Field:
                case TargetType.TargetType_QC_Collimator:
                    return new OutputFactorConfiguration1Cell();
                default:
                    throw new NotSupportedException($"Failed to create output factor configuration for the TargetType {targetType}.");
            }
        }

        public bool IsIncomplete()
        {
            bool isIncomplete = false;

            foreach (var entry in OutputFactors)
            {
                isIncomplete |= !entry.Factor.HasValue;
            }

            return isIncomplete;
        }

        public void Reset()
        {
            foreach (var entry in OutputFactors)
            {
                entry.Factor = null;
            }
        }

        public override void AcceptChanges()
        {
            base.AcceptChanges();

            foreach (var entry in OutputFactors)
            {
                entry.AcceptChanges();
            }
        }
    }

    public class OutputFactorConfiguration13Cells: OutputFactorConfigurationBase
    {
        public OutputFactorConfiguration13Cells()
        {
            List<IOutputFactorEntry> outputFactors = [];
            
            for (int i = 1; i <= 13; i++)
            {
                OutputFactorEntry entry = new()
                {
                    FieldName = Mappings.TargetType_13CellsCentralLarge[i],
                    DisplayName = i.ToString(),
                    IsModified = false,
                };

                outputFactors.Add(entry);
            }

            OutputFactors = new List<IOutputFactorEntry>(outputFactors);
            IsModified = false;
        }
    }

    public class OutputFactorConfiguration7Cells : OutputFactorConfigurationBase
    {
        public OutputFactorConfiguration7Cells()
        {
            List<IOutputFactorEntry> outputFactors = [];

            for (int i = 1; i <= 7; i++)
            {
                OutputFactorEntry entry = new()
                {
                    FieldName = Mappings.TargetType_30mmSsd7Fields[i],
                    DisplayName = i.ToString(),
                    IsModified = false,
                };

                outputFactors.Add(entry);
            }

            OutputFactors = new List<IOutputFactorEntry>(outputFactors);
            IsModified = false;
        }
    }

    public class OutputFactorConfiguration61Cells : OutputFactorConfigurationBase
    {
        public OutputFactorConfiguration61Cells()
        {
            List<IOutputFactorEntry> outputFactors = [];
            
            for (int i = 1; i <= 61; i++)
            {
                OutputFactorEntry entry = new()
                {
                    FieldName = Mappings.TargetType_61Head[i],
                    DisplayName = i.ToString(),
                    IsModified = false,
                };

                outputFactors.Add(entry);
            }

            _outputFactors = new List<IOutputFactorEntry>(outputFactors);
            IsModified = false;
        }

        private IList<IOutputFactorEntry> _outputFactors;
        public override IList<IOutputFactorEntry> OutputFactors
        {
            get => _outputFactors;
            set
            {
                if(SetPropertyWithDirtyFlag(ref _outputFactors, value))
                {
                    RaisePropertyChanged(nameof(OutputFactors1To30));
                    RaisePropertyChanged(nameof(OutputFactors31To61));
                }
            }
        }

        public IList<IOutputFactorEntry> OutputFactors1To30 => OutputFactors.ToList().GetRange(0, 30);

        public IList<IOutputFactorEntry> OutputFactors31To61 => OutputFactors.ToList().GetRange(30, 31);
    }

    public class OutputFactorConfiguration1Cell : OutputFactorConfigurationBase
    {
        public OutputFactorConfiguration1Cell()
        {
            _outputFactors =
            [
                new OutputFactorEntry
                {
                    FieldName = Mappings.TargetType_CircularCell[1], 
                    DisplayName = "1", 
                    IsModified = false,
                    Factor = 1.0
                }
            ];
            IsModified = false;
        }

        private IList<IOutputFactorEntry> _outputFactors;
        public override IList<IOutputFactorEntry> OutputFactors
        {
            get => _outputFactors;
            set => SetPropertyWithDirtyFlag(ref _outputFactors, value);
        }
    }
}
