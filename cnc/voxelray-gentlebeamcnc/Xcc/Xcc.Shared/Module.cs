using Prism.Ioc;
using Prism.Modularity;

using Xcc.Shared.Views;

namespace Xcc.Shared
{
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LogView>();
            containerRegistry.RegisterDialog<ReportView>();
            containerRegistry.RegisterDialog<DialogBoxView>();
            containerRegistry.RegisterDialog<FaultsView>();
            containerRegistry.RegisterDialog<DatePickerDialogView>();
            containerRegistry.RegisterDialog<EnterStringDialogView>();
            containerRegistry.RegisterDialog<TelemetryDialogView>();
            containerRegistry.RegisterDialog<CreateFilterDialogView>();
        }
    }
}
