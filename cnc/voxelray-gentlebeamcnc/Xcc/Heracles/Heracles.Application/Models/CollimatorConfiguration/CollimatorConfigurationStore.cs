using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.Domain.System;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;

namespace Heracles.Application.Models.CollimatorConfiguration
{
    public interface ICollimatorConfigurationStore
    {
        IHeaterCurrentConfig HeaterCurrent { get; }
        ICollection<ICoilConfigurationEntry> CoilConfigurations { get; }

        MagnetometerConfiguration MagnetometerConfiguration { get; }

        Task FetchConfigurationAsync(Energy energy, TargetType targetType);
    }


    public class CollimatorConfigurationStore : ICollimatorConfigurationStore
    {
        public IHeaterCurrentConfig HeaterCurrent { get; private set; }
        public ICollection<ICoilConfigurationEntry> CoilConfigurations { get; private set; }
        public MagnetometerConfiguration MagnetometerConfiguration { get; private set; }
        private ICollimatorModel CollimatorModel { get; }
        public CollimatorService CollimatorService { get; }
        private ICoilConfigurationCommands CoilConfigurationCommands { get; }
        private IHeaterCurrentConfigCommands HeaterCurrentCommands { get; }
        private ICorrectionMatrixCommands CorrectionMatrixCommands { get; }
        private IReferenceFieldCommands ReferenceFieldCommands { get; }

        public CollimatorConfigurationStore(
            ICollimatorModel collimatorModel,
            CollimatorService collimatorService,
            ICoilConfigurationCommands coilConfigurationCommands,
            IHeaterCurrentConfigCommands heaterCurrentCommands,
            ICorrectionMatrixCommands correctionMatrixCommands,
            IReferenceFieldCommands referenceFieldCommands)
        {
            CollimatorModel = collimatorModel;
            CollimatorService = collimatorService;
            CoilConfigurationCommands = coilConfigurationCommands;
            HeaterCurrentCommands = heaterCurrentCommands;
            CorrectionMatrixCommands = correctionMatrixCommands;
            ReferenceFieldCommands = referenceFieldCommands;
        }

        public async Task FetchConfigurationAsync(Energy energy, TargetType collimatorType)
        {
            if (CollimatorModel.CollimatorConfigurations == null)
            {
                await CollimatorService.UpdateCollimatorModelAsync();
            }

            ICollimatorConfiguration matchingCollimatorConfiguration = CollimatorModel.FindConfigurationByType(collimatorType, energy);

            if (matchingCollimatorConfiguration == null)
            {
                throw new NullReferenceException("Cannot find a matching applicator configuration");
            }
            else if (matchingCollimatorConfiguration.DefaultPreset is null)
            {
                throw new NullReferenceException("Cannot find a proper applicator preset");
            }

            CoilConfigurations = await CoilConfigurationCommands.ReadListAsync(matchingCollimatorConfiguration.DefaultPreset.Id);
            
            var t1 = FetchAndValidateHeaterCurrentAsync(matchingCollimatorConfiguration);
            var t2 = FetchAndValidateMagnetometerConfigurationAsync(matchingCollimatorConfiguration);

            await Task.WhenAll(t1, t2);
        }

        private async Task FetchAndValidateHeaterCurrentAsync(ICollimatorConfiguration matchingCollimatorConfiguration)
        {
            var heaterCurrentConfigurations = (await HeaterCurrentCommands.ReadListAsync(matchingCollimatorConfiguration.DefaultPreset.Id))?.OrderBy(cfg => cfg.Id);
            HeaterCurrent = heaterCurrentConfigurations?.LastOrDefault();

            if (HeaterCurrent?.HeaterCurrent is null)
            {
                HeaterCurrent = null; // just set the entire config to null to not be able to use it later
            }

            // Range check:
            double heaterCurrent = HeaterCurrent.HeaterCurrent.Value;
            if (heaterCurrent < PhysicsValueRange.HeaterCurrentMin
                || heaterCurrent > PhysicsValueRange.HeaterCurrentMax)
            {
                throw new ArgumentOutOfRangeException($"Invalid heater current configuration: value={heaterCurrent} is out of range {PhysicsValueRange.HeaterCurrentMin}..{PhysicsValueRange.HeaterCurrentMax}");
            }
        }
        private async Task FetchAndValidateMagnetometerConfigurationAsync(ICollimatorConfiguration matchingCollimatorConfiguration)
        {
            MagnetometerConfiguration config = new();

            long presetId = matchingCollimatorConfiguration.DefaultPreset.Id;

            var matrices = await CorrectionMatrixCommands.ReadListAsync(presetId);
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

            var fields = await ReferenceFieldCommands.ReadListAsync(presetId);
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

            MagnetometerConfiguration = config;
        }
    }
}
