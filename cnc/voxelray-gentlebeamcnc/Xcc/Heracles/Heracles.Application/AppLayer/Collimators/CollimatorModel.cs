using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xcc.Application.AppLayer.Service.TreatmentConsole;
using Xcc.Core.Domain.DataManagement.System;

namespace Heracles.Application.AppLayer.Collimators
{
    public class CollimatorModel : BindableBase, ICollimatorModel, IActiveHeadProvider
    {
        private ObservableCollection<ICollimatorConfiguration> _collimatorConfigurations = [];
        private ObservableCollection<ICollimator> _collimators = [];
        private ICollimator _activeCollimator;
        private IHead _activeHead;

        public IHead ActiveHead { get => _activeHead; private set => SetProperty(ref _activeHead, value); }
        public ObservableCollection<ICollimatorConfiguration> CollimatorConfigurations => _collimatorConfigurations;
        public ObservableCollection<ICollimator> Collimators
        {
            get => _collimators;
            private set => SetProperty(ref _collimators, value);
        }
        public ICollimator ActiveCollimator { get => _activeCollimator; private set => SetProperty(ref _activeCollimator, value); }

        public void Reset(IHead activeHead)
        {
            Collimators.Clear();
            ActiveCollimator = null;
            CollimatorConfigurations.Clear();
            ActiveHead = activeHead;
        }

        public void AddConfiguration(ICollimatorConfiguration configuration)
        {
            if (CollimatorConfigurations.Any(x => x.Id == configuration.Id))
            {
                throw new ArgumentException($"Collimator model - configuration registering error: id={configuration.Id} already exists");
            }
            CollimatorConfigurations.Add(configuration);
        }

        public ICollimatorConfiguration FindConfigurationByType(TargetType collimatorType, Energy energy)
        {
            var matchingConfiguration = CollimatorConfigurations?.FirstOrDefault(c => c.Type == collimatorType && c.Energy == energy);

            //if (matchingConfiguration == null && Enum.IsDefined(collimatorType) && Enum.IsDefined(energy))
            //{
            //    string energyTypeStr = energy.GetAttribute<DisplayAttribute>().Name;
            //    string collimatorTypesStr = collimatorType.GetAttribute<DisplayAttribute>().Name;
            //    string msg = $"Cannot find any collimator configuration for Type=\"{collimatorTypesStr}\", Energy={energyTypeStr}kV";
            //    _ = LogWriter.LogAsync(msg, LogRecordSeverity.Error, LogRecordType.Error);
            //}

            return matchingConfiguration;
        }

        public ICollimatorConfiguration FindConfigurationById(long id)
        {
            return CollimatorConfigurations.FirstOrDefault(x => x.Id == id);
        }

        public ICollimatorConfiguration UpdateConfigurationDoseRate(
            ICollimatorConfiguration collimatorConfiguration,
            double newDoseRateValue)
        {
            // For now, we just update the existing value in the model,
            // in order to maintain model consistency more easily:
            if (!CollimatorConfigurations.Contains(collimatorConfiguration))
            {
                throw new ArgumentException("Collimator model - configuration update error: the provided configuration is missing");
            }

            // Ok, we have it on the list, so we can update it now:
            collimatorConfiguration.ReferencedDoseRate = newDoseRateValue;
            // And just in case raise active collimator change event, if needed:
            if (ActiveCollimator?.Configuration?.Id == collimatorConfiguration.Id)
            {
                RaisePropertyChanged(nameof(ActiveCollimator));
            }
            return collimatorConfiguration;
        }

        public ICollimator FindCollimatorBySerial(string collimatorSerial)
        {
            return Collimators.FirstOrDefault(c => c.Serial == collimatorSerial);
        }

        public ICollimator AddCollimator(ICollimator collimator)
        {
            if (FindCollimatorBySerial(collimator.Serial) != null)
            {
                throw new ArgumentException($"Collimator model - collimator registering error: id={collimator.Id} already exists");
            }

            // To be consistent, set configuration to the input collimator value
            var configuration = FindConfigurationById(collimator.CollimatorConfigurationId);
            collimator.Configuration = configuration;

            // To update entire list for all who listen to its change:
            var collimators = Collimators.ToList();
            collimators.Add(collimator);
            Collimators = new(collimators);

            UpdateActiveCollimator(collimator);

            return collimator;
        }

        public ICollimator UpdateCollimator(ICollimator newCollimatorValue)
        {
            var existingCollimator = FindCollimatorBySerial(newCollimatorValue.Serial);
            var actualConfiguration = FindConfigurationById(newCollimatorValue.CollimatorConfigurationId);
            if (existingCollimator is null)
            {
                throw new ArgumentException($"Collimator model - collimator update error: id={newCollimatorValue.Id} is missing");
            }
            else if (actualConfiguration is null)
            {
                throw new ArgumentException($"Collimator model - collimator update error: configuration id={newCollimatorValue.CollimatorConfigurationId} is missing");
            }
            else if (existingCollimator.Id != newCollimatorValue.Id)
            {
                throw new ArgumentException($"Collimator model - collimator update error: new value id={newCollimatorValue.Id} doesn't match to the record in the list");
            }

            // Just to be consistent, set configuration to the input collimator value
            newCollimatorValue.Configuration = actualConfiguration;
            
            var collimators = Collimators.ToList();
            collimators[collimators.IndexOf(existingCollimator)] = newCollimatorValue;
            Collimators = new(collimators);


            UpdateActiveCollimator(newCollimatorValue);

            return newCollimatorValue;
        }

        public void SetActiveCollimator(string activeCollimatorSerial)
        {
            if (activeCollimatorSerial is null)
            {
                ActiveCollimator = null;
            }
            else
            {
                var existingActiveCollimatorEntry = FindCollimatorBySerial(activeCollimatorSerial);
                ActiveCollimator = existingActiveCollimatorEntry ?? new Collimator { Serial = activeCollimatorSerial };
            }
        }

        private void UpdateActiveCollimator(ICollimator candidate = null)
        {
            if (candidate != null)
            {
                if (candidate.Serial == ActiveCollimator?.Serial)
                {
                    ActiveCollimator = candidate;
                }
            }
            else
            {
                // We just try to look for an existing collimator with the same serial again (if active was defined)
                SetActiveCollimator(ActiveCollimator?.Serial);
            }
        }
    }
}