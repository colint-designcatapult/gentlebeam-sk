using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.QualityCheck
{
    /// <summary>
    /// Mock implementation of IQcbReadingModel for testing without physical QCB hardware.
    /// Delegates to MockQcbService for synthetic data generation.
    /// </summary>
    public class MockQcbReadingModel : IQcbReadingModel
    {
        private readonly IQcbService _qcbService;
        private readonly ILogWriter _logWriter;

        public MockQcbReadingModel(IQcbService qcbService, ILogWriter logWriter)
        {
            _qcbService = qcbService;
            _logWriter = logWriter;
        }

        /// <summary>
        /// Mock: Checks if QCB board is "connected" (always true for mock)
        /// </summary>
        public Task<bool> PingBoardAsync()
        {
            return _qcbService.PingBoardAsync();
        }

        /// <summary>
        /// Mock: Reads synthetic QC intensity data.
        /// Returns mock readings with equal intensity across all diodes.
        /// </summary>
        public async Task<QcReadings> ReadQCAsync(int numberOfDiodes, CancellationToken token, int samplingWindowMs = 0)
        {
            // Start mock reading
            var startStatus = await _qcbService.StartQCReadingsAsync(numberOfDiodes, samplingWindowMs);
            
            if (startStatus != QcbCommandResponseStatus.StartConfirmed)
            {
                throw new Exception("MockQcbReadingModel: Failed to start QC readings");
            }

            // Simulate sampling delay
            if (samplingWindowMs > 0)
            {
                await Task.Delay(samplingWindowMs, token);
            }

            // Stop reading and get synthetic data
            var readings = await _qcbService.StopQCReadingsAsync(numberOfDiodes);

            if (readings == null)
            {
                throw new Exception("MockQcbReadingModel: No readings data returned");
            }

            return readings;
        }
    }
}
