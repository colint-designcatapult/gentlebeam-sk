using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Logging;
using Xcc.Core.Enums;

namespace Xcc.Infra.QualityCheck
{
    public class MockQcbService : IQcbService
    {
        private readonly ILogWriter _logWriter;
        private bool _isStarted;

        public MockQcbService(ILogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public void Start()
        {
            _logWriter.LogAsync(
                "MockQcbService: Start - Mock QCB service initialized",
                LogRecordSeverity.Info,
                LogRecordType.System).Wait();
        }

        public Task<bool> PingBoardAsync() => Task.FromResult(true);

        public Task<QcbCommandResponseStatus> StartQCReadingsAsync()
        {
            _isStarted = true;
            return Task.FromResult(QcbCommandResponseStatus.StartConfirmed);
        }

        public Task<QcReadings> StopQCReadingsAsync()
        {
            if (!_isStarted)
            {
                throw new InvalidOperationException("QC acquisition has not been started.");
            }

            _isStarted = false;
            return Task.FromResult(new QcReadings(1u, 1u, 1u, 1u));
        }

        public void Dispose()
        {
        }
    }
}
