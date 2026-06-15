using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Application.Helpers.Threading
{
    /// <summary>
    /// This is an attempt to implement an abstraction for tasks with infinite loops
    /// (typically, with cancellation by token from outside)
    /// </summary>
    public interface IJob
    {
        void RunJob();
        void OnCancelled();

    }

    public interface IAsyncJob
    {
        Task RunJob(CancellationToken token);
        void OnCancelled();
        void OnFailed();
    }


    public class LoopTask
    {
        public async Task Loop(IJob job, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await OnIterationBegins(token);
                job.RunJob();
                await OnIterationEnds(token);
            }
        }

        public async Task Loop(IAsyncJob job, CancellationToken token)
        {
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await OnIterationBegins(token);
                    await job.RunJob(token);
                    await OnIterationEnds(token);
                }
            }
            catch (TaskCanceledException)
            {
                job.OnCancelled();
                throw;
            }
            catch
            {
                job.OnFailed();
                throw;
            }
        }

        protected virtual Task OnIterationBegins(CancellationToken token)
        {
            return Task.CompletedTask;
        }
        protected virtual Task OnIterationEnds(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }


    public class LoopWithDelay(int timeoutMs) : LoopTask
    {
        protected override Task OnIterationEnds(CancellationToken token)
        {
            return Task.Delay(timeoutMs, token);
        }
    }
}
