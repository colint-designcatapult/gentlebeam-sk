using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;

namespace Xcc.Infra.QualityCheck
{
    public sealed class GcbQcbService(IGcbCommandInterface gcbCommands) : IQcbService
    {
        public void Start()
        {
        }

        public Task<bool> PingBoardAsync() => gcbCommands.PingQcb();

        public async Task<QcbCommandResponseStatus> StartQCReadingsAsync()
        {
            await gcbCommands.StartQcbReadings();
            return QcbCommandResponseStatus.StartConfirmed;
        }

        public Task<QcReadings> StopQCReadingsAsync() =>
            gcbCommands.StopQcbReadings();

        public void Dispose()
        {
        }
    }
}
