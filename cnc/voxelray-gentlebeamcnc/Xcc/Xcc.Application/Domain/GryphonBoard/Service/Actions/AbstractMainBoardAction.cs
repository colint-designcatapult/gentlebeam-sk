using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public abstract class AbstractMainBoardAction : IAsyncAction
    {
        public IMainBoardState MainBoard { get; }
        public IEnumerable<GcbStateNew> FromStates { get; }
        public IEnumerable<GcbStateNew> ToStates { get; }

        protected AbstractMainBoardAction(
            IMainBoardState mainBoardState,
            IEnumerable<GcbStateNew> fromStates,
            IEnumerable<GcbStateNew> toStates)
        {
            MainBoard = mainBoardState;
            FromStates = fromStates;
            ToStates = toStates;
        }

        protected abstract Task RunActionAsync(CancellationToken token);

        public virtual bool CanRun()
        {
            // TODO: temporary uses SystemCrash state as a substitution to NoConnection
            return FromStates.Contains(MainBoard.State ?? GcbStateNew.SystemCrash);
        }

        public virtual async Task RunAsync(CancellationToken token)
        {
            if (!CanRun())
            {
                throw new Exception("Cannot run the action");
            }
            await RunActionAsync(token);
            await WaitForAnyState(ToStates, token);
        }

        protected async Task<GcbStateNew?> WaitForAnyState(
            IEnumerable<GcbStateNew> expectedStates,
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var state = MainBoard.State;
                if (state is not null && expectedStates.Contains(state.Value))
                {
                    return MainBoard.State;
                }

                await Task.Delay(50, cancellationToken);
            }

            throw new TaskCanceledException($"Failed to wait for expected state {expectedStates}: task was cancelled.");
        }

        protected async Task<GcbStateNew> WaitForState(GcbStateNew expectedState, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (expectedState == MainBoard.State)
                {
                    return expectedState;
                }

                await Task.Delay(50, token);
            }

            throw new TaskCanceledException($"Failed to wait for expected state {expectedState}: task was cancelled.");
        }
    }
}
