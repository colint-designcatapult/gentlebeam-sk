using Empyrean.Common.Infra.Threading;
using NUnit.Framework.Legacy;

namespace Empyrean.Common.Test.Infra.Threading
{
    internal class TaskQueueTests
    {
        [Test]
        public void Constructor_SemaphoreCount_Default()
        {
            var queue = new TaskQueue();

            Assert.That(queue.SemaphoreCount, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_SemaphoreCount(
            [Random(0, 100, 2)] int semaphoreCount)
        {
            var queue = new TaskQueue(semaphoreCount);

            Assert.That(queue.SemaphoreCount, Is.EqualTo(semaphoreCount));
        }

        [Test]
        public async Task Enqueue()
        {
            var queue = new TaskQueue();
            bool isCalled = false;

            var result = await queue.Enqueue(() =>
            {
                isCalled = true;
                return Task.FromResult(42);
            });

            Assert.That(isCalled, Is.True);
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public async Task Enqueue_Tasks()
        {
            var queue = new TaskQueue();
            var tasksOrder = new List<int>();
            var tasks = new List<Task>();

            for (int i = 0; i < 3; i++)
            {
                int taskId = i;
                tasks.Add(queue.Enqueue(async () =>
                {
                    await Task.Delay(100);
                    tasksOrder.Add(taskId);
                }));
            }

            await Task.WhenAll(tasks);

            var expectedOrder = new List<int> { 0, 1, 2 };
            Assert.That(tasksOrder, Is.EqualTo(expectedOrder).AsCollection);
        }

        [Test]
        public void Enqueue_TaskWithException()
        {
            var queue = new TaskQueue();
            var expectedException = new Exception("Test exception");

            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await queue.Enqueue<int>(() => throw expectedException));

            Assert.That(exception.Message, Is.EqualTo(expectedException.Message));
        }

        [Test]
        public async Task Enqueue_ContinueNextTask_After_TaskWithException()
        {
            var queue = new TaskQueue(1);
            bool secondTaskCalled = false;

            try
            {
                await queue.Enqueue(() => Task.FromException(new Exception("Test exception")));
            }
            catch
            {
                // Skip exception from first task
            }

            await queue.Enqueue(() =>
            {
                secondTaskCalled = true;
                return Task.CompletedTask;
            });

            Assert.That(secondTaskCalled, Is.True);
        }

        [Test]
        public async Task Enqueue_Parallel_Tasks(
            [Random(1, 10, 2)] int maxParallelTasks,
            [Random(1, 50, 2)] int totalTasks)
        {
            var queue = new TaskQueue(maxParallelTasks);
            int currentParaellelTasks = 0;
            int currentMaximum = 0;
            var sync = new object();

            var tasks = Enumerable.Range(0, totalTasks).Select(async i =>
            {
                await queue.Enqueue(async () =>
                {
                    lock (sync)
                    {
                        currentParaellelTasks++;
                        currentMaximum = Math.Max(currentMaximum, currentParaellelTasks);
                    }

                    await Task.Delay(50);

                    lock (sync)
                    {
                        currentParaellelTasks--;
                    }
                });
            }).ToArray();

            await Task.WhenAll(tasks);

            Assert.That(currentMaximum, Is.LessThanOrEqualTo(maxParallelTasks));
        }
    }
}