using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xcc.Core.Domain.DataManagement.System;

namespace Heracles.Application.AppLayer.Collimators
{
    public interface ICollimatorModel : INotifyPropertyChanged
    {
        ObservableCollection<ICollimator> Collimators { get; }
        ICollimator ActiveCollimator { get; }
        IHead ActiveHead { get; }
        ObservableCollection<ICollimatorConfiguration> CollimatorConfigurations { get; }

        void Reset(IHead activeHead);

        void AddConfiguration(ICollimatorConfiguration configuration);
        ICollimatorConfiguration? FindConfigurationByType(TargetType collimatorType, Energy energy);
        ICollimatorConfiguration FindConfigurationById(long id);
        ICollimatorConfiguration UpdateConfigurationDoseRate(ICollimatorConfiguration collimatorConfiguration, double newDoseRateValue);

        ICollimator FindCollimatorBySerial(string collimatorSerial);
        ICollimator AddCollimator(ICollimator collimator);
        ICollimator UpdateCollimator(ICollimator collimator);
        void SetActiveCollimator(string activeCollimatorSerial);
    }
}