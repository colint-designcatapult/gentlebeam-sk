using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.External.Models.CollimatorConfiguration
{
    public struct CoilConfigurationInfo
    {
        public double XDeflectionCurrent;
        public double YDeflectionCurrent;
        public double FocusCurrent;
        public CoilConfigurationInfo(ICoilConfigurationEntry coilConfiguration)
        {
            // exception will be thrown in case of invalid coil configuration
            XDeflectionCurrent = coilConfiguration.XDeflectionCurrent;
            YDeflectionCurrent = coilConfiguration.YDeflectionCurrent;
            FocusCurrent = coilConfiguration.FocusCurrent;
        }
    }

    public interface ICollimatorCalibrationInfo
    {
        ICollimatorConfiguration CollimatorConfiguration { get; }
        double HeaterCurrent { get; }
        CoilConfigurationInfo? GetCoilConfiguration(
            TreatmentFieldName fieldName);
        CoilConfigurationInfo GetCorrectedCoilConfiguration(
            TreatmentFieldName fieldName, 
            MagnetometerValues valuesFront, 
            MagnetometerValues valuesBack);
        OutputFactorInfo? GetOutputFactor(TreatmentFieldName fieldName);
    }

    public class CollimatorCalibrationInfo : ICollimatorCalibrationInfo
    {
        private MagnetometerConfiguration _magnetometer;
        private readonly IDictionary<TreatmentFieldName, CoilConfigurationInfo> _coilConfigurations;
        private readonly IDictionary<TreatmentFieldName, OutputFactorInfo> _outputFactors;

        public double HeaterCurrent { get; }

        public ICollimatorConfiguration CollimatorConfiguration { get; }

        public CoilConfigurationInfo? GetCoilConfiguration(TreatmentFieldName fieldName)
        {
            if (_coilConfigurations.TryGetValue(fieldName, out var configuration) == false)
            {
                //throw new ArgumentException($"CollimatorConfigurationInfo error: {fieldName} field configuration is missing");
                return null;
            }
            return configuration;
        }
        public OutputFactorInfo? GetOutputFactor(TreatmentFieldName fieldName)
        {
            if (_outputFactors.TryGetValue(fieldName, out var factor) == false)
            {
                //throw new ArgumentException($"CollimatorConfigurationInfo error: {fieldName} field output factor is missing");
                return null;
            }
            return factor;
        }

        public CoilConfigurationInfo GetCorrectedCoilConfiguration(
            TreatmentFieldName fieldName,
            MagnetometerValues valuesFront,
            MagnetometerValues valuesBack)
        {
            CoilConfigurationInfo info = GetCoilConfiguration(fieldName).Value;
            // TODO: move and apply correction here

            return info;
        }

        public CollimatorCalibrationInfo(
            ICollimatorConfiguration collimatorConfiguration,
            IEnumerable<ICoilConfigurationEntry> coilConfigurations,
            IEnumerable<IOutputFactor> outputFactors,
            MagnetometerConfiguration magnetometer,
            double heaterCurrent,
            double referencedDoseRate)
        {
            CollimatorConfiguration = collimatorConfiguration;
            _magnetometer = magnetometer;
            _coilConfigurations = coilConfigurations.ToDictionary(v => v.FieldName, v => new CoilConfigurationInfo(v));
            _outputFactors = outputFactors.ToDictionary(
                v => v.FieldName, 
                v => new OutputFactorInfo(v.Factor.Value, referencedDoseRate));
            HeaterCurrent = heaterCurrent;
        }
    }
}
