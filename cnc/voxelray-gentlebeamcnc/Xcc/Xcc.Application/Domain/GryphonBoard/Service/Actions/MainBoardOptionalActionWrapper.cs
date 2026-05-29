using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class MainBoardOptionalActionWrapper : IAsyncAction
    {
        private readonly AbstractMainBoardAction action;

        public MainBoardOptionalActionWrapper(AbstractMainBoardAction action)
        {
            this.action = action;
        }

        public bool CanRun()
        {
            return action.CanRun() || CanSkip();
        }

        public async Task RunAsync(CancellationToken token)
        {
            if (!CanSkip())
            {
                await action.RunAsync(token);
            }
        }

        /// <summary>
        /// By default, we can skip an action 
        /// if the board is already in a desired state
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanSkip()
        {
            // TODO: temporary uses SystemCrash state as a substitution to NoConnection
            return action.ToStates.Contains(action.MainBoard.State ?? GcbStateNew.SystemCrash);
        }
    }
}
