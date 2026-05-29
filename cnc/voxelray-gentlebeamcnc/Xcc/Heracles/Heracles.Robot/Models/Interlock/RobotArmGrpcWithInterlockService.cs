using Heracles.Core.Models;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Heracles.Robot.Services;

using Prism.Services.Dialogs;

using System;
using System.Threading;
using System.Threading.Tasks;

using Xcc.Application.Common;
using Xcc.Application.Models.RobotArm;
using Xcc.Application.Models.RobotArm.Enums;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Interlock
{
    enum RecoveryAction
    {
        Resume, // try to complite step
        Revert, // revert step
        Cancel  // abort step
    }

    public class RobotArmGrpcWithInterlockService : RobotArmGrpcService, IRobotArmService, IDisposable
    {
        private const int InterlockPollingIntervalMs = 50;
        private const int DelayAfterStopMs = 200;
        private const int InterlockTimeoutMs = 60000;

        public RobotArmGrpcWithInterlockService(ILogRepository logWriter, IHeraclesMainSettings heraclesMainSettings, IInterlockService interlockService_, DialogService dialogService)
            : base(logWriter, heraclesMainSettings)
        {
            _interlockService = interlockService_;
            _logWriter = logWriter;
            _dialogService = dialogService;
            var token = _cancellationTokenSource.Token;
            _monitorTask = Task.Run(async () => {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                        if (_isMoving && _interlockService.State == State.Deny)
                        {
                            Stop();
                            await Task.Delay(DelayAfterStopMs, token);
                        }
                        await Task.Delay(InterlockPollingIntervalMs, token);
                    }
                }
                catch (Exception )
                {
                }
            }, token);
        }

        public new bool MoveCustomAction(JointsPosition jointsPosition) 
        {
            if (!tryToActivate())
            {
                _logWriter.LogAsync("RobotArmGrpcWithInterlockService.MoveCustomAction: robot is not Activated", LogRecordSeverity.Warn, LogRecordType.System);
                return false;
            }
            // The robot must be activated to obtain the correct position.
            var rollbackPosition = JointsPositionDeg;
            var targetPosition = jointsPosition;
            return MoveAction(() => { return base.MoveCustomAction(targetPosition); }, () => { return base.MoveCustomAction(rollbackPosition); });
        }
        public new bool MoveByMatrixAction(MovementMatrix matrix)
        {
            bool res = false;
            if (_interlockService.State == State.Allow)
            {
                _isMoving = true;
                res = base.MoveByMatrixAction(matrix);
                _isMoving = false;
            }
            return res;
        }

        public new bool RotateAction(Axis axis, float angleDeg, CoordinateSystem frame)
        {
            if (!tryToActivate())
            {
                _logWriter.LogAsync("RobotArmGrpcWithInterlockService.RotateAction: robot is not Activated", LogRecordSeverity.Warn, LogRecordType.System);
                return false;
            }
            // The robot must be activated to obtain the correct position.
            var rollbackPosition = this.CartesianAngularPosition;
            var targetPosition = ConvertRotateRelativeToPosition(axis, angleDeg, frame); 
            return MoveAction(() => { return base.MoveToPositionAction(targetPosition); }, () => { return base.MoveToPositionAction(rollbackPosition); });
        }

        public new bool TranslateAction(Axis axis, float distMm, CoordinateSystem frame)
        {
            if (!tryToActivate())
            {
                _logWriter.LogAsync("RobotArmGrpcWithInterlockService.TranslateAction: robot is not Activated", LogRecordSeverity.Warn, LogRecordType.System);
                return false;
            }
            // The robot must be activated to obtain the correct position.
            var rollbackPosition = this.CartesianAngularPosition;
            var targetPosition = ConvertTranslateRelativeToPosition(axis, distMm, frame);
            return MoveAction(() => { return base.MoveToPositionAction(targetPosition); }, () => { return base.MoveToPositionAction(rollbackPosition); });
        }

        public new void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _monitorTask.Wait();
            _monitorTask.Dispose();
            _cancellationTokenSource.Dispose();
            base.Dispose();
        }

        private bool MoveAction(Func<bool> moveTarget, Func<bool> moveRollback)
        {
            RecoveryAction recoveryAction = RecoveryAction.Resume;

            bool res = false;
            var timeout = DateTime.Now.AddMilliseconds(InterlockTimeoutMs);
            while (res != true)
            {
                if (DateTime.Now > timeout)
                {
                    _logWriter.LogAsync("RobotArmGrpcWithInterlockService.MoveCustomAction: timeout", LogRecordSeverity.Warn, LogRecordType.System);
                    break;
                }

                if (!tryToActivate())
                {
                    _logWriter.LogAsync("RobotArmGrpcWithInterlockService.MoveCustomAction: robot is not Activated", LogRecordSeverity.Warn, LogRecordType.System);
                    break;
                }

                if (_interlockService.State == State.Allow)
                {
                    _isMoving = true;
                    if (recoveryAction == RecoveryAction.Resume)
                    {
                        res = moveTarget();
                    }
                    else if (recoveryAction == RecoveryAction.Revert)
                    {
                        res = moveRollback();
                    }
                    _isMoving = false;

                    if (res == true && recoveryAction == RecoveryAction.Revert)
                    {
                        // if rollback done - step result must be false (we do not complete the step)
                        res = false;
                        break;
                    }

                    if (res == false)
                    {
                        recoveryAction = askUserAction();
                        if (recoveryAction == RecoveryAction.Cancel)
                        {
                            break;
                        }
                    }
                        
                    // extend timeout if activity occurs
                    timeout = DateTime.Now.AddMilliseconds(InterlockTimeoutMs);
                }
                Thread.Sleep(InterlockPollingIntervalMs);
            }
            return res;
        }

        private RecoveryAction askUserAction()
        {
            RecoveryAction recoveryAction = RecoveryAction.Cancel;
            bool resume = false;
            string caption = "Confirm Robotic Arm Action";
            string message = "Click Resume to resume movement, Revert to rollback movement and Cancel for stop";
            _dialogService.Report(
                caption,
                message,
                ReportType.ConfirmationResumeRevertCancel,
                result =>
                {
                    recoveryAction = result.Result switch
                    {
                        ButtonResult.OK => RecoveryAction.Resume,
                        ButtonResult.Abort => RecoveryAction.Revert,
                        _ => RecoveryAction.Cancel
                    };

                    resume = result.Result == ButtonResult.OK;
                });
            return recoveryAction;

        }
        private bool tryToActivate()
        {
            bool activated = (base.Status == Status.Activated) || base.SetOperatingMode(OperatingMode.RemoteControl);
            if (!activated)
            {
                string caption = "Error";
                string message = "Robot failed to gain SPOC - contact support ";
                _dialogService.Report(
                caption,
                message,
                ReportType.Error,
                result => {});
            }
            return activated;
        }

        private IInterlockService _interlockService = null;
        private ILogRepository _logWriter = null;
        private DialogService _dialogService = null;
        private bool _isMoving = false;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task _monitorTask = null;
    }
}
