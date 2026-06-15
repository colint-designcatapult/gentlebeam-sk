using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;

using System;

using Xcc.Application.Models.RobotArm;
using Xcc.Application.Models.RobotArm.Enums;
using Xcc.Core.Helpers;

namespace Heracles.Application.Protos
{
    public class ProtoRobotTypesConverter : Xcc.Infra.Persistence.DataAccess.gRPC.ProtoTypesConverter
    {
        #region enums

        public static ActionResponseTag FromProto(Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag tag)
        {
            switch (tag)
            {
                case Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.GoalUnspecified:
                    return ActionResponseTag.Goal;
                case Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Feedback:
                    return ActionResponseTag.Feedback;
                case Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1.ActionResponseTag.Result:
                    return ActionResponseTag.Result;
                default:
                    throw new InvalidCastException("Unknown argument: " + tag.ToString());
            }
        }

        public static Com.Empyreanmed.HeraclesRoboticArm.Axes.V1.Axis ToProto(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Com.Empyreanmed.HeraclesRoboticArm.Axes.V1.Axis.XUnspecified;
                case Axis.Y:
                    return Com.Empyreanmed.HeraclesRoboticArm.Axes.V1.Axis.Y;
                case Axis.Z:
                    return Com.Empyreanmed.HeraclesRoboticArm.Axes.V1.Axis.Z;
                default:
                    throw new InvalidCastException("Unknown argument: " + axis.ToString());
            }
        }

        public static Com.Empyreanmed.HeraclesRoboticArm.CoordinateSystems.V1.CoordinateSystem ToProto(CoordinateSystem coordinateSystem)
        {
            return coordinateSystem switch
            {
                CoordinateSystem.RobotFrame => Com.Empyreanmed.HeraclesRoboticArm.CoordinateSystems.V1.CoordinateSystem.Robot,
                CoordinateSystem.WorldFrame => Com.Empyreanmed.HeraclesRoboticArm.CoordinateSystems.V1.CoordinateSystem.World,
                _ => throw new InvalidCastException("Unknown argument: " + coordinateSystem.ToString())
            };
        }

        public static Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1.OperatingMode ToProto(OperatingMode operatingMode)
        {
            switch (operatingMode)
            {
                case OperatingMode.RemoteControl:
                    return Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1.OperatingMode.RemoteControl;
                case OperatingMode.LocalControl:
                    return Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1.OperatingMode.LocalControl;
                default:
                    throw new InvalidCastException("Unknown argument: " + operatingMode.ToString());
            }
        }

        public static OperatingMode FromProto(Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1.OperatingMode operatingMode)
        {
            switch (operatingMode)
            {
                case Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1.OperatingMode.RemoteControl:
                    return OperatingMode.RemoteControl;
                case Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1.OperatingMode.LocalControl:
                    return OperatingMode.LocalControl;
                default:
                    throw new InvalidCastException("Unknown argument: " + operatingMode.ToString());
            }
        }

        public static Status FromProto(Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status status)
        {
            switch (status)
            {
                case Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status.Activated:
                    return Status.Activated;
                case Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status.Deactivated:
                    return Status.Deactivated;
                case Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status.RoboticFailure:
                    return Status.RoboticFailure;
                case Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status.RosServerFailure:
                    return Status.RosServerFailure;
                case Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status.RosClientFailure:
                    return Status.RosClientFailure;
                case Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1.Status.Unspecified:
                    return Status.Unspecified;
                default:
                    throw new InvalidCastException("Unknown argument: " + status.ToString());
            }
        }


        #endregion

        public static AngularPosition FromProto(Com.Empyreanmed.HeraclesRoboticArm.Positions.V1.AngularPosition angularPosition)
        {
            return new AngularPosition()
            {
                Rx = (float)MathHelpers.ConvertRadiansToDegrees(angularPosition.Rx),
                Ry = (float)MathHelpers.ConvertRadiansToDegrees(angularPosition.Ry),
                Rz = (float)MathHelpers.ConvertRadiansToDegrees(angularPosition.Rz),
            };
        }

