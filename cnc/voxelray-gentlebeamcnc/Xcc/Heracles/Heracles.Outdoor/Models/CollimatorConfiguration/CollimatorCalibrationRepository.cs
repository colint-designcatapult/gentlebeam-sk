using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Models;
using Xcc.Application.Domain.System;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;

namespace Heracles.External.Models.CollimatorConfiguration
{
    public interface ICollimatorCalibrationRepository
    {
        Task<ICollimatorCalibrationInfo> FetchConfigurationInfoAsync(ICollimatorConfiguration configuration);
    }

    /// <summary>
    /// It should become a replacement to CollimatorConfigurationStore from App project
    /// TODO: switch to use of this class and remove the former one
    /// </summary>
    public class CollimatorCalibrationRepository : ICollimatorCalibrationRepository
    {
        private readonly ICoilConfigurationCommands _coilConfigurationCommands;
        private readonly IHeaterCurrentConfigCommands _heaterCurrentCommands;
        private readonly ICorrectionMatrixCommands _correctionMatrixCommands;
        private readonly IReferenceFieldCommands _referenceFieldCommands;
        private readonly IOutputFactorCommands _outputFactorCommands;

        public CollimatorCalibrationRepository(
            ICoilConfigurationCommands coilConfigurationCommands,
            IHeaterCurrentConfigCommands heaterCurrentCommands,
            ICorrectionMatrixCommands correctionMatrixCommands,
            IReferenceFieldCommands referenceFieldCommands,
            IOutputFactorCommands outputFactorCommands
            )
        {
            _coilConfigurationCommands = coilConfigurationCommands;
            _heaterCurrentCommands = heaterCurrentCommands;
            _correctionMatrixCommands = correctionMatrixCommands;
            _referenceFieldCommands = referenceFieldCommands;
            _outputFactorCommands = outputFactorCommands;
        }

        public async Task<ICollimatorCalibrationInfo> FetchConfigurationInfoAsync(ICollimatorConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "CollimatorConfigurationRepository error: input configuration is missing");
            }

            if (configuration.DefaultPreset is null)
            {
                throw new NullReferenceException("CollimatorConfigurationRepository error: input configuration preset is missing");
            }
            
            long presetId = configuration.DefaultPreset.Id;

            var fetchCoils = _coilConfigurationCommands.ReadListAsync(presetId);
            var fetchMagnetometerConfig = FetchAndValidateMagnetometerConfigurationAsync(presetId);
            var fetchHeaterCurrent = FetchAndValidateHeaterCurrentAsync(presetId);
            var fetchOutputFactors = FetchOutputFactorsAsync(presetId);

            await Task.WhenAll(fetchCoils, fetchMagnetometerConfig, fetchHeaterCurrent, fetchOutputFactors);

            return new CollimatorCalibrationInfo(
                configuration,
                fetchCoils.Result, fetchOutputFactors.Result, 
                fetchMagnetometerConfig.Result, fetchHeaterCurrent.Result, configuration.ReferencedDoseRate);
        }

        private async Task<ICollection<IOutputFactor>> FetchOutputFactorsAsync(long presetId)
        {
            var allOutputFactors = await _outputFactorCommands.ReadListAsync(presetId);
            return allOutputFactors.ToList();
        }

        public async Task<double> FetchAndValidateHeaterCurrentAsync(long presetId)
        {
            var heaterCurrentConfigurations = await _heaterCurrentCommands.ReadListAsync(presetId); // order to get latest one if there's multiple
            var heaterCurrent = heaterCurrentConfigurations?.OrderBy(cfg => cfg.Id).LastOrDefault()?.HeaterCurrent;

            // Range check:
            double heaterCurrentValue = heaterCurrent.Value;
            if (heaterCurrentValue < PhysicsValueRange.HeaterCurrentMin
                || heaterCurrentValue > PhysicsValueRange.HeaterCurrentMax)
            {
                throw new ArgumentOutOfRangeException(
                    $"{Application.Common.StringConstants.TreatmentConsole.InvalidHeaterCurrentConfiguration}: value={heaterCurrent} is out of range {PhysicsValueRange.HeaterCurrentMin}..{PhysicsValueRange.HeaterCurrentMax}");
            }

            return heaterCurrentValue;

        }
        public async Task<MagnetometerConfiguration> FetchAndValidateMagnetometerConfigurationAsync(long presetId)
        {
            MagnetometerConfiguration config = new();

            var matrices = await _correctionMatrixCommands.ReadListAsync(presetId);
            foreach (var matrix in matrices)
            {
                Matrix2x3 matrix3x2 = MagnetometerConfiguration.GetMatrix2x3(matrix);

                _ = matrix.MagnetometerType switch
                {
                    MagnetometerType.Front => config.FrontMatrix = matrix3x2,
                    MagnetometerType.Back => config.BackMatrix = matrix3x2,
                    _ => throw new InvalidOperationException($"Wrong input magnetometer type {matrix.MagnetometerType}"),
                };
            }

            var fields = await _referenceFieldCommands.ReadListAsync(presetId);
            foreach (var field in fields)
            {
                Vector3 value = MagnetometerConfiguration.GetVector3(field);

                _ = field.MagnetometerType switch
                {
                    MagnetometerType.Front => config.FrontReferenceField = value,
                    MagnetometerType.Back => config.BackReferenceField = value,
                    _ => throw new InvalidOperationException($"Wrong input magnetometer type {field.MagnetometerType}"),
                };
            }

            return config;
        }
    }
}
