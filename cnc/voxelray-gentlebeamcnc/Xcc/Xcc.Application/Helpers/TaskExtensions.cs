using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Application.Helpers
{
    public static class TaskExtensions
    {
        public static readonly int GracefulCancelWaitSec = 5;

        public static async Task<T> RunWithTimeoutAsync<T>(
            Func<CancellationToken, Task<T>> work,
            TimeSpan timeout,
            TimeSpan gracefulCancelWait = default)
        {
            if (work == null) 
                throw new ArgumentNullException(nameof(work));
            if (timeout <= TimeSpan.Zero) 
                throw new ArgumentOutOfRangeException(nameof(timeout));

            if (gracefulCancelWait == default) 
                gracefulCancelWait = TimeSpan.FromSeconds(GracefulCancelWaitSec);

            using var cts = new CancellationTokenSource();
            var workTask = work(cts.Token);
            var timeoutTask = Task.Delay(timeout);

            var completed = await Task.WhenAny(workTask, timeoutTask).ConfigureAwait(false);
            if (completed == workTask)
                return await workTask.ConfigureAwait(false);
            
            // timeout -> cancel
            try
            {
                cts.Cancel();
            }
            catch
            {
                // ignored
            }

            // wait for graceful task completion
            var finished = await Task.WhenAny(workTask, Task.Delay(gracefulCancelWait)).ConfigureAwait(false);
            if (finished == workTask)
                return await workTask.ConfigureAwait(false);

            throw new TimeoutException($"Operation did not complete within {timeout}.");
        }

        public static Task RunWithTimeoutAsync(
            Func<CancellationToken, Task> work,
            TimeSpan timeout,
            TimeSpan gracefulCancelWait = default)
            => RunWithTimeoutAsync<object>(
                async token =>
                {
                    await work(token).ConfigureAwait(false); 
                    return null!;
                },
                timeout,
                gracefulCancelWait);
    }
}
