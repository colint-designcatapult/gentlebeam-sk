using System;
using System.Threading.Tasks;
using Xcc.Core.Services;

namespace Xcc.Infra.Services
{
    public class TestDispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            action.Invoke();
        }

        public T Invoke<T>(Func<T> func)
        {
            return func.Invoke();
        }

        public Task InvokeAsync(Action action)
        {
            action.Invoke();
            return Task.CompletedTask;
        }
    }
}
