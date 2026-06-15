//using Com.Empyreanmed.HeraclesRoboticArm.Axes.V1;

using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;

using System;

using Xcc.Application.Models.RobotArm;
using Xcc.Application.Models.RobotArm.Enums;

namespace Heracles.Robot.Services
{
    public class RobotArmMockService : IRobotArmService
    {
        public RobotArmMockService() { }

        public event EventHandler<PingActionResponse> PingActionFeedback;
        public event EventHandler<MotionActionResponse> MotionActionFeedback;
        public event EventHandler<SetOperatingModeActionResponse> SetOperatingModeActionFeedback;
        public event EventHandler<Status> StatusFeedback;

        public CartesianPosition PositionMm { get => new () {X = 1.0f, Y = 1000.0f, Z = 1000.0f }; }
        public AngularPosition PositionDeg { get => new () { Rx = 4.0f, Ry = 5.0f, Rz = 6.0f }; }

        public CartesianAngularPosition CartesianAngularPosition { get => new() {CartesianPositionMM = PositionMm, AngularPositionDeg = PositionDeg }; }

        public JointsPosition JointsPositionDeg { get => new JointsPosition(); }

        public Status Status { get => Status.Activated; }

        public void Dispose()
        {
        }

        public bool MoveCustomAction(JointsPosition jointsPosition)
        {
            return true;
        }

        public bool MoveToPositionAction(CartesianAngularPosition position)
        {
            return true;
        }

        public bool MoveByMatrixAction(MovementMatrix matrix)
        {
            return true;
        }

        public bool Ping(int pongs_amount)
        {
            return true;
        }

        public bool? IsFakeHardware()
        {
            return true;
        }

        public bool RotateAction(Axis axis, float deg, CoordinateSystem coordinateSystem)
        {
            return true;
        }

        public bool SetOperatingMode(OperatingMode operatingMode)
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public bool TranslateAction(Axis axis, float mm, CoordinateSystem coordinateSystem)
        {
            return true;
        }

        public CartesianAngularPosition ConvertTranslateRelativeToPosition(Axis axis, float mm, CoordinateSystem coordinateSystem)
        {
            return new CartesianAngularPosition() { 
                CartesianPositionMM =  new () { X = 1.0f, Y = 1000.0f, Z = 1000.0f }, 
                AngularPositionDeg = new () { Rx = 4.0f, Ry = 5.0f, Rz = 6.0f }
            };
        }
        public CartesianAngularPosition ConvertRotateRelativeToPosition(Axis axis, float mm, CoordinateSystem coordinateSystem)
        {
            return new CartesianAngularPosition()
            {
                CartesianPositionMM = new() { X = 2.0f, Y = 2000.0f, Z = 2000.0f },
                AngularPositionDeg = new() { Rx = 40.0f, Ry = 50.0f, Rz = 60.0f }
            };
        }
    }
}
