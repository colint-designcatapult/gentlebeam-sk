using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;

using System;

using Xcc.Application.Models.RobotArm;
using Xcc.Application.Models.RobotArm.Enums;

namespace Heracles.Robot.Services
{
    public interface IRobotArmService : IDisposable
    {
        public event EventHandler<PingActionResponse> PingActionFeedback;
        public event EventHandler<MotionActionResponse> MotionActionFeedback;
        public event EventHandler<SetOperatingModeActionResponse> SetOperatingModeActionFeedback;
        public event EventHandler<Status> StatusFeedback;

        public CartesianPosition PositionMm { get; }
        public AngularPosition PositionDeg { get; }
        public CartesianAngularPosition CartesianAngularPosition { get; }
        public JointsPosition JointsPositionDeg { get; }
        public Status Status { get; }

        public bool Stop();
        public bool Ping(int pongs_amount);
        public bool? IsFakeHardware();
        public bool TranslateAction(Axis axis, float mm, CoordinateSystem coordinateSystem);
        public bool RotateAction(Axis axis, float deg, CoordinateSystem coordinateSystem);
        public bool MoveCustomAction(JointsPosition jointsPosition);
        public bool MoveToPositionAction(CartesianAngularPosition position);
        public bool MoveByMatrixAction(MovementMatrix matrix);
        public bool SetOperatingMode(OperatingMode operatingMode);
        public CartesianAngularPosition ConvertTranslateRelativeToPosition(Axis axis, float mm, CoordinateSystem coordinateSystem);
        public CartesianAngularPosition ConvertRotateRelativeToPosition(Axis axis, float mm, CoordinateSystem coordinateSystem);

        public new void Dispose();
    }
}
