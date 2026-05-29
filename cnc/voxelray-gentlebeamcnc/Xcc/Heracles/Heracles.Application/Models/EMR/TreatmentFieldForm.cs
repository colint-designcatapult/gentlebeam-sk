using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;
using System;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.EMR
{
    public interface ITreatmentFieldForm : ITreatmentField
    {
        bool IsReadOnly { get; }
    }

    public class TreatmentFieldForm : BindableBase, ITreatmentFieldForm
    {
        private Energy _energy;
        private double _dwellTime = 0;
        private double _actualDose = 0;
        private int _displayValue;
        private double _calculatedDose;
        private IPlan plan = null!;
        private bool isReadOnly = true;
        private double _current;
        private readonly ITreatmentDoseCalculation _treatmentDoseCalculation = null;
        private readonly ICollimatorConfiguration _collimatorConfiguration = null;    
        
        #region Properties
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; }
        public long PlanId { get; set; }
        public Energy Energy 
        { 
            get => _energy;
            set
            {
                if (SetProperty(ref _energy, value))
                {
                    Current = CurrentCalculator.CalculateCurrent(_energy);
                }
            }
        }

        public double DwellTime
        {
            get => _dwellTime;
            set
            {
                if (SetProperty(ref _dwellTime, value))
                {
                    CalculatedDose = _treatmentDoseCalculation.CalculateDose(Name, _collimatorConfiguration, _dwellTime);
                }
            }
        }
        public virtual IPlan Plan { 
            get => plan;
            set
            {
                if (SetProperty(ref plan, value))
                {
                    IsReadOnly = value == null || value.Status.Equals(PlanStatus.APPROVED);
                }
            }
        }
        public bool IsReadOnly { get => isReadOnly; set => SetProperty(ref isReadOnly, value); }

        public TreatmentFieldName Name { get; set; }
        // TODO: now this is used in the VM binding only, not sure if we need it here
        public double ActualDose { get => _actualDose; set => SetProperty(ref _actualDose, value); }
        public int DisplayValue
        {
            get => _displayValue;
            set => SetProperty(ref _displayValue, value);
        }
        public double CalculatedDose { get => _calculatedDose; set => SetProperty(ref _calculatedDose, value); }
        public double Current { get => _current; set => SetProperty(ref _current, value); }

        public bool IsActive { get; set; }

        #endregion Properties

        public TreatmentFieldForm(
            ITreatmentDoseCalculation treatmentDoseCalculation,
            ICollimatorConfiguration collimatorConfiguration,
            ITreatmentField field = null, 
            int displayValue = 0)
        {
            _treatmentDoseCalculation = treatmentDoseCalculation;
            _collimatorConfiguration = collimatorConfiguration;
            DisplayValue = displayValue;
            field?.CopyProperties(this);
            Current = CurrentCalculator.CalculateCurrent(Energy);
        }
    }
}
