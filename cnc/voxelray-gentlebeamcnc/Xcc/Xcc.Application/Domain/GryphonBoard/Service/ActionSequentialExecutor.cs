using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Application.Domain.GryphonBoard.Service
{
    public class ActionSequentialExecutor
    {
        public CancellationTokenSource? cancellationTokenSource;
        public IAsyncAction? CurrentAction;
        public object lockTokenSource = new();

        public async Task Execute(
            IAsyncAction action, 
            CancellationToken? externalCancellationToken = null)
        {
            CancelOngoingTask();

            CurrentAction = action;

            try
            {
                await action.RunAsync(GetNewToken(externalCancellationToken));
            }
            finally
            {
                CurrentAction = null;
            }
        }

        public void CancelOngoingTask()
        {
            lock (lockTokenSource)
            {
                if (cancellationTokenSource is not null && !cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource = null;
                }
            }
        }

        private CancellationToken GetNewToken(CancellationToken? externalCancellationToken)
        {
            lock (lockTokenSource)
            {
                cancellationTokenSource = 
                    (externalCancellationToken is null)
                    ? new CancellationTokenSource()
                    : CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value);

                return cancellationTokenSource.Token;
            }
        }

        internal bool IsBusy()
        {
            return CurrentAction != null;
        }
    }
}
