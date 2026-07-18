using Prism.Events;
using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public class SystemTelemetryChangedEvent : PubSubEvent<ISystemTelemetry?> { }

    public class GCBDataStore(IEventAggregator eventAggregator) : BindableBase, IGCBDataStore
    {
        public IEventAggregator EventAggregator { get; } = eventAggregator;

        private ISystemTelemetry? _systemTelemetry;
        public ISystemTelemetry? SystemTelemetry
        {
            get => _systemTelemetry;
            set
            {
                if (SetProperty(ref _systemTelemetry, value))
                {
                    EventAggregator.GetEvent<SystemTelemetryChangedEvent>().Publish(_systemTelemetry);
                }
            }
        }

    }
}
