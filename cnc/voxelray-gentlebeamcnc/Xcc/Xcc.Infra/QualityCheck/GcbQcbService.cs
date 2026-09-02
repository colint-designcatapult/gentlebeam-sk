using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;

namespace Xcc.Infra.QualityCheck
{
    public sealed class GcbQcbService(IGcbCommandInterface gcbCommands) : IQcbService
    {
        private const int FirmwareDiodeCount = 5;

        public void Start()
        {
        }

        public Task<bool> PingBoardAsync() => gcbCommands.PingQcb();

        public async Task<QcbCommandResponseStatus> StartQCReadingsAsync(
            int numberOfDiodes,
            int samplingIntervalMs = 50)
        {
            ValidateDiodeCount(numberOfDiodes);
            await gcbCommands.StartQcbReadings(samplingIntervalMs);
            return QcbCommandResponseStatus.StartConfirmed;
        }

        public async Task<QcReadings?> StopQCReadingsAsync(int numberOfDiodes)
        {
            ValidateDiodeCount(numberOfDiodes);
            return await gcbCommands.StopQcbReadings();
        }

        public void Dispose()
        {
        }

        private static void ValidateDiodeCount(int numberOfDiodes)
        {
            if (numberOfDiodes != FirmwareDiodeCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberOfDiodes),
                    numberOfDiodes,
                    $"Main-control returns exactly {FirmwareDiodeCount} QC channels.");
            }
        }
    }
}
