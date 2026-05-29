using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.Common;
using Xcc.Application.UI.Mvvm;

namespace Xcc.Application.Domain.QualityAssurance
{
    public abstract class QaTabViewModelBase : RegionViewModelBase
    {
        public QaTabViewModelBase(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IDialogService dialogService)
            : base(regionManager, eventAggregator, dialogService)
        {
            CommonProperties = new CommonProperties(eventAggregator);
        }

        public CommonProperties CommonProperties { get; }
    }
}
