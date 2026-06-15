using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.Common;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Xcc.Application.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Application.Models.EMR
{
    [Obsolete] // Use External's OutputFactorInfo + ICollimatorCalibrationInfo instead
    public interface ITreatmentDoseCalculation
    {
        event EventHandler OutputFactorsChanged;

        Task FetchTreatmentFactorsAsync(ICollimatorConfiguration collimatorConfiguration, TargetType collimatorType, Energy energy);
        double CalculateDose(TreatmentFieldName treatmentField, ICollimatorConfiguration collimatorConfiguration, double dwellTime);
        double CalculateDuration(TreatmentFieldName treatmentField, ICollimatorConfiguration collimator, double dailyDose);
    }

    public class TreatmentDoseCalculation : ITreatmentDoseCalculation
    {
        public TreatmentDoseCalculation(
            IOutputFactorCommands outputFactorCommands,
            ILogWriter logWriter)
        {
            OutputFactorCommands = outputFactorCommands;
            LogWriter = logWriter;
        }

        public IOutputFactorCommands OutputFactorCommands { get; }
        public ILogWriter LogWriter { get; }

        public event EventHandler OutputFactorsChanged;

        [Obsolete] //Use External's OutputFactorInfo instead
        public double CalculateDose(TreatmentFieldName fieldName, ICollimatorConfiguration collimatorConfiguration, double dwellTime)
        {
            var fieldOutputFactor = GetTreatmentFieldFactor(fieldName);

            return (fieldOutputFactor * collimatorConfiguration.ReferencedDoseRate) * dwellTime / 60.0;
        }

        [Obsolete] //Use External's OutputFactorInfo instead
        public double CalculateDuration(TreatmentFieldName fieldName, ICollimatorConfiguration collimatorConfiguration, double dailyDose)
        {
            var outputFactor = GetTreatmentFieldFactor(fieldName);

            // PrescribedDose / (FieldOutputFactor * DoseRate) - rounded up, according to H10SG-20
            return Math.Ceiling(dailyDose / (outputFactor * collimatorConfiguration.ReferencedDoseRate) * 60.0);
        }

        public async Task FetchTreatmentFactorsAsync(ICollimatorConfiguration collimatorConfiguration, TargetType collimatorType, Energy energy)
        {
            try
            {
                _outputFactors.Clear();

                if (collimatorConfiguration == null)
                {
                    throw new NullReferenceException(StringConstants.EMR.PlanTreatmentFactorsMissingActiveApplicatorError);
                }

                if (collimatorConfiguration.DefaultPreset == null)
                {
                    throw new NullReferenceException(StringConstants.EMR.PlanTreatmentFactorsMissingActiveApplicatorPresetError);
                }

                // Select proper output factors (if any)
                var allOutputFactors = await OutputFactorCommands.ReadListAsync(collimatorConfiguration.DefaultPreset.Id);
                
                // Verify that there are all necessary factors:
                var fetchedFactorsLookupSource = allOutputFactors.ToLookup(p => p.FieldName);
                var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(collimatorType);

                var missingFactors = fieldNameMapping.Values.Where(name => !fetchedFactorsLookupSource.Contains(name)).ToList();
                if (missingFactors.Count > 0)
                {
                    throw new Exception($"{missingFactors.Count} lookup factors are missing in the applicator preset");
                }

                _outputFactors = allOutputFactors.ToList();
            }
            catch (Exception ex)
            {
                string energyTypeStr = energy.GetAttribute<DisplayAttribute>().Name;
                string collimatorTypeStr = collimatorType.GetAttribute<DisplayAttribute>().Name;

                string msg = $"{StringConstants.EMR.PlanTreatmentFactorsMissingDataError}.{Environment.NewLine}" +
                             $"Applicator size=\"{collimatorTypeStr}\", Energy={energyTypeStr}kV.";

                await LogWriter.LogAsync($"{msg}{Environment.NewLine}{ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
                throw new Exception(msg, ex);
            }
            finally
            {
                OutputFactorsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private double GetTreatmentFieldFactor(TreatmentFieldName fieldName)
        {
            // TODO: we don't use output factors for 1-point applicators, as they're always equal to 1
            //var outputFactor = _outputFactors?.FirstOrDefault(f => f.FieldName == fieldName);
            //if (outputFactor?.Factor == null)
            //{
            //    throw new ArgumentException(StringConstants.EMR.FailedToDetermineOutputFactorMessage);
            //}

            //return outputFactor.Factor.Value;
            return 1.0;
        }

        private ICollection<IOutputFactor> _outputFactors = new List<IOutputFactor>();
    }
}