        public static Com.Empyreanmed.HeraclesRoboticArm.Positions.V1.AngularPosition ToProto(AngularPosition angularPosition)
        {
            return new Com.Empyreanmed.HeraclesRoboticArm.Positions.V1.AngularPosition()
            {
                Rx = (float)MathHelpers.ConvertDegreesToRadians(angularPosition.Rx),
                Ry = (float)MathHelpers.ConvertDegreesToRadians(angularPosition.Ry),
                Rz = (float)MathHelpers.ConvertDegreesToRadians(angularPosition.Rz),
            };
        }

        public static CartesianPosition FromProto(Com.Empyreanmed.HeraclesRoboticArm.Positions.V1.CartesianPosition cartesianPosition)
        {
            return new CartesianPosition()
            {
                X = cartesianPosition.X,
                Y = cartesianPosition.Y,
                Z = cartesianPosition.Z,
            };
        }

        public static Com.Empyreanmed.HeraclesRoboticArm.Positions.V1.CartesianPosition ToProto(CartesianPosition cartesianPosition)
        {
            return new Com.Empyreanmed.HeraclesRoboticArm.Positions.V1.CartesianPosition()
            {
                X = cartesianPosition.X,
                Y = cartesianPosition.Y,
                Z = cartesianPosition.Z,
            };
        }

