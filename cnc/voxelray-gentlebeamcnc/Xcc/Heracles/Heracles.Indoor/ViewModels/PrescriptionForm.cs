using Heracles.Application.Common;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xcc.Application.Common;
using Xcc.Application.Domain.System;
using Xcc.Application.Forms;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels
{    
    public class DurationCalculator(IDictionary<Energy, OutputFactorInfo> doseFactors)
    {
        public double CalcDuration(Energy energy, double dose)
        {
            if (doseFactors.TryGetValue(energy, out OutputFactorInfo info))
            {
                return info.DurationUpTo10th(dose);
            }
            else 
                throw new Exception(StringConstants.EMR.CannotFindApplicatorConfigErrorMessage);
        }

        public double CalcDose(Energy energy, double duration)
        {
            if (doseFactors.TryGetValue(energy, out OutputFactorInfo info))
            {
                return info.Dose(duration);
            }
            else
                throw new Exception(StringConstants.EMR.CannotFindApplicatorConfigErrorMessage);
        }
    }

    public class PrescriptionForm(
        IPopUpService popUpService,
        Prescription prescription,
        DurationCalculator durationCalculator)
        : Form
    {
        public Prescription GetValue()
        {
            return new Prescription(prescription)
            {
                FxsPerWeek = _fractionsPerWeek.ValidValue,
                Tdf = _tdf.ValidValue,
                DailyDose = _dailyDose.ValidValue,
                NumberOfFxs = _numberOfFxs.ValidValue,
                Energy = _energy.ValidValue,
                MinTdf = _minTdf.ValidValue,
                DwellTime = _duration.ValidValue,
                TotalDose = TotalDose ?? throw new NullReferenceException(nameof(TotalDose))
            };
        }

        private FormField<int> _fractionsPerWeek = new(prescription.FxsPerWeek);
        private EnumFormField<TDF> _tdf = new(prescription.Tdf);
        private FormField<double> _dailyDose = new(prescription.IsBlank ? null : prescription.DailyDose);
        private FormField<int> _numberOfFxs = new(prescription.IsBlank ? null : prescription.NumberOfFxs);
        private EnumFormField<Energy> _energy = new(prescription.IsBlank ? null : prescription.Energy);

        private EnumFormField<TDF> _minTdf = new(TDF.Tdf_94);
        private FormField<double> _duration = new(prescription.IsBlank ? null : prescription.DwellTime);
        private double? _totalDose = prescription.TotalDose;
        private double? _actualDose = InitActualDose(durationCalculator, prescription);
        private bool _canCalcDuration = !prescription.IsBlank;

        private static double? InitActualDose(DurationCalculator calculator, Prescription prescription)
        {
            return Enum.IsDefined(prescription.Energy)
                ? calculator.CalcDose(prescription.Energy, prescription.DwellTime)
                : null;
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.FractionsPerWeekRequired)]
        [Int]
        [DeniedValues<int>(0)]
        public int? FxsPerWeek
        {
            get => GetFieldTypedValue<int>(_fractionsPerWeek);
            set => SetFieldValue(value, _fractionsPerWeek);
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.TdfRequired)]
        public TDF? Tdf
        {
            get => GetFieldTypedValue<TDF>(_tdf);
            set => SetFieldValue(value, _tdf);
        }


        [Required(ErrorMessage = StringConstants.EMR.Validation.DailyDoseRequired)]
        [Double]
        [NumericRange(0d, 1500d)]
        [DeniedValues<double>(0d, ErrorMessage = StringConstants.EMR.Validation.DailyDoseMustBeNonZero)]
        public object? DailyDose
        {
            get => GetFieldValue(_dailyDose);
            set
            {
                if (SetFieldValue(value, _dailyDose))
                {
                    CalcTotalDose();
                    CalcDurationAndActualDose();
                }
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.NumberOfFractionsRequired)]
        [Int]
        [NumericRange(0, int.MaxValue)]
        [DeniedValues<int>(0, ErrorMessage = StringConstants.EMR.Validation.NumberOfFractionsMustBeNonZero)]
        public object? NumberOfFxs
        {
            get => GetFieldValue(_numberOfFxs);
            set
            {
                if (SetFieldValue(value, _numberOfFxs))
                    CalcTotalDose();
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.EnergyRequired)]
        [DeniedValues<Energy>(0, ErrorMessage = StringConstants.EMR.Validation.EnergyRequired)]
        public Energy? Energy
        {
            get => GetFieldTypedValue<Energy>(_energy);
            set
            {
                if (SetFieldValue(value, _energy))
                {
                    CalcDurationAndActualDose();
                }
            }
        }

        public TDF MinTdf
        {
            get => GetFieldTypedValueStrict<TDF>(_minTdf);
            set => SetFieldValue(value, _minTdf);
        }

        [Double]
        [NumericRange(1, double.MaxValue)]
        [DeniedValues<double>(0d, ErrorMessage = StringConstants.EMR.Validation.DurationMustBeGreaterZero)]
        public object? Duration
        {
            get => GetFieldValue(_duration);
            set
            {
                if ((CanCalcDuration || value is null) && SetFieldValue(value, _duration))
                {
                    CalcDurationAndActualDose(calcDuration: false);
                }
            }
        }

        public Status Status => prescription.Status;

        public bool IsBlank => prescription.IsBlank;

        public double? TotalDose { get => _totalDose; set => SetProperty(ref _totalDose, value); }
        public double? ActualDose { get => _actualDose; set => SetProperty(ref _actualDose, value); }
        public bool AreEnergyAndDoseSet => Energy is not null && DailyDose is not null;

        public DurationCalculator DurationCalculator => durationCalculator;

        private void CalcTotalDose()
        {
            if (_dailyDose.IsValid && _numberOfFxs.IsValid)
            {
                TotalDose = _dailyDose.ValidValue * _numberOfFxs.ValidValue;
            }
            else
            {
                TotalDose = null;
            }
        }

        private void CalcDurationAndActualDose(bool calcDuration = true)
        {
            if (CanCalcDuration = _dailyDose.IsValid && _energy.IsValid)
            {
                try
                {
                    if (calcDuration)
                    {
                        Duration = durationCalculator.CalcDuration(_energy.ValidValue, _dailyDose.ValidValue);
                    }
                    ActualDose = _duration.IsValid
                        ? durationCalculator.CalcDose(_energy.ValidValue, _duration.ValidValue)
                        : null;
                }
                catch (Exception ex)
                {
                    Duration = null;
                    ActualDose = null;
                    popUpService.LogAndShowError(StringConstants.EMR.PrescriptionError, ex.Message);
                }
            }
            else
            {
                Duration = null;
                ActualDose = null;
            }
        }

        public bool CanCalcDuration { 
            get => _canCalcDuration; 
            set => SetProperty(ref _canCalcDuration, value); 
        }


        /// <summary>
        /// according to GentleCure Protocol Final April 2025
        /// </summary>
        /// <param name="simulation"></param>
        /// <param name="pathology"></param>
        /// <returns></returns>
        public static PrescriptionForm GetDefaultPrescriptionState(
            IPopUpService popUpService,
            ISimulation simulation,
            Pathology pathology,
            DurationCalculator calculator)
        {
            TDF minTdf = TDF.Tdf_94;
            Energy? energy = null;
            int fractionCount = 20;
            double? dailyDose = null;
            int fxPerWeek = 4;

            if (pathology == Pathology.Bcc ||
                pathology == Pathology.Scc)
            {
                dailyDose = 285.0;

                if (simulation.LesionDepth is >= 0.0 and < 0.55)
                {
                    minTdf = TDF.Tdf_95;
                    energy = Core.Enums.Energy.Energy_50;
                }
                else if (simulation.LesionDepth is >= 0.55 and < 1.05)
                {
                    minTdf = TDF.Tdf_95;
                    energy = Core.Enums.Energy.Energy_70;
                }
                else if (simulation.LesionDepth is >= 1.05 and < 1.55)
                {
                    minTdf = TDF.Tdf_96;
                    energy = Core.Enums.Energy.Energy_70;
                }
                else if (simulation.LesionDepth is >= 1.55 and < 2.05)
                {
                    minTdf = TDF.Tdf_97;
                    energy = Core.Enums.Energy.Energy_100;
                }
                else if (simulation.LesionDepth is >= 2.05 and < 2.55)
                {
                    minTdf = TDF.Tdf_98;
                    energy = Core.Enums.Energy.Energy_100;
                }
                else if (simulation.LesionDepth is >= 2.55 and < 3.0)
                {
                    minTdf = TDF.Tdf_98;
                    energy = Core.Enums.Energy.Energy_100;
                }
                // todo: what should be set for the other cases?
            }
            else if (pathology == Pathology.SccIs)
            {
                dailyDose = 285.0;

                if (simulation.LesionDepth < 0.8)
                {
                    energy = Core.Enums.Energy.Energy_50;
                }
                else if (simulation.LesionDepth is >= 0.8 and < 1.55)
                {
                    energy = Core.Enums.Energy.Energy_70;
                }
                else if (simulation.LesionDepth is >= 1.55)
                {
                    energy = Core.Enums.Energy.Energy_100;
                }
            }
            else if (pathology == Pathology.Keloid)
            {
                energy = Core.Enums.Energy.Energy_100;
                fractionCount = 3;
                dailyDose = 600.0;
            }

            var blankPrescription = new Prescription
            {
                SimulationId = simulation.Id,
                NumberOfFxs = fractionCount,
                MinTdf = minTdf,
                Tdf = minTdf,
                FxsPerWeek = fxPerWeek,
            };
            if (energy.HasValue)
            {
                blankPrescription.Energy = energy.Value;
            }

            return new PrescriptionForm(popUpService, blankPrescription, calculator);
        }
    }
}
