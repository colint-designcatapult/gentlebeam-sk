using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Logging;
using Xcc.Core.Enums;

namespace Xcc.Infra.QualityCheck
{
    /// <summary>
    /// Mock implementation of IQcbService for testing without physical QCB hardware.
    /// Returns synthetic quality check data as placeholder until new I2C-based QC collimator integration is complete.
    /// </summary>
    public class MockQcbService : IQcbService
    {
        private readonly ILogWriter _logWriter;
        private bool _isStarted = false;

        public MockQcbService(ILogWriter logWriter)
        {
            _logWriter = logWriter;
        }

        public void Start()
        {
            // Mock: board is always "connected"
            _logWriter.LogAsync("MockQcbService: Start - Mock QCB service initialized", LogRecordSeverity.Info, LogRecordType.System).Wait();
        }

        public Task<bool> PingBoardAsync()
        {
            // Mock: always returns true (board is connected)
            return Task.FromResult(true);
        }

        public Task<QcbCommandResponseStatus> StartQCReadingsAsync(int numberOfDiodes, int samplingIntervalMs = 50)
        {
            // Mock: always succeeds
            _isStarted = true;
            return Task.FromResult(QcbCommandResponseStatus.StartConfirmed);
        }

        public Task<QcReadings?> StopQCReadingsAsync(int numberOfDiodes)
        {
            // Mock: return synthetic data - 5 equal diodes at 1.0f intensity
            if (!_isStarted)
            {
                return Task.FromResult<QcReadings?>(null);
            }

            _isStarted = false;
            var mockData = new float[numberOfDiodes];
            for (int i = 0; i < numberOfDiodes; i++)
            {
                mockData[i] = 1.0f; // Baseline mock intensity
            }

            return Task.FromResult<QcReadings?>(new QcReadings(mockData));
        }

        public void Dispose()
        {
            // Mock: no resources to dispose
        }
    }
}
