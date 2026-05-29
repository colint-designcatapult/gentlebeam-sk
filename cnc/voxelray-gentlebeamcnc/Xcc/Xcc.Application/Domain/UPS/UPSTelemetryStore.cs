using System;
using Prism.Mvvm;

using Xcc.Core.Domain.UPS;

namespace Xcc.Application.Domain.UPS
{
    public class UPSTelemetryStore : BindableBase, IUPSTelemetryStore
    {
        private IUpsTelemetry? _primary;
        public IUpsTelemetry? Primary
        {
            get => _primary;
            set
            {
                if (SetProperty(ref _primary, value))
                {
                    if (_primary == null)
                        BatteryInUseStateUpdated?.Invoke(this, null);
                    else 
                        _primary.BatteryInUseStateUpdated += OnBatteryInUseStateUpdated;
                }
            }
        }
        
        private void OnBatteryInUseStateUpdated(object? sender, bool? e)
        {
            var primaryNotInUse = _primary?.BatteryNotInUse;

            if (primaryNotInUse == null)
            {
                BatteryInUseStateUpdated?.Invoke(sender, null);
                return;
            }

            BatteryInUseStateUpdated?.Invoke(sender, !primaryNotInUse.Value);
        }

        public event EventHandler<bool?>? BatteryInUseStateUpdated;
    }
}
