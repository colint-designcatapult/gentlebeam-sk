using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class ClearFaults : IAsyncAction
    {
        private readonly IMainBoardState mainBoard;
        private readonly IGcbCommandInterface gcbCommands;

        public ClearFaults(
            IMainBoardState mainBoardState,
            IGcbCommandInterface gcbCommands)
        {
            this.mainBoard = mainBoardState;
            this.gcbCommands = gcbCommands;
        }

        public bool CanRun()
        {
            return mainBoard.CanClearFaults();
        }

        public async Task RunAsync(CancellationToken token)
        {
            var initialState = mainBoard.State!.Value;
            if (!CanRun()) 
            {
                throw new Exception("Cannot run the ClearFaults action");
            }
            await gcbCommands.ClearFaults();
            await WaitForStateChange(initialState, token);
        }

        protected async Task<GcbStateNew> WaitForStateChange(GcbStateNew initialState, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var state = mainBoard.State;
                if (state is not null && initialState != state)
                {
                    return state.Value;
                }

                await Task.Delay(50, token);
            }

            throw new TaskCanceledException($"Failed to wait for initial board state {initialState} to change: task was cancelled.");
        }
    }
}
