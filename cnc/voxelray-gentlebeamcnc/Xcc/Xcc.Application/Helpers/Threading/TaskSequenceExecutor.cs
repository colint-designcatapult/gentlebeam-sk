using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcc.Application.Helpers.Threading
{
    public class TaskSequenceExecutor
    {
        public int Enqueue(Func<Task> taskDelegate)
        {
            if (ScheduledTaskChannel.Writer.TryWrite(taskDelegate))
                return ScheduledTaskChannel.Reader.Count;

            return -1;
        }
        
        public Task Execute(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (ExecutionTask != null)
                    return ExecutionTask;

                CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); 
                var taskCancellationToken = CancellationTokenSource.Token;
                var reader = ScheduledTaskChannel.Reader;
                return ExecutionTask ??= Task.Run(
                    async () =>
                    {
                        //try
                        //{
                            while (await reader.WaitToReadAsync(taskCancellationToken))
                            {
                                // We first peek the task from the channel,
                                // and remove it from there only on success,
                                // otherwise keeping it there for retry
                                while (reader.TryPeek(out var task))
                                {
                                    await ExecuteTask(task);
                                    Debug.WriteLine($"TaskCount = {reader.Count}");
                                    await reader.ReadAsync(taskCancellationToken);
                                }
                            }
                        //}
                        //catch (TaskCanceledException)
                        //{
                            //if (taskCancellationToken.IsCancellationRequested)
                            //{
                            //    // just quit, as we stop the execution
                            //    Debug.WriteLine("TaskSequenceExecutor: stop execution task on cancellation request");
                            //}
                            //else throw;// probably some inner task failed
                        //}
                    }, taskCancellationToken);
            }
        }

        protected virtual async Task ExecuteTask(Func<Task> task)
        {
            await task();
        }

        public Task Retry(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (ExecutionTask is null || ScheduledTaskChannel is null)
                {
                    throw new NullReferenceException("TaskSequenceExecutor: cannot retry - no tasks");
                }

                if (!ExecutionTask.IsFaulted)
                {
                    if (ExecutionTask.IsCompletedSuccessfully)
                        return ExecutionTask;
                    throw new InvalidOperationException("TaskSequenceExecutor: cannot retry - invalid execution task state");
                }

                ExecutionTask = null;
            }
            return Execute(cancellationToken);
        }

        public void Reset()
        {
            lock (_lock)
            {
                if (ExecutionTask is not null)
                {
                    CancellationTokenSource?.Cancel();
                    ScheduledTaskChannel = NewChannel();
                    ExecutionTask = null;
                }
            }
        }

        public void Complete()
        {
            ScheduledTaskChannel.Writer.Complete();
            CancellationTokenSource?.Cancel();
        }
        
        #region Properties
        Channel<Func<Task>> ScheduledTaskChannel { get; set; } = NewChannel();

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource CancellationTokenSource
        {
            get => _cancellationTokenSource;
            set
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = value;
            }
        }

        public Task? ExecutionTask { get; private set; }

        public bool IsCancelled => ExecutionTask == null;

        public int Count => ScheduledTaskChannel.Reader.Count;
        #endregion Properties

        private static Channel<Func<Task>> NewChannel()
        {
            return Channel.CreateUnbounded<Func<Task>>();
        }

        private readonly object _lock = new object();
    }
}
