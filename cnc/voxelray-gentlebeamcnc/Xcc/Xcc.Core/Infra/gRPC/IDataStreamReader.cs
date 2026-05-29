using System;
using System.Threading.Tasks;

namespace Xcc.Core.Infra.gRPC
{
    public interface IDataStreamReader<TData> : IDisposable
    {
        Task<TData> ReceiveAsync();
    }
}
