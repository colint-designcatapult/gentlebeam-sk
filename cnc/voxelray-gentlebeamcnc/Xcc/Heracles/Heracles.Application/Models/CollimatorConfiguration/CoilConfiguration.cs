using Heracles.Core.Constants;
using Heracles.Core.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Application.Helpers;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public abstract class CoilConfigurationBase : DirtyFlaggedBindableBase
    {
        public IList<CoilConfigurationForm> GetConfiguration() => CoilConfiguration;

        public abstract IList<CoilConfigurationForm> CoilConfiguration { get; set; }

        public static CoilConfigurationBase CreateCoilConfiguration(TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.TargetType_None:
                    return null;
                case TargetType.TargetType_61_Fields:
                    return new CoilConfiguration61Cell();
                case TargetType.TargetType_50mm_SSD_13_Fields:
                    return new CoilConfiguration13Cell();
                case TargetType.TargetType_30mm_SSD_7_Fields:
                    return new CoilConfiguration7Cell();
                case TargetType.TargetType_50mm_SSD_15mm_Field:
                case TargetType.TargetType_50mm_SSD_20mm_Field:
                case TargetType.TargetType_50mm_SSD_30mm_Field:
                case TargetType.TargetType_50mm_SSD_40mm_Field:
                case TargetType.TargetType_50mm_SSD_50mm_Field:
                case TargetType.TargetType_QC_Collimator:
                    return new CoilConfiguration1Cell();
                default:
                    throw new NotSupportedException($"Failed to create coil configuration for the TargetType {targetType}.");
            }
        }
    }


    public class CoilConfiguration61Cell : CoilConfigurationBase
    {
        public CoilConfiguration61Cell() 
        {
            List<CoilConfigurationForm> coilConfiguration = [];

            for (int i = 1; i <= 61; i++)
            {
                CoilConfigurationForm config = new()
                {
                    FieldName = Mappings.TargetType_61Head[i],
                    DisplayName = i.ToString(),
                    IsModified = false,
                };

                coilConfiguration.Add(config);
                config.IsModifiedChanged += CoilConfigurationEntryIsModifiedChanged;
            }

            CoilConfiguration = coilConfiguration;
            IsModified = false;
        }

        private IList<CoilConfigurationForm> _coilConfiguration;
        public override IList<CoilConfigurationForm> CoilConfiguration
        {
            get => _coilConfiguration;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _coilConfiguration, value))
                {
                    RaisePropertyChanged(nameof(CoilConfiguration1To30));
                    RaisePropertyChanged(nameof(CoilConfiguration31To61));
                }
            }
        }

        public IList<CoilConfigurationForm> CoilConfiguration1To30 => CoilConfiguration.ToList().GetRange(0, 30);

        public IList<CoilConfigurationForm> CoilConfiguration31To61 => CoilConfiguration.ToList().GetRange(30, 31);

        private void CoilConfigurationEntryIsModifiedChanged(object sender, bool isModified)
        {
            IsModified |= isModified;
        }
    };


    public class CoilConfiguration13Cell : CoilConfigurationBase
    {
        public CoilConfiguration13Cell()
        {
            List<CoilConfigurationForm> coilConfiguration = [];

            for (int i = 1; i <= 13; i++)
            {
                CoilConfigurationForm config = new()
                {
                    FieldName = Mappings.TargetType_13CellsCentralLarge[i],
                    DisplayName = i.ToString(),
                    IsModified = false,
                };

                coilConfiguration.Add(config);
                config.IsModifiedChanged += CoilConfigurationEntryIsModifiedChanged;
            }

            CoilConfiguration = coilConfiguration;
            IsModified = false;
        }

        private IList<CoilConfigurationForm> _coilConfiguration;
        public override IList<CoilConfigurationForm> CoilConfiguration
        {
            get => _coilConfiguration;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _coilConfiguration, value))
                {
                    RaisePropertyChanged(nameof(CoilConfiguration1To3));
                    RaisePropertyChanged(nameof(CoilConfiguration4To6));
                    RaisePropertyChanged(nameof(CoilConfiguration7));
                    RaisePropertyChanged(nameof(CoilConfiguration8To10));
                    RaisePropertyChanged(nameof(CoilConfiguration11To13));
                }
            }
        }

        public IList<CoilConfigurationForm> CoilConfiguration1To3 => CoilConfiguration.ToList().GetRange(0, 3);

        public IList<CoilConfigurationForm> CoilConfiguration4To6 => CoilConfiguration.ToList().GetRange(3, 3);

        public IList<CoilConfigurationForm> CoilConfiguration7 => CoilConfiguration.ToList().GetRange(6, 1);

        public IList<CoilConfigurationForm> CoilConfiguration8To10 => CoilConfiguration.ToList().GetRange(7, 3);

        public IList<CoilConfigurationForm> CoilConfiguration11To13 => CoilConfiguration.ToList().GetRange(10, 3);

        private void CoilConfigurationEntryIsModifiedChanged(object sender, bool isModified)
        {
            IsModified |= isModified;
        }
    };


    public class CoilConfiguration7Cell : CoilConfigurationBase
    {
        public CoilConfiguration7Cell()
        {
            List<CoilConfigurationForm> coilConfiguration = [];

            for (int i = 1; i <= 7; i++)
            {
                CoilConfigurationForm config = new()
                {
                    FieldName = Mappings.TargetType_30mmSsd7Fields[i],
                    DisplayName = i.ToString(),
                    IsModified = false
                };

                coilConfiguration.Add(config);
                config.IsModifiedChanged += CoilConfigurationEntryIsModifiedChanged;
            }

            CoilConfiguration = coilConfiguration;
            IsModified = false;
        }

        private IList<CoilConfigurationForm> _coilConfiguration;
        public override IList<CoilConfigurationForm> CoilConfiguration
        {
            get => _coilConfiguration;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _coilConfiguration, value))
                {
                    RaisePropertyChanged(nameof(CoilConfiguration1To3));
                    RaisePropertyChanged(nameof(CoilConfiguration4));
                    RaisePropertyChanged(nameof(CoilConfiguration5To7));
                }
            }
        }

        public IList<CoilConfigurationForm> CoilConfiguration1To3 => CoilConfiguration.ToList().GetRange(0, 3);

        public IList<CoilConfigurationForm> CoilConfiguration4 => CoilConfiguration.ToList().GetRange(3, 1);

        public IList<CoilConfigurationForm> CoilConfiguration5To7 => CoilConfiguration.ToList().GetRange(4, 3);

        private void CoilConfigurationEntryIsModifiedChanged(object sender, bool isModified)
        {
            IsModified |= isModified;
        }
    };


    public class CoilConfiguration1Cell : CoilConfigurationBase
    {
        public CoilConfiguration1Cell()
        {
            var config = new CoilConfigurationForm()
            {
                FieldName = Mappings.TargetType_CircularCell[1],
                DisplayName = "1",
                IsModified = false,
            };

            config.IsModifiedChanged += CoilConfigurationEntryIsModifiedChanged;

            List<CoilConfigurationForm> coilConfiguration = [config];
            CoilConfiguration = coilConfiguration;
            IsModified = false;
        }

        private IList<CoilConfigurationForm> _coilConfiguration;
        public override IList<CoilConfigurationForm> CoilConfiguration
        {
            get => _coilConfiguration;
            set => SetPropertyWithDirtyFlag(ref _coilConfiguration, value);
        }

        private void CoilConfigurationEntryIsModifiedChanged(object sender, bool isModified)
        {
            IsModified |= isModified;
        }
    };
}
