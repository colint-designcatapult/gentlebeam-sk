using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Core.Infra.DataManagement.Common.DataAccess
{
    public interface IEventStream<StreamArgsType>
    {
        Task RunStreamAsync(Action<StreamArgsType> streamCallback, CancellationToken cancellationToken);
    }
}