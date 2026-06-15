using Xcc.Application.UI.Mvvm;
using Xcc.Core.Models;

namespace Heracles.Application.UI.ViewModels
{
    public class InterlocksDialogViewModel(IGCBDataStore gcbDataStore) : DialogViewModelBase
    {
        public IGCBDataStore GCBDataStore { get; } = gcbDataStore;
    }
}
