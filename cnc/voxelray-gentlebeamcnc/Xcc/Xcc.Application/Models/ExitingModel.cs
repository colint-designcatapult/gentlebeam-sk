using Prism.Events;
using Xcc.Application.Events;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public interface IExitingModel
    {
        void ExitApplication();
        bool CanExitApplication();
    }

    public class ExitingModel : IExitingModel
    {
        public ExitingModel(IEventAggregator eventAggregator, IAppGlobals appGlobals)
        {
            EventAggregator = eventAggregator;
            AppGlobals = appGlobals;
        }


        #region Properties
        private IEventAggregator EventAggregator { get; }
        private IAppGlobals AppGlobals { get; }
        #endregion Properties


        #region Commands
        public void ExitApplication()
        {
            EventAggregator.GetEvent<ExitApplicationEvent>().Publish();

            AppGlobals.AppCancellationTokenSource.Cancel();
            // TODO: premature disposal causes crashes on app exit
            //AppGlobals.AppCancellationTokenSource.Dispose();

            System.Windows.Application.Current.Shutdown();
        }

        public bool CanExitApplication()
        {
            return true;
        }
        #endregion Commands
    }
}
