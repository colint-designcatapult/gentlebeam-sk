using Heracles.Application.Domain.DataManagement.System.Collimators;
using System.ComponentModel;

namespace Heracles.Application.AppLayer.Collimators;

public interface IApplicatorReadinessSource : INotifyPropertyChanged
{
    ICollimatorConfiguration? CollimatorConfiguration { get; }
}
