using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;

namespace Xcc.Application.UI.Mvvm
{
    public abstract class ViewModelBase(IEventAggregator? eventAggregator, IDialogService? dialogService): BindableBase, IDestructible
    {
        protected IEventAggregator? EventAggregator { get; } = eventAggregator;
        public IDialogService? DialogService { get; } = dialogService;

        #region IDestructible
        public virtual void Destroy() { }
        #endregion IDestructible
    }
}
