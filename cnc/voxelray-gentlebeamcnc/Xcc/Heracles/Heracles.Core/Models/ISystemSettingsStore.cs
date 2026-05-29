using System.ComponentModel;

namespace Heracles.Core.Models
{
    public interface ISystemSettingsStore : INotifyPropertyChanged
    {
        ISystemSettings Settings { get; set; }
    }
}
