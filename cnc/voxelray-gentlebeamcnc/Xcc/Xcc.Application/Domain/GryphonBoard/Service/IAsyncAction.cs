using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Application.Domain.GryphonBoard.Service
{
    public interface IAsyncAction
    {
        bool CanRun();
        Task RunAsync(CancellationToken token);
    }
}