        public static PingActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.PingActionResponse response)
        {
            PingActionResponse r = new();
            r.Tag = FromProto(response.Tag);
            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasFeedbackPongId)
            {
                r.FeedbackPongId = response.FeedbackPongId;
            }
            if (response.HasResultPongsTotal)
            {
                r.ResultPongsTotal = response.ResultPongsTotal;
            }
            return r;
        }

        public static MotionActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.TranslateRelativeActionResponse response)
        {
            MotionActionResponse r = new();
            r.Tag = FromProto(response.Tag);

            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasResultSuccess)
            {
                r.ResultSuccess = response.ResultSuccess;
            }
            if (response.HasResultDetails)
            {
                r.ResultDetails = response.ResultDetails;
            }
            if (response.FeedbackPositionMm != null)
            {
                r.FeedbackPositionMm = FromProto(response.FeedbackPositionMm);
            }
            if (response.FeedbackPositionDeg != null)
            {
                r.FeedbackPositionDeg = FromProto(response.FeedbackPositionDeg);
            }
            return r;
        }

        public static SetOperatingModeActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.SetOperatingModeActionResponse response)
        {
            SetOperatingModeActionResponse r = new();
            r.Tag = FromProto(response.Tag);

            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasResultSuccess)
            {
                r.ResultSuccess = response.ResultSuccess;
            }
            if (response.HasResultDetails)
            {
                r.ResultDetails = response.ResultDetails;
            }
            if (response.HasResultOperatingMode)
            {
                r.ResultOperatingMode = FromProto(response.ResultOperatingMode);
            }
            return r;
        }

        public static MotionActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.RotateRelativeActionResponse response)
        {
            MotionActionResponse r = new();
            r.Tag = FromProto(response.Tag);

            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasResultSuccess)
            {
                r.ResultSuccess = response.ResultSuccess;
            }
            if (response.HasResultDetails)
            {
                r.ResultDetails = response.ResultDetails;
            }
            if (response.FeedbackPositionMm != null)
            {
                r.FeedbackPositionMm = FromProto(response.FeedbackPositionMm);
            }
            if (response.FeedbackPositionDeg != null)
            {
                r.FeedbackPositionDeg = FromProto(response.FeedbackPositionDeg);
            }
            return r;
        }

        public static JointsPosition FromProto(Google.Protobuf.Collections.RepeatedField<double> feedbackJointsPositionsRad)
        {
            JointsPosition feedbackJointPositionsDeg = null;
            if (feedbackJointsPositionsRad != null && feedbackJointsPositionsRad.Count > 0)
            {
                feedbackJointPositionsDeg = new();
                feedbackJointPositionsDeg.JArray.Clear();
                foreach (var joint in feedbackJointsPositionsRad)
                {
                    feedbackJointPositionsDeg.JArray.Add(MathHelpers.ConvertRadiansToDegrees(joint));
                }
            }
            return feedbackJointPositionsDeg;
        }

        public static MotionActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveCustomActionResponse response)
        {
            MotionActionResponse r = new();
            r.Tag = FromProto(response.Tag);

            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasResultSuccess)
            {
                r.ResultSuccess = response.ResultSuccess;
            }
            if (response.HasResultDetails)
            {
                r.ResultDetails = response.ResultDetails;
            }
            if (response.FeedbackJointsPositionsRad != null && response.FeedbackJointsPositionsRad.Count > 0)
            {
                r.FeedbackJointPositionsDeg = FromProto(response.FeedbackJointsPositionsRad);
            }
            return r;
        }

        public static MotionActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveToPositionActionResponse response)
        {
            MotionActionResponse r = new();
            r.Tag = FromProto(response.Tag);

            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasResultSuccess)
            {
                r.ResultSuccess = response.ResultSuccess;
            }
            if (response.HasResultDetails)
            {
                r.ResultDetails = response.ResultDetails;
            }
            if (response.FeedbackPositionMm != null)
            {
                r.FeedbackPositionMm = FromProto(response.FeedbackPositionMm);
            }
            if (response.FeedbackPositionDeg != null)
            {
                r.FeedbackPositionDeg = FromProto(response.FeedbackPositionDeg);
            }
            return r;
        }

        public static MotionActionResponse FromProto(Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1.MoveByMatrixActionResponse response)
        {
            MotionActionResponse r = new();
            r.Tag = FromProto(response.Tag);

            if (response.HasGoalAccepted)
            {
                r.GoalAccepted = response.GoalAccepted;
            }
            if (response.HasResultSuccess)
            {
                r.ResultSuccess = response.ResultSuccess;
            }
            if (response.HasResultDetails)
            {
                r.ResultDetails = response.ResultDetails;
            }
            if (response.FeedbackPositionMm != null)
            {
                r.FeedbackPositionMm = FromProto(response.FeedbackPositionMm);
            }
            if (response.FeedbackPositionDeg != null)
            {
                r.FeedbackPositionDeg = FromProto(response.FeedbackPositionDeg);
            }
            if (response.FeedbackJointsPositionsRad != null && response.FeedbackJointsPositionsRad.Count > 0)
            {
                r.FeedbackJointPositionsDeg = FromProto(response.FeedbackJointsPositionsRad);
            }
            return r;
        }

        public static Com.Empyreanmed.HeraclesRoboticArm.Matrices.V1.Matrix4x4 ToProto(MovementMatrix matrix)
        {
            Com.Empyreanmed.HeraclesRoboticArm.Matrices.V1.Matrix4x4 m = new();
            m.A11 = (float)matrix[0, 0];
            m.A12 = (float)matrix[0, 1];
            m.A13 = (float)matrix[0, 2];
            m.A14 = (float)matrix[0, 3];

            m.A21 = (float)matrix[1, 0];
            m.A22 = (float)matrix[1, 1];
            m.A23 = (float)matrix[1, 2];
            m.A24 = (float)matrix[1, 3];

            m.A31 = (float)matrix[2, 0];
            m.A32 = (float)matrix[2, 1];
            m.A33 = (float)matrix[2, 2];
            m.A34 = (float)matrix[2, 3];

            m.A41 = (float)matrix[3, 0];
            m.A42 = (float)matrix[3, 1];
            m.A43 = (float)matrix[3, 2];
            m.A44 = (float)matrix[3, 3];
            return m;
        }
    }
}
