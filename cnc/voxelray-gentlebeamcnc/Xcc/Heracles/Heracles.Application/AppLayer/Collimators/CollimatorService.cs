using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Service;

namespace Heracles.Application.AppLayer.Collimators
{
    public class CollimatorService(
        ICollimatorModel collimatorModel,
        ICollimatorRepository collimatorRepository
        )
    {
        public async Task UpdateCollimatorModelAsync()
        {
            // First we fetch all the data, and only when it succeeds, we reset the collimator model:
            var head = await collimatorRepository.FetchActiveHeadAsync();
            
            // If no active head exists, ensure one is created/activated
            if (head is null)
            {
                head = await collimatorRepository.EnsureActiveHeadExistsAsync();
            }
            
            var configurations = await collimatorRepository.FetchCollimatorConfigurationsAsync();
            var collimators = new List<ICollimator>();

            foreach (var configuration in configurations)
            {
                var configCollimators = await collimatorRepository.FetchCollimatorsAsync(configuration.Id);
                collimators.AddRange(configCollimators);
            }
            collimators = collimators.OrderBy(x => x.Id).ToList();

            // Publish one complete snapshot so consumers never observe a partially rebuilt model.
            collimatorModel.Reset(head, configurations, collimators);
        }

        public async Task<ICollimator> CreateCollimatorAsync(string serial, TargetType targetType, Energy energy, bool isActive, IActionAuditService? audit = null)
        {
            if (serial == null)
            {
                throw new ArgumentNullException(nameof(serial));
            }
            else if (collimatorModel.FindCollimatorBySerial(serial) != null)
            {
                throw new ArgumentException($"Create applicator - error: collimator with serial={serial} already exists");
            }
            else if (collimatorModel.ActiveHead is null)
            {
                throw new NullReferenceException("Create applicator - error: active head is not specified");
            }

            ICollimatorConfiguration configuration = await FindOrCreateConfiguration(targetType, energy, audit);

            var storedCollimator = await collimatorRepository.CreateCollimatorAsync(
                serial,
                collimatorModel.ActiveHead,
                configuration,
                isActive);
            audit?.RegisterAction("Configuration created",
                $"Entity=Collimator; Id={storedCollimator.Id}; Fields=Serial,CollimatorConfigurationId,IsActive");

            return collimatorModel.AddCollimator(storedCollimator);
        }

        public async Task<ICollimator> UpdateCollimatorAsync(string serial, TargetType targetType, Energy energy, bool isActive, IActionAuditService? audit = null)
        {
            var existingValue = collimatorModel.FindCollimatorBySerial(serial);

            ICollimatorConfiguration configuration = await FindOrCreateConfiguration(targetType, energy, audit);
            var newValue = new Collimator(existingValue)
            {
                CollimatorConfigurationId = configuration.Id,
                IsActive = isActive,
            };
            var previousConfigurationId = existingValue.CollimatorConfigurationId;
            var previousIsActive = existingValue.IsActive;
            var fields = new List<string>();
            if (previousConfigurationId != newValue.CollimatorConfigurationId)
                fields.Add(nameof(newValue.CollimatorConfigurationId));
            if (previousIsActive != newValue.IsActive)
                fields.Add(nameof(newValue.IsActive));
            if (fields.Count == 0)
                return existingValue;

            var storedValue = await collimatorRepository.UpdateCollimatorAsync(existingValue, newValue);
            fields.Clear();
            if (previousConfigurationId != storedValue.CollimatorConfigurationId)
                fields.Add(nameof(storedValue.CollimatorConfigurationId));
            if (previousIsActive != storedValue.IsActive)
                fields.Add(nameof(storedValue.IsActive));
            if (fields.Count > 0)
                audit?.RegisterAction("Configuration saved",
                    $"Entity=Collimator; Id={storedValue.Id}; Fields={string.Join(",", fields)}");

            return collimatorModel.UpdateCollimator(storedValue);
        }

        /// <summary>
        /// Updates dose rate field for a specified CollimatorConfiguration.
        /// Because of high dependency of Physics tabs on the same CollimatorConfiguration instance,
        /// we just update its dose rate field, and not updating entire instance or list of instances
        /// </summary>
        /// <param name="collimatorConfiguration"></param>
        /// <param name="doseRate"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<ICollimatorConfiguration> UpdateCollimatorConfigurationDoseRateAsync(long configurationId, double doseRate, IActionAuditService? audit = null)
        {
            var configurationToUpdate = collimatorModel.FindConfigurationById(configurationId);
            if (configurationToUpdate == null)
            {
                throw new ArgumentException($"Applicator configuration update error: no configuration with id={configurationId} in the list");
            }

            var previousDoseRate = configurationToUpdate.ReferencedDoseRate;
            var updatedConfiguration = await collimatorRepository.UpdateCollimatorConfigurationAsync(
                configurationToUpdate,
                new CollimatorConfiguration(configurationToUpdate) { ReferencedDoseRate = (float)doseRate });
            if (updatedConfiguration.ReferencedDoseRate != previousDoseRate)
                audit?.RegisterAction("Configuration saved",
                    $"Entity=CollimatorConfiguration; Id={updatedConfiguration.Id}; Fields=ReferencedDoseRate");

            // For now, we just update the existing value in the model,
            // in order to maintain model consistency more easily:
            return collimatorModel.UpdateConfigurationDoseRate(configurationToUpdate, updatedConfiguration.ReferencedDoseRate);
        }

        private async Task<ICollimatorConfiguration> FindOrCreateConfiguration(TargetType targetType, Energy energy, IActionAuditService? audit)
        {
            // Find a matching configuration, and if there's no such, create a new one:
            var configuration = collimatorModel.FindConfigurationByType(targetType, energy);
            if (configuration is null)
            {
                configuration = await collimatorRepository.CreateCollimatorConfigurationAsync(targetType, energy, audit);
                collimatorModel.AddConfiguration(configuration);
            }

            return configuration;
        }
    }
}
