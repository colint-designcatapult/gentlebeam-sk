using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Application.Infra.DataManagement.System
{
    public interface ICollimatorRepository
    {
        Task<IHead> FetchActiveHeadAsync();
    
        Task<ICollection<ICollimatorConfiguration>> FetchCollimatorConfigurationsAsync();
        Task<ICollimatorConfiguration> CreateCollimatorConfigurationAsync(TargetType targetType, Energy energy);
        Task<ICollimatorConfiguration> UpdateCollimatorConfigurationAsync(ICollimatorConfiguration oldValue, ICollimatorConfiguration newValue);

        Task<ICollection<ICollimator>> FetchCollimatorsAsync(long configurationId);
        Task<ICollimator> CreateCollimatorAsync(string collimatorSerial, IHead head, ICollimatorConfiguration configuration);
        Task<ICollimator> UpdateCollimatorAsync(ICollimator oldValue, ICollimator newValue);
    }

    public class CollimatorRepository(
            IHeadCommands headCommands,
            ICollimatorConfigurationCommands collimatorConfigurationCommands,
            IPresetConfigurationCommands presetConfigurationCommands,
            ICollimatorCommands collimatorCommands,
            ILogWriter logWriter) : ICollimatorRepository
    {
        #region public methods
        public async Task<IHead> FetchActiveHeadAsync()
        {
            try
            {
                var heads = await headCommands.ReadAllAsync();
                return heads.FirstOrDefault(h => h.IsActive);
            }
            catch (Exception ex)
            {
                _ = logWriter.LogAsync($"Failed to fetch Head: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }


        public async Task<ICollection<ICollimatorConfiguration>> FetchCollimatorConfigurationsAsync()
        {
            try
            {
                var configurations = await collimatorConfigurationCommands.ReadAllAsync();

                // Before setting configurations property, we first need to set their presets:
                foreach (var config in configurations)
                {
                    var presets = await presetConfigurationCommands.ReadListAsync(config.Id);
                    config.SetPresets(presets);
                }
                return configurations;
            }
            catch (Exception ex)
            {
                _ = logWriter.LogAsync($"Failed to fetch collimators: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
                throw;
            }
        }

        public Task<ICollection<ICollimator>> FetchCollimatorsAsync(long configurationId)
        {
            return collimatorCommands.ReadListAsync(configurationId);
        }

        public async Task<ICollimatorConfiguration> CreateCollimatorConfigurationAsync(TargetType targetType, Energy energy)
        {
            var configurationToCreate = new CollimatorConfiguration()
            {
                ReferencedDoseRate = 0,
                Type = targetType,
                Energy = energy,
                SsdType = (targetType == TargetType.TargetType_30mm_SSD_7_Fields) ? SsdType.SsdType30mm : SsdType.SsdType50mm
            };


            var storedConfiguration = await collimatorConfigurationCommands.CreateAsync(configurationToCreate);
            await AddDefaultPresetAsync(storedConfiguration);
            return storedConfiguration;
        }

        public async Task<ICollimatorConfiguration> UpdateCollimatorConfigurationAsync(ICollimatorConfiguration oldValue, ICollimatorConfiguration newValue)
        {
            if (oldValue?.Id != newValue.Id)
            {
                throw new ArgumentException(
                    $"Applicator update error: new value id={newValue.Id} doesn't match to the record in the list");
            }
            return new CollimatorConfiguration(
                await collimatorConfigurationCommands.UpdateAsync(oldValue, newValue),
                presets: oldValue?.Presets);
        }

        public Task<ICollimator> CreateCollimatorAsync(string collimatorSerial, IHead head, ICollimatorConfiguration configuration)
        {
            var collimatorToCreate = new Collimator()
            {
                HeadId = head.Id,
                CollimatorConfigurationId = configuration.Id,
                Serial = collimatorSerial
            };

            return collimatorCommands.CreateAsync(collimatorToCreate);
        }

        public async Task<ICollimator> UpdateCollimatorAsync(ICollimator oldValue, ICollimator newValue)
        {
            if (oldValue?.Id != newValue.Id)
            {
                throw new ArgumentException($"Applicator update error: new value id={newValue.Id} doesn't match to the record in the list");
            }

            return new Collimator(
                await collimatorCommands.UpdateAsync(oldValue, newValue),
                configuration: newValue.Configuration
                );
        }

        #endregion public methods


        #region private methods
        private async Task<IPresetConfiguration> AddPresetAsync(ICollimatorConfiguration collimatorConfiguration, string presetName, bool isActive, bool isDefault)
        {
            var preset = new PresetConfiguration
            {
                CollimatorConfigurationId = collimatorConfiguration.Id,
                PresetName = presetName,
                IsActive = isActive,
                IsDefault = isDefault,
                CreationDate = DateTime.Now,
            };
            var storedPreset = await presetConfigurationCommands.CreateAsync(preset);
            collimatorConfiguration.AddPreset(storedPreset);
            return storedPreset;
        }

        private Task<IPresetConfiguration> AddDefaultPresetAsync(ICollimatorConfiguration collimatorConfiguration, bool isActive = true)
        {
            return AddPresetAsync(collimatorConfiguration, presetName: "Default", isActive, isDefault: true);
        }
        #endregion private methods
    }
}
