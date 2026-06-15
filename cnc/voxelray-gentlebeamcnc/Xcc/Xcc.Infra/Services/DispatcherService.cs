using System;
using System.Threading.Tasks;
using Xcc.Core.Services;

namespace Xcc.Infra.Services
{
    public class DispatcherService : IDispatcherService
    {
        public async Task InvokeAsync(Action action)
        {
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
        }

        public void Invoke(Action action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(action);
        }

        public T Invoke<T>(Func<T> func)
        {
            return System.Windows.Application.Current.Dispatcher.Invoke<T>(func);
        }
    }
}
