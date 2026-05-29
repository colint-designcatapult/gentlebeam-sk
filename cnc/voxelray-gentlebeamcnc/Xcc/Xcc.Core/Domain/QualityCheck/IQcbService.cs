using System;
using System.Threading.Tasks;

namespace Xcc.Core.Domain.QualityCheck
{
    public interface IQcbService : IDisposable
    {
        void Start();
        Task<bool> PingBoardAsync();
        Task<QcbCommandResponseStatus> StartQCReadingsAsync(int numberOfDiodes, int samplingIntervalMs = 50);
        Task<QcReadings?> StopQCReadingsAsync(int numberOfDiodes);
    }

    public enum QcbCommandResponseStatus
    {
        NoResponse = 1,
        StartConfirmed,
        StartRejected
    }

}
