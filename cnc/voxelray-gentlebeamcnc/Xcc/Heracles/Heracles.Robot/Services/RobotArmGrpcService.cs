using Grpc.Net.Client;

using Heracles.Application.Protos;
using Heracles.Core.Models;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;

using System;
using System.Threading;
using System.Threading.Tasks;

using Xcc.Application.Models.RobotArm;
using Xcc.Application.Models.RobotArm.Enums;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Core.Logging;

namespace Heracles.Robot.Services
{
    public class RobotArmGrpcService : IRobotArmService
    {
        public event EventHandler<PingActionResponse> PingActionFeedback;
        public event EventHandler<MotionActionResponse> MotionActionFeedback;
        public event EventHandler<SetOperatingModeActionResponse> SetOperatingModeActionFeedback;
        public event EventHandler<Status> StatusFeedback;

        private const int GrpcResponseMillisecondsTimeout = 5000;
        private const int StatusChangeMillisecondsTimeout = 10000;
        ILogWriter _logWriter;
        IHeraclesMainSettings _heraclesMainSettings;
        GrpcChannel _channel;
        Status _status = Status.Unspecified;
        Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveCommandsService.MoveCommandsServiceClient _client;
        CancellationTokenSource _cancelListenStatus = new ();

        private CartesianAngularPosition _cartesianAngularPosition = new();

        public CartesianPosition PositionMm 
        { 
            get {
                return CartesianAngularPosition.CartesianPositionMM; 
            }
        }
        public AngularPosition PositionDeg {
            get
            {
                return CartesianAngularPosition.AngularPositionDeg;
            }
        }
        public CartesianAngularPosition CartesianAngularPosition
        {
            get
            {
                getPosition(out _cartesianAngularPosition);
                return _cartesianAngularPosition;
            }
        }

        public JointsPosition JointsPositionDeg
        { 
            get
            {
                return getJointsPositionDeg();
            }
        }

        public Status Status
        {
            get => _status;
        }

        public RobotArmGrpcService(ILogWriter logWriter, IHeraclesMainSettings heraclesMainSettings)
        { 
            _logWriter = logWriter;
            _heraclesMainSettings = heraclesMainSettings;

            _channel = GrpcChannel.ForAddress(_heraclesMainSettings.RobotGrpcServerUri);
            _client = new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveCommandsService.MoveCommandsServiceClient(_channel);


            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: grpc connection state = {_channel.State.ToString()} for address= {_heraclesMainSettings.RobotGrpcServerUri}", LogRecordSeverity.Info, LogRecordType.System);
            // todo use internal mechanism
            //if (Ping(1) == true)
            //{
            //    _logService.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: grpc connection successfully established for address = {_appSettings.RobotGrpcServerUri}", LogRecordSeverity.Info, LogRecordType.System);
            //}
            //else
            //{
            //    _logService.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: grpc connection ping failed for address = {_appSettings.RobotGrpcServerUri}", LogRecordSeverity.Warn, LogRecordType.Error);
            //}

            Task.Run(() => listenStatus(_cancelListenStatus.Token));//, _cancelListenStatus.Token);
        }

        bool moveNextWithTimeout(Task<bool> task) 
        { 
            bool completed = task.Wait(GrpcResponseMillisecondsTimeout);
            if (!completed)
            {
                throw new Exception("Timeout");
            }
            return completed ? task.Result : false; 
        }

        public void Dispose()
        {
            _cancelListenStatus.Cancel();
            if (_channel != null)
            {
                _channel.Dispose();
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: grpc connection closed for address = {_heraclesMainSettings.RobotGrpcServerUri}", LogRecordSeverity.Info, LogRecordType.System);
            }
        }

        private void updateStatus(Status status)
        {
            _status = status;
            StatusFeedback?.Invoke(this, status);
        }

