using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.QualityCheck
{
    public class QcbReadingModel : IQcbReadingModel
    {
        private readonly int WATCHDOG_REQUEST_INTERVAL_MS = 250;
        private readonly int WATCHDOG_EXPIRATION_TIMEOUT_MS = 1000;
        private readonly ILogWriter _logWriter;
        private readonly IQcbService _qcbService;
        private readonly GcbReachedStateFlags _reachedStates;
        private readonly QcbWatchdog _qcbWatchdog;

        public QcbReadingModel(
            ILogWriter logWriter,
            IQcbService qcbService,
            IEventAggregator eventAggregator)
        {
            _logWriter = logWriter;
            _qcbService = qcbService;
            _reachedStates = new(eventAggregator);
            _qcbWatchdog = new(qcbService);
        }

        /// <summary>
        /// Sends status query to the board and expects it to respond
        /// </summary>
        /// <returns></returns>
        public Task<bool> PingBoardAsync()
        {
            return _qcbService.PingBoardAsync();
        }

        /// <summary>
        /// Does the main job of reading the intensities from QCBoard.
        /// Should be called from Ready state before/parallel to BeamOn initiation
        /// </summary>
        /// <param name="numberOfDiodes"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="samplingWindowMs"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        public Task<QcReadings> ReadQCAsync(int numberOfDiodes, CancellationToken cancellationToken, int samplingWindowMs = 0)
        {
            //isInEmissionState = false;
            ICollection<GcbStateNew?> unwantedStates =
            [
                GcbStateNew.Staged,
                GcbStateNew.Fault
            ];

            return Task.Run(async () =>
            {
                _reachedStates.ResetAllFlags();
                bool gotIntoEmissionState = false;

                // First we send start command to the board to make it collecting the data
                bool started = await StartQcReadings(numberOfDiodes, cancellationToken, samplingWindowMs);
                if (started == false)
                {
                    Debug.WriteLine("QcbReadingModel: QCBoard start command failed");
                    throw new Exception("QcbReadingModel: QCBoard start command failed");
                }

                try
                {
                    // Now we need to check if we reach the emission state and don't reach any fault or staged first
                    // We supposed to start this from Ready
                    // Wait when we reach emission state
                    while (!gotIntoEmissionState)
                    {
                        await Task.Delay(50, cancellationToken);
                        gotIntoEmissionState = _reachedStates.CheckFlag(GcbStateNew.Emission);
                        if (_reachedStates.CheckFlags(unwantedStates))
                        {
                            Debug.WriteLine("QcbReadingModel: unexpected GCB state, stop reading task");
                            throw new Exception("QcbReadingModel: unexpected GCB state, stop reading task");
                        }

                        if (_qcbWatchdog.IsAlive == false)
                        {
                            Debug.WriteLine("QcbReadingModel: connection to QCB board is lost");
                            throw new Exception("QcbReadingModel: connection to QCB board is lost");
                        }
                    }

                    // Now we want to wait until the state will change from the emission to something else
                    while (_reachedStates.LastState() == GcbStateNew.Emission)
                    {
                        await Task.Delay(50, cancellationToken);
                        if (_reachedStates.CheckFlag(GcbStateNew.Fault))
                        {
                            Debug.WriteLine("QcbReadingModel: unexpected GCB state, stop reading task");
                            throw new Exception("QcbReadingModel: unexpected GCB state, stop reading task");
                        }

                        if (_qcbWatchdog.IsAlive == false)
                        {
                            Debug.WriteLine("QcbReadingModel: connection to the QC board is lost, stop reading task");
                            throw new Exception("QcbReadingModel: connection to the QC board is lost, stop reading task");
                        }
                    }
                    // Now we've done with the state observing part of the task,
                    // and can stop worrying about terminating the reading command
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"QcbReadingModel: task was interrupted with exception {ex.Message}");
                    try
                    {
                        // If we get our task cancelled or failed before the readings were done,
                        // we need to at least send the board Stop command:
                        _ = StopQcReadings(numberOfDiodes);
                    }
                    catch (Exception stopReadingException)
                    {
                        Debug.WriteLine($"QcbReadingModel: stop readings failed with exception {ex.Message}");
                        throw new Exception(stopReadingException.Message, innerException: ex);
                    }

                    throw;
                }

                // If we weren't interrupted, then just return the readings and wipe the token source out
                if (!cancellationToken.IsCancellationRequested)
                {
                    // Here we can get to the actual readings
                    Debug.WriteLine("QcbReadingModel: stop readings and quit");
                    var result = await StopQcReadings(numberOfDiodes);
                    Debug.WriteLine($"QcbReadingModel: after stop, result is {result != null}, quit");
                    return result!;
                }
                else
                {
                    Debug.WriteLine("QcbReadingModel: task was cancelled, stop readings and throw");
                    _ = StopQcReadings(numberOfDiodes);
                    throw new TaskCanceledException("QcbReadingModel: intensity reading task was cancelled");
                }
            }, cancellationToken: cancellationToken);
        }

        private async Task<QcReadings> StopQcReadings(int numberOfDiodes, int attempts = 3)
        {
            _qcbWatchdog.StopWatch();

            Debug.WriteLine("QcbReadingModel: StopQcReadings - try stop and receive readings");

            var result = await _qcbService.StopQCReadingsAsync(numberOfDiodes);
            if (result != null)
            {
                return result;
            }

            Debug.WriteLine("QcbReadingModel: StopQcReadings - response timeout");

            throw new Exception("StopCommand: no response from QCB");
        }

        public async Task<bool> StartQcReadings(int numberOfDiodes, CancellationToken cancellationToken, int samplingWindowMs)
        {
            var status = await _qcbService.StartQCReadingsAsync(numberOfDiodes, samplingWindowMs);

            if (status != QcbCommandResponseStatus.StartConfirmed)
                _= _logWriter.LogAsync($"QcbReadingModel: StartQcReadings response status={status}", LogRecordSeverity.Error, LogRecordType.System);

            switch (status)
            {
                case QcbCommandResponseStatus.StartConfirmed:
                    _ = _qcbWatchdog.StartWatch(cancellationToken, WATCHDOG_REQUEST_INTERVAL_MS, WATCHDOG_EXPIRATION_TIMEOUT_MS);
                    return true;
                case QcbCommandResponseStatus.StartRejected: // board was not stopped yet, so it could not start
                    await _qcbService.StopQCReadingsAsync(numberOfDiodes);
                    return false;
                case QcbCommandResponseStatus.NoResponse:
                default:
                    return false;
            }
        }
    }
}
