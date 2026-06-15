using Prism.Mvvm;
using Xcc.Core.Models;

namespace Heracles.Application.UI.ViewModels
{
    public class DetailedTelemetryViewModel(IGCBDataStore gcbDataStore) : BindableBase
    {
        public IGCBDataStore GCBDataStore { get; } = gcbDataStore;
    }
}
