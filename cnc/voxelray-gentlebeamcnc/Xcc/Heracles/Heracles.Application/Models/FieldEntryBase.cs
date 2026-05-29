using Heracles.Core.Enums;
using Heracles.Core.Models;
using Prism.Mvvm;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models
{
    public class FieldEntryBase : BindableBase, IFieldEntryBase
    {
        private long _id = BaseEntry.NEW_ENTRY_ID;
        public long Id { get => _id; set => SetProperty(ref _id, value); }

        private TreatmentFieldName _fieldName;
        public TreatmentFieldName Name { get => _fieldName; set => SetProperty(ref _fieldName, value); }

        private Energy _energy;
        public Energy Energy { get => _energy; set => SetProperty(ref _energy, value); }

        private float _current = 0.0f;
        public float Current { get => _current; set => SetProperty(ref _current, value); }

        private double _duration = 0.0;
        public double Duration { get => _duration; set => SetProperty(ref _duration, value); }
        /// <summary>
        /// just to match a xaml template
        /// </summary>
        public double DwellTime { get => _duration; set => SetProperty(ref _duration, value); }

        private float _actual = 0.0f;
        public float Actual { get => _actual; set => SetProperty(ref _actual, value); }

        private int _displayValue;
        public int DisplayValue
        {
            get { return _displayValue; }
            set { SetProperty(ref _displayValue, value); }
        }
    }
}