        private async void listenStatus(CancellationToken cancellationToken)
        {
            bool reportException = true;

            for (; ; )
            {
                try
                {
                    using (var feedback = _client.GetKeepAlive(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.GetKeepAliveRequest()))
                    {
                        while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                        {
                            var resp = feedback.ResponseStream.Current;
                            if (resp != null)
                            {
                                updateStatus(ProtoRobotTypesConverter.FromProto(resp.Status));
                            }

                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            reportException = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    updateStatus(Status.RosClientFailure);
                    if (reportException)
                    {
                        _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception = {e.Message}", LogRecordSeverity.Error, LogRecordType.System);
                        reportException = false;
                    }

                    await Task.Delay(1000);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Stop KeepAlive thread", LogRecordSeverity.Info, LogRecordType.System);
                    return;
                }
            }
        }

        public bool MoveCustomAction(JointsPosition jointsPosition)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, jointsPosition = {jointsPosition.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool res = false;
            try
            {
                var req = new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveCustomActionRequest();
                foreach (var j in jointsPosition.JArray)
                {
                    req.ValuesRad.Add(MathHelpers.ConvertDegreesToRadians(j));
                }
                // TODO: check safe range
                using (var feedback = _client.MoveCustomAction(req))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            MotionActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                            if (resp.Tag == Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result)
                            {
                                res = resp.ResultSuccess;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, jointsPosition = {jointsPosition.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return res;
        }
        public bool MoveToPositionAction(CartesianAngularPosition position)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, position = {position.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool res = false;
            try
            {
                var req = new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveToPositionActionRequest()
                {
                    WorldPositionMm = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(position.CartesianPositionMM),
                    WorldPositionDeg = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(position.AngularPositionDeg),
                };
                using (var feedback = _client.MoveToPositionAction(req))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            MotionActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                            if (resp.Tag == Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result)
                            {
                                res = resp.ResultSuccess;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, position = {position.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return res;
        }
        public bool MoveByMatrixAction(MovementMatrix matrix)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, matrix = {matrix.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool res = false;
            try
            {
                using (var feedback = _client.MoveByMatrixAction(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveByMatrixActionRequest { Matrix4X4 = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(matrix) }))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            MotionActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                            if (resp.Tag == Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result)
                            {
                                res = resp.ResultSuccess;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, matrix = {matrix.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return res;
        }

        public bool Ping(int pongs_amount)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, pongs_amount = {pongs_amount.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool result = false;
            try
            {
                using (var feedback = _client.PingAction(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.PingActionRequest { PongsAmount = pongs_amount }))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            PingActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                        }
                        result = true;
                    }
                }

            }
            catch (Exception ex)
            {
                PingActionFeedback?.Invoke(this, new PingActionResponse { Tag = ActionResponseTag.Goal, GoalAccepted = false});
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, pongs_amount = {pongs_amount.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }

            return result;
        }

        public bool? IsFakeHardware()
        {
            bool? result = null;
            try
            {
                var response = _client.IsFakeHardware(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.IsFakeHardwareRequest { });
                return (response.HasIsFakeHardware) ? response.IsFakeHardware : null;
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return result;
        }

        public bool RotateAction(Axis axis, float angleDeg, CoordinateSystem coordinateSystem)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, axis = {axis.ToString()}, angleDeg = {angleDeg.ToString()}, coordinateSystem = {coordinateSystem.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool res = false;
            try
            {
                using (var feedback = _client.RotateRelativeAction(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.RotateRelativeActionRequest { Axis = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(axis), Deg = angleDeg }))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            MotionActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                            if (resp.Tag == Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result)
                            {
                                res = resp.ResultSuccess;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, axis = {axis.ToString()}, angleDeg = {angleDeg.ToString()}, coordinateSystem = {coordinateSystem.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return res;
        }

        public bool Stop()
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start", LogRecordSeverity.Info, LogRecordType.System);
            try
            {
                var response = _client.StopMotion(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.StopMotionRequest { });
                return response.Accepted;
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }

        public bool TranslateAction(Axis axis, float distMm, CoordinateSystem coordinateSystem)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, axis = {axis.ToString()}, distMm = {distMm.ToString()}, coordinateSystem = {coordinateSystem.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool res = false;
            try
            {
                using (var feedback = _client.TranslateRelativeAction(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.TranslateRelativeActionRequest { Axis = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(axis), Mm = distMm }))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            MotionActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                            if (resp.Tag == Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result)
                            {
                                res = resp.ResultSuccess;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, axis = {axis.ToString()}, distMm = {distMm.ToString()}, coordinateSystem = {coordinateSystem.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return res;
        }
        public bool SetOperatingMode(OperatingMode operatingMode)
        {
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, operatingMode = {operatingMode.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            bool res = false;
            try
            {
                using (var feedback = _client.SetOperatingModeAction(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.SetOperatingModeActionRequest { OperatingMode = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(operatingMode) }))
                {
                    var cancellationToken = new CancellationToken();
                    while (moveNextWithTimeout(feedback.ResponseStream.MoveNext(cancellationToken)))
                    {
                        var resp = feedback.ResponseStream.Current;
                        if (resp != null)
                        {
                            SetOperatingModeActionFeedback?.Invoke(this, Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(resp));
                            if (resp.Tag == Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result)
                            {
                                res = resp.ResultSuccess;
                            }
                        }
                    }
                }
                if (res)
                {
                    if ((_status == Status.Activated) && (operatingMode == OperatingMode.LocalControl))
                    {
                        res = waitForStatus(Status.Deactivated, StatusChangeMillisecondsTimeout);
                    }
                    else if ((_status == Status.Deactivated) && (operatingMode == OperatingMode.RemoteControl))
                    {
                        res = waitForStatus(Status.Activated, StatusChangeMillisecondsTimeout);
                    }
                }
            }
            catch (Exception ex)
            {
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}, operatingMode = {operatingMode.ToString()}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return res;
        }

        private void getPosition(out CartesianAngularPosition position)
        {
            position = new();
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start", LogRecordSeverity.Info, LogRecordType.System);
            try
            {
                var response = _client.GetPosition(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.GetPositionRequest { }, deadline: DateTime.UtcNow.AddMilliseconds(GrpcResponseMillisecondsTimeout));
                position.CartesianPositionMM = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.FeedbackPositionMm);
                position.AngularPositionDeg = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.FeedbackPositionDeg);
            }
            catch (Exception ex)
            {
                position.CartesianPositionMM = null;
                position.AngularPositionDeg = null;
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
        }

        private JointsPosition getJointsPositionDeg()
        {
            JointsPosition jointsPosition = null;
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start", LogRecordSeverity.Info, LogRecordType.System);
            try
            {
                var response = _client.GetJointsPosition(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.GetJointsPositionRequest { }, deadline: DateTime.UtcNow.AddMilliseconds(GrpcResponseMillisecondsTimeout));
                jointsPosition = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.FeedbackJointsPositionsRad);
            }
            catch (Exception ex)
            {
                jointsPosition = null;
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }

            return jointsPosition;
        }
        private bool waitForStatus(Status status, int timeoutMs)
        {
            const int delayMs = 40;
            int k = timeoutMs / delayMs + 1;
            for (int i = 0; i < k; ++i)
            {
                if (_status == status)
                {
                    return true;
                }
                Thread.Sleep(delayMs);
            }
            return false;
        }
        public CartesianAngularPosition ConvertTranslateRelativeToPosition(Axis axis, float dist_mm, CoordinateSystem coordinateSystem)
        {
            CartesianAngularPosition position = new();
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, axis = {axis.ToString()}, dist_mm = {dist_mm.ToString()}, coordinateSystem = {coordinateSystem.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            try
            {
                var response = _client.ConvertTranslateRelativeToPosition(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.ConvertTranslateRelativeToPositionRequest { Axis = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(axis), DistanceMm = dist_mm, CoordinateSystem = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(coordinateSystem) }, deadline: DateTime.UtcNow.AddMilliseconds(GrpcResponseMillisecondsTimeout));
                position.CartesianPositionMM = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.WorldPositionMm);
                position.AngularPositionDeg = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.WorldPositionDeg);
            }
            catch (Exception ex)
            {
                position = null;
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return position;
        }
        public CartesianAngularPosition ConvertRotateRelativeToPosition(Axis axis, float angleDeg, CoordinateSystem coordinateSystem)
        {
            CartesianAngularPosition position = new();
            _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Start, axis = {axis.ToString()}, angleDeg = {angleDeg.ToString()}, coordinateSystem = {coordinateSystem.ToString()}", LogRecordSeverity.Info, LogRecordType.System);
            try
            {
                var response = _client.ConvertRotateRelativeToPosition(new Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.ConvertRotateRelativeToPositionRequest { Axis = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(axis), AngleDeg = angleDeg, CoordinateSystem = Heracles.Application.Protos.ProtoRobotTypesConverter.ToProto(coordinateSystem) }, deadline: DateTime.UtcNow.AddMilliseconds(GrpcResponseMillisecondsTimeout));
                position.CartesianPositionMM = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.WorldPositionMm);
                position.AngularPositionDeg = Heracles.Application.Protos.ProtoRobotTypesConverter.FromProto(response.WorldPositionDeg);
            }
            catch (Exception ex)
            {
                position = null;
                _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return position;
        }
    }
}
