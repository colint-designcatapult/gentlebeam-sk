using Prism.Events;
using Xcc.Application.ViewModels;
using Xcc.Core.Models;

namespace Heracles.Application.UI.ViewModels
{
    public class MonitorViewModel(IGCBDataStore gcbDataStore, IEventAggregator eventAggregator) : TelemetryViewModel(gcbDataStore, eventAggregator)
    {
    }
}
