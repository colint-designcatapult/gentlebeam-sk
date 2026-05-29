using Heracles.Core.Models;
using Prism.Mvvm;

namespace Heracles.Application.Models.Settings
{
    public class SystemSettingsStore : BindableBase, ISystemSettingsStore
    {
        private ISystemSettings _settings;
        public ISystemSettings Settings
        {
            get => _settings;
            set
            {
                SetProperty(ref _settings, value);
            }
        }
    }
}
