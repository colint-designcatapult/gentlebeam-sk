using System;
using System.Threading.Tasks;

namespace Xcc.Core.Domain.QualityCheck
{
    public interface IQcbService : IDisposable
    {
        void Start();
        Task<bool> PingBoardAsync();
        Task<QcbCommandResponseStatus> StartQCReadingsAsync();
        Task<QcReadings> StopQCReadingsAsync();
    }

    public enum QcbCommandResponseStatus
    {
        NoResponse = 1,
        StartConfirmed,
        StartRejected
    }

}
