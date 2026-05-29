using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Core.Domain.QualityCheck
{
    public interface IQcbReadingModel
    {
        Task<bool> PingBoardAsync();
        Task<QcReadings> ReadQCAsync(int numberOfDiodes, CancellationToken token, int samplingWindowMs = 0);
    }
}
