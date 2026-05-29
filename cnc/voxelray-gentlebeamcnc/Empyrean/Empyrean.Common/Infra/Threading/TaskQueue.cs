namespace Empyrean.Common.Infra.Threading
{
    public class TaskQueue
    {
        public int SemaphoreCount { get; private set; } = 1;

        private readonly SemaphoreSlim _semaphore;

        public TaskQueue()
        {
            _semaphore = new SemaphoreSlim(SemaphoreCount);
        }

        public TaskQueue(int semaphoreCount)
        {
            SemaphoreCount = semaphoreCount;
            _semaphore = new SemaphoreSlim(semaphoreCount);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await taskGenerator();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task Enqueue(Func<Task> taskGenerator)
        {
            await _semaphore.WaitAsync();
            try
            {
                await taskGenerator();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
