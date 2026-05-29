using Heracles.Application.Common;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Common;

namespace Heracles.Application.Models
{
    public class SimulationState : DirtyFlaggedBindableBase, ISimulationState
    {
        public SimulationState(ISimulation initialState = null)
        {
            PatientPositions = [];
            TreatmentDevices = [];

            initialState.CopyProperties(this);

            IsModified = false;
        }

        public long Id { get; set; } = BaseEntry.NewEntryId;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public long DiagnosisId { get => _diagnosisId; set => SetPropertyWithDirtyFlag(ref _diagnosisId, value); }

        public long VisitId { get; set; }

        public string PerformedBy { get; set; } = string.Empty;

        [Required(ErrorMessage = StringConstants.EMR.Validation.ApplicatorSizeRequired)]
        public TargetType TargetType
        {
            get => _targetType;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _targetType, value))
                {
                    Validate(value);
                }
            }
        }

        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.LesionDepthMustBeNonZero)]
        public double? LesionDepth
        {
            get => _lesionDepth;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _lesionDepth, value))
                {
                    Validate(value);
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.LesionSizeLRequired)]
        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.LesionSizeLMustBeNonZero)]
        public double? LesionSizeL
        {
            get => _lesionSizeL; 
            set
            {
                if (SetPropertyWithDirtyFlag(ref _lesionSizeL, value))
                {
                    Validate(value);

                    ShieldSizeL = MarginSizeL * 2 + LesionSizeL;
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.LesionSizeWRequired)]
        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.LesionSizeWMustBeNonZero)]
        public double? LesionSizeW { 
            get => _lesionSizeW;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _lesionSizeW, value))
                {
                    Validate(value);

                    ShieldSizeW = MarginSizeW * 2 + LesionSizeW;
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.MarginSizeRequired)]
        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.MarginSizeMustBeNonZero)]
        public double? MarginSizeL { 
            get => _marginSizeL;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _marginSizeL, value))
                {
                    Validate(value);

                    ShieldSizeL = MarginSizeL * 2 + LesionSizeL;
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.MarginSizeRequired)]
        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.MarginSizeMustBeNonZero)]
        public double? MarginSizeW
        {
            get => _marginSizeW;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _marginSizeW, value))
                {
                    Validate(value);
                    
                    ShieldSizeW = MarginSizeW * 2 + LesionSizeW;
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.ShieldSizeLengthRequired)]
        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.ShieldSizeLengthMustBeNonZero)]
        public double? ShieldSizeL 
        { 
            get => _shieldSizeL;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _shieldSizeL, value))
                {
                    Validate(value);
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.ShieldSizeWidthRequired)]
        [NumericRange(0d, double.MaxValue)]
        [DeniedValues(0d, ErrorMessage = StringConstants.EMR.Validation.ShieldSizeWidthMustBeNonZero)]
        public double? ShieldSizeW
        {
            get => _shieldSizeW;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _shieldSizeW, value))
                {
                    Validate(value);
                }
            }
        }

        public double? ApplicatorSize { get => _applicatorSize; set => SetPropertyWithDirtyFlag(ref _applicatorSize, value); }

        private string _setupNote;
        public string SetupNote 
        { 
            get => _setupNote;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _setupNote, value))
                {
                    Validate(value);
                }
            }
        }

        private ObservableCollection<DeviceType> _treatmentDevices;
        [Required(ErrorMessage = StringConstants.EMR.Validation.TreatmentDevicesRequired)]
        [Length(1, int.MaxValue, ErrorMessage = StringConstants.EMR.Validation.TreatmentDevicesMustNotBeEmpty)]
        public ObservableCollection<DeviceType> TreatmentDevices 
        {
            get => _treatmentDevices;
            set
            {
                if (_treatmentDevices != null &&
                    _treatmentDevices.Equals(value) == false)
                {
                    _treatmentDevices.CollectionChanged -= TreatmentDevicesOnCollectionChanged;
                }

                if (SetPropertyWithDirtyFlag(ref _treatmentDevices, value))
                {
                    if (value is null)
                        return;

                    _treatmentDevices.CollectionChanged += TreatmentDevicesOnCollectionChanged;
                }
            }
        }

        private ObservableCollection<PatientPosition> _patientPositions;
        [Required(ErrorMessage = StringConstants.EMR.Validation.PatientPositionsRequired)]
        [Length(1, int.MaxValue, ErrorMessage = StringConstants.EMR.Validation.PatientPositionsMustNotBeEmpty)]
        public ObservableCollection<PatientPosition> PatientPositions
        {
            get => _patientPositions;
            set
            {
                if (_patientPositions != null &&
                    _patientPositions.Equals(value) == false)
                {
                    _patientPositions.CollectionChanged -= PatientPositionsOnCollectionChanged;
                }

                if (SetPropertyWithDirtyFlag(ref _patientPositions, value))
                {
                    if (value is null)
                        return;

                    _patientPositions.CollectionChanged += PatientPositionsOnCollectionChanged;
                }
            }
        }

        public SimulationStatus Status 
        { 
            get => _status; 
            set => SetPropertyWithDirtyFlag(ref _status, value); 
        }
        
        private void TreatmentDevicesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsModified = true;
            Validate(TreatmentDevices, nameof(TreatmentDevices));
        }

        private void PatientPositionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsModified = true;
            Validate(PatientPositions, nameof(PatientPositions));
        }

        private double? _lesionDepth = null;
        private double? _lesionSizeL = null;
        private double? _lesionSizeW = null;
        private double? _marginSizeL = null;
        private double? _marginSizeW = null;
        private double? _shieldSizeL = null;
        private double? _shieldSizeW = null;
        private double? _applicatorSize = null;
        private SimulationStatus _status;
        private TargetType _targetType;
        private long _diagnosisId;
    }
}
