using Heracles.Core.Constants;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using Prism.Mvvm;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Empyrean.Common.Core.Domain.DataManagement.Common;

using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class TreatmentField : BindableBase, ITreatmentField
    {
        public long Id { get; set; } = BaseEntry.NewEntryId;
        public DateTime CreationDate { get; set; }
        public long PlanId { get; set; }

        private Energy _energy = Energy.Energy_100;
        public Energy Energy { get => _energy; set => SetProperty(ref _energy, value); }

        private double _dwellTime;
        public double DwellTime { get => _dwellTime; set => SetProperty(ref _dwellTime, value); }

        public IPlan Plan { get; set; } = null!;

        private double _calculatedDose;
        public double CalculatedDose { get => _calculatedDose; set => SetProperty(ref _calculatedDose, value); }


        private double _current;
        public double Current { get => _current; set => SetProperty(ref _current, value); }

        private bool _isActive;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        private int _displayValue = 0;
        public int DisplayValue { get => _displayValue; set => SetProperty(ref _displayValue, value); }

        public TreatmentFieldName Name { get; set; }

        public TreatmentField(ITreatmentField selectedTreatmentField)
        {
            selectedTreatmentField.CopyProperties(this);
        }
        public TreatmentField()
        {
        }

        public static ObservableCollection<ITreatmentField> GetTreatmentFieldCollection(TargetType? targetType)
        {
            switch (targetType)
            {
                case TargetType.TargetType_None:
                    return TreatmentFieldCollectionNone;
                case TargetType.TargetType_QC_Collimator:
                    return TreatmentFieldCollectionNone;
                case TargetType.TargetType_61_Fields:
                    return TreatmentFieldCollection61Cells;
                case TargetType.TargetType_30mm_SSD_7_Fields:
                    return TreatmentFieldCollection7Cells;
                case TargetType.TargetType_50mm_SSD_13_Fields:
                    return TreatmentFieldCollection13Cells;
                case TargetType.TargetType_50mm_SSD_15mm_Field:
                    return TreatmentFieldCollectionCircular15;
                case TargetType.TargetType_50mm_SSD_20mm_Field:
                    return TreatmentFieldCollectionCircular20;
                case TargetType.TargetType_50mm_SSD_30mm_Field:
                    return TreatmentFieldCollectionCircular30;
                case TargetType.TargetType_50mm_SSD_40mm_Field:
                    return TreatmentFieldCollectionCircular40;
                case TargetType.TargetType_50mm_SSD_50mm_Field:
                    return TreatmentFieldCollectionCircular50;
                default:
                    throw new NotSupportedException($"Applicator {targetType} is not supported.");
            }
        }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollection61Cells { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollection13Cells { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollection7Cells { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollectionCircular15 { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollectionCircular20 { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollectionCircular30 { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollectionCircular40 { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollectionCircular50 { get; }

        public static ObservableCollection<ITreatmentField> TreatmentFieldCollectionNone { get; }

        static TreatmentField()
        {
            TreatmentFieldCollection13Cells = [];
            foreach (var fieldName in Mappings.TargetType_13CellsCentralLarge.Values)
            {
                TreatmentFieldCollection13Cells.Add(new TreatmentField { Name = fieldName });
            }

            TreatmentFieldCollection61Cells = [];
            foreach (var fieldName in Mappings.TargetType_61Head.Values)
            {
                TreatmentFieldCollection61Cells.Add(new TreatmentField { Name = fieldName });
            }

            TreatmentFieldCollection7Cells = [];
            foreach (var fieldName in Mappings.TargetType_30mmSsd7Fields.Values)
            {
                TreatmentFieldCollection7Cells.Add(new TreatmentField { Name = fieldName });
            }

            TreatmentFieldCollectionCircular15 = [.. Mappings.TargetType_CircularCell.Select(x => new TreatmentField { Name = x.Value })];
            TreatmentFieldCollectionCircular20 = [.. Mappings.TargetType_CircularCell.Select(x => new TreatmentField { Name = x.Value })];
            TreatmentFieldCollectionCircular30 = [.. Mappings.TargetType_CircularCell.Select(x => new TreatmentField { Name = x.Value })];
            TreatmentFieldCollectionCircular40 = [.. Mappings.TargetType_CircularCell.Select(x => new TreatmentField { Name = x.Value })];
            TreatmentFieldCollectionCircular50 = [.. Mappings.TargetType_CircularCell.Select(x => new TreatmentField { Name = x.Value })];

            TreatmentFieldCollectionNone = [];
        }
    }
}
