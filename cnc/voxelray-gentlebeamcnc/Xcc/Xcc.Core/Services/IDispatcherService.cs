using System;
using System.Threading.Tasks;

namespace Xcc.Core.Services
{
    public interface IDispatcherService
    {
        void Invoke(Action action);
        T Invoke<T>(Func<T> func);
        Task InvokeAsync(Action action);
    }
}
