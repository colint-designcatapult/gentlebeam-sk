using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var configurations = await collimatorRepository.FetchCollimatorConfigurationsAsync();
            var collimators = new List<ICollimator>();

            foreach (var configuration in configurations)
            {
                var configCollimators = await collimatorRepository.FetchCollimatorsAsync(configuration.Id);
                collimators.AddRange(configCollimators);
            }
            collimators = collimators.OrderBy(x => x.Id).ToList();

            // Now we can fill the model:
            collimatorModel.Reset(head);
            foreach (var configuration in configurations)
            {
                collimatorModel.AddConfiguration(configuration);
            }
            foreach (var collimator in collimators)
            {
                collimatorModel.AddCollimator(collimator);
            }
        }

        public async Task<ICollimator> CreateCollimatorAsync(string serial, TargetType targetType, Energy energy, bool isActive)
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

            ICollimatorConfiguration configuration = await FindOrCreateConfiguration(targetType, energy);

            var storedCollimator = await collimatorRepository.CreateCollimatorAsync(
                serial,
                collimatorModel.ActiveHead,
                configuration,
                isActive);

            return collimatorModel.AddCollimator(storedCollimator);
        }

        public async Task<ICollimator> UpdateCollimatorAsync(string serial, TargetType targetType, Energy energy, bool isActive)
        {
            var existingValue = collimatorModel.FindCollimatorBySerial(serial);

            ICollimatorConfiguration configuration = await FindOrCreateConfiguration(targetType, energy);
            var newValue = new Collimator(existingValue)
            {
                CollimatorConfigurationId = configuration.Id,
                IsActive = isActive,
            };

            var storedValue = await collimatorRepository.UpdateCollimatorAsync(existingValue, newValue);

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
        public async Task<ICollimatorConfiguration> UpdateCollimatorConfigurationDoseRateAsync(long configurationId, double doseRate)
        {
            var configurationToUpdate = collimatorModel.FindConfigurationById(configurationId);
            if (configurationToUpdate == null)
            {
                throw new ArgumentException($"Applicator configuration update error: no configuration with id={configurationId} in the list");
            }

            var updatedConfiguration = await collimatorRepository.UpdateCollimatorConfigurationAsync(
                configurationToUpdate,
                new CollimatorConfiguration(configurationToUpdate) { ReferencedDoseRate = (float)doseRate });

            // For now, we just update the existing value in the model,
            // in order to maintain model consistency more easily:
            return collimatorModel.UpdateConfigurationDoseRate(configurationToUpdate, doseRate);
        }

        private async Task<ICollimatorConfiguration> FindOrCreateConfiguration(TargetType targetType, Energy energy)
        {
            // Find a matching configuration, and if there's no such, create a new one:
            var configuration = collimatorModel.FindConfigurationByType(targetType, energy);
            if (configuration is null)
            {
                configuration = await collimatorRepository.CreateCollimatorConfigurationAsync(targetType, energy);
                collimatorModel.AddConfiguration(configuration);
            }

            return configuration;
        }
    }
}
