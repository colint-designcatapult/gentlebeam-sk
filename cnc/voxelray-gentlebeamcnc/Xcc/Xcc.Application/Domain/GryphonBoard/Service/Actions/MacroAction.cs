using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class MacroAction : IAsyncAction
    {
        private readonly IEnumerable<IAsyncAction> actions;

        public MacroAction(IEnumerable<IAsyncAction> actions)
        {
            this.actions = actions;
        }

        public virtual bool CanRun()
        {
            return actions.First().CanRun();
        }

        public async Task RunAsync(CancellationToken token)
        {
            if (!CanRun())
            {
                throw new InvalidOperationException("Cannot run the macroaction");
            }
            foreach (IAsyncAction action in actions)
            {
                await action.RunAsync(token);
            }
        }
    }
}
