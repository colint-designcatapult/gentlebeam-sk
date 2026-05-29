using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.AppLayer.Warmup
{
    public interface IWarmupService
    {
        Task UpdateWarmupHistoryAsync();
        Task RunSafeWarmupAsync(WarmupParameters warmupParameters);
    }
}
