using Com.Empyreanmed.HeraclesRoboticArm.ActionResponseTags.V1;
using Com.Empyreanmed.HeraclesRoboticArm.MoveCommands.V1;
using Com.Empyreanmed.HeraclesRoboticArm.OperatingModes.V1;
using Com.Empyreanmed.HeraclesRoboticArm.Positions.V1;
using Com.Empyreanmed.HeraclesRoboticArm.Statuses.V1;

using Heracles.Application.Protos;
using Heracles.Application.Test.TestUtils;
using Xcc.Application.Models.RobotArm;
using Xcc.Core.Helpers;

using RobotArm = Heracles.Robot.Models.RobotArm;

namespace Heracles.Robot.Test.Protos
{
    ///// <summary>
    ///// Particular static method invocation utility class via reflection
    ///// for ProtoTypesConverterInvokerTemplate's overloaded ToProto/FromProto methods
    ///// </summary>
    ///// <typeparam name="TClass"></typeparam>
    ///// <typeparam name="TConverterClass"></typeparam>
    public class ProtoTypesConverterInvokerTemplate<TClass, TConverterClass>
        where TConverterClass : class
    {
        protected static object? InvokeConverter(string methodName, TClass value)
        {
            return ReflectionHelper<TConverterClass>.InvokeStaticWithParam(methodName, value);
        }

        public virtual object? ToProto(TClass value)
        {
            return InvokeConverter("ToProto", value);
        }

        public virtual object? FromProto(TClass value)
        {
            return InvokeConverter("FromProto", value);
        }
    }

    /// <summary>
    /// Particular static method invocation utility class via reflection
    /// for ProtoRobotTypeConverter's overloaded ToProto/FromProto methods
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    public class ProtoRobotTypesConverterInvoker<TClass> : ProtoTypesConverterInvokerTemplate<TClass, ProtoRobotTypesConverter>
    { }

    /// <summary>
    /// Test fixture with parameters of enum type and its invalid value to test against.
    /// For many enums not having any non-convertable values, some out-of-range value is used to test the default (error) branch on it.
    /// </summary>
    /// <typeparam name="TEnumType">Type of Xcc/Heracles to convert to a Protos enum</typeparam>
    [TestFixture(typeof(Xcc.Application.Models.RobotArm.Enums.Axis), Xcc.Application.Models.RobotArm.Enums.Axis.X - 1)]
    [TestFixture(typeof(RobotArm.Enums.OperatingMode), RobotArm.Enums.OperatingMode.RemoteControl - 1)]
    [TestFixture(typeof(RobotArm.Enums.CoordinateSystem), RobotArm.Enums.CoordinateSystem.RobotFrame - 1)]
    public class ProtoRobotTypesConverter_RobotEnumsToProtoTests<TEnumType> : ProtoRobotTypesConverterInvoker<TEnumType>
        where TEnumType : struct, Enum
    {
        public TEnumType InvalidValue { get; }

        public ProtoRobotTypesConverter_RobotEnumsToProtoTests(TEnumType invalidValue)
        {
            InvalidValue = invalidValue;
        }

        [Test]
        public void ValidConversionTest([Values] TEnumType enumValue)
        {
            TestDelegate testCall = () => ToProto(enumValue);
            if (!enumValue.Equals(InvalidValue))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }

        [Test]
        public void InvalidConversionTest()
        {
            Assert.Throws<InvalidCastException>(() => ToProto(InvalidValue));
        }
    }

    /// <summary>
    /// Test fixture with parameters of enum type and its invalid value to test against.
    /// For some enums not having any non-convertable values, an out-of-range value is used to test the default (error) branch on it.
    /// </summary>
    /// <typeparam name="TEnumType">Type of Protos to convert from</typeparam>
    [TestFixture(typeof(ActionResponseTag), ActionResponseTag.GoalUnspecified - 1)]
    [TestFixture(typeof(OperatingMode), OperatingMode.Unspecified)]
    [TestFixture(typeof(Status), Status.Unspecified - 1)]
    public class ProtoRobotTypesConverter_RobotEnumsFromProtoTests<TEnumType> : ProtoRobotTypesConverterInvoker<TEnumType>
        where TEnumType : struct, Enum
    {
        public TEnumType InvalidValue { get; }

        public ProtoRobotTypesConverter_RobotEnumsFromProtoTests(TEnumType invalidValue)
        {
            InvalidValue = invalidValue;
        }

        /// <summary>
        /// Tests for the conversion of all the defined values of the enum type
        /// </summary>
        /// <param name="enumValue"></param>
        [Test]
        public void ValidConversionTest([Values] TEnumType enumValue)
        {
            TestDelegate testCall = () => FromProto(enumValue);
            if (!enumValue.Equals(InvalidValue))
            {
                Assert.DoesNotThrow(testCall);
            }
            else
            {
                Assert.Throws<InvalidCastException>(testCall);
            }
        }

        /// <summary>
        /// Tests for an invalid value conversion in case if it's out of range of the enum type
        /// </summary>
        [Test]
        public void InvalidConversionTest()
        {
            Assert.Throws<InvalidCastException>(() => FromProto(InvalidValue));
        }
    }

    public class ProtoRobotTypesConverter_RobotClassesFromProtoTests
    {
        [Test]
        public void AngularPosition_ConversionTest()
        {
            var position = new AngularPosition { Rx = 1, Ry = 2, Rz = 3 };
            RobotArm.AngularPosition converted = ProtoRobotTypesConverter.FromProto(position);
            // As protos use radians, and the conversion turns them into degrees,
            // and some tiny errors are introduced by type conversion & cast,
            // we expect that the converted values in degrees
            // will correspond to the initial values in degrees within some error interval
            Assert.Multiple(() =>
            {
                Assert.That(Math.Abs(converted.Rx - MathHelpers.ConvertRadiansToDegrees(position.Rx)), Is.AtMost(0.001));
                Assert.That(Math.Abs(converted.Ry - MathHelpers.ConvertRadiansToDegrees(position.Ry)), Is.AtMost(0.001));
                Assert.That(Math.Abs(converted.Rz - MathHelpers.ConvertRadiansToDegrees(position.Rz)), Is.AtMost(0.001));
            });
        }

        [Test]
        public void AngularPosition_Back_ConversionTest()
        {
            var position = new RobotArm.AngularPosition { Rx = 1, Ry = 2, Rz = 3 };
            AngularPosition converted = ProtoRobotTypesConverter.ToProto(position);
            // As protos use radians, and the conversion turns them into degrees,
            // and some tiny errors are introduced by type conversion & cast,
            // we expect that the converted values in degrees
            // will correspond to the initial values in degrees within some error interval
            Assert.Multiple(() =>
            {
                Assert.That(Math.Abs(converted.Rx - MathHelpers.ConvertDegreesToRadians(position.Rx)), Is.AtMost(0.001));
                Assert.That(Math.Abs(converted.Ry - MathHelpers.ConvertDegreesToRadians(position.Ry)), Is.AtMost(0.001));
                Assert.That(Math.Abs(converted.Rz - MathHelpers.ConvertDegreesToRadians(position.Rz)), Is.AtMost(0.001));
            });
        }

        [Test]
        public void CartesianPosition_ConversionTest()
        {
            var position = new CartesianPosition { X = 1, Y = 2, Z = 3 };
            RobotArm.CartesianPosition converted = ProtoRobotTypesConverter.FromProto(position);
            Assert.Multiple(() =>
            {
                Assert.That(converted.X, Is.EqualTo(position.X));
                Assert.That(converted.Y, Is.EqualTo(position.Y));
                Assert.That(converted.Z, Is.EqualTo(position.Z));
            });
        }

        [Test]
        public void CartesianPosition_Back_ConversionTest()
        {
            var position = new RobotArm.CartesianPosition { X = 1, Y = 2, Z = 3 };
            CartesianPosition converted = ProtoRobotTypesConverter.ToProto(position);
            Assert.Multiple(() =>
            {
                Assert.That(converted.X, Is.EqualTo(position.X));
                Assert.That(converted.Y, Is.EqualTo(position.Y));
                Assert.That(converted.Z, Is.EqualTo(position.Z));
            });
        }

        [Test]
        public void PingActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields 
                ? new PingActionResponse() { Tag = ActionResponseTag.Feedback, FeedbackPongId = 1, GoalAccepted = true, ResultPongsTotal = 2 } 
                : new PingActionResponse() { Tag = ActionResponseTag.Feedback };
            RobotArm.PingActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Feedback));
            if (hasFields) {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.FeedbackPongId, Is.EqualTo(response.FeedbackPongId));
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultPongsTotal, Is.EqualTo(response.ResultPongsTotal));
                });
            }
            else
            {
                Assert.That(converted.FeedbackPongId, Is.Null);
                Assert.That(converted.GoalAccepted, Is.Null);
                Assert.That(converted.ResultPongsTotal, Is.Null);
            }
        }

        [Test]
        public void TranslateRelativeActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields
                ? new TranslateRelativeActionResponse() { 
                    Tag = ActionResponseTag.Result,
                    GoalAccepted = true,
                    ResultSuccess = true,
                    ResultDetails = "details",
                    FeedbackPositionMm = new CartesianPosition { X = 1, Y = 2, Z = 3 },
                    FeedbackPositionDeg = new AngularPosition { Rx = 1, Ry = 2, Rz = 3 },
                    }
                : new TranslateRelativeActionResponse();
            RobotArm.MotionActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            if (hasFields)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Result));
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultSuccess, Is.EqualTo(response.ResultSuccess));
                    Assert.That(converted.ResultDetails, Is.EqualTo(response.ResultDetails));
                    Assert.That(converted.FeedbackPositionMm.X, Is.EqualTo(response.FeedbackPositionMm.X));
                    Assert.That(converted.FeedbackPositionMm.Y, Is.EqualTo(response.FeedbackPositionMm.Y));
                    Assert.That(converted.FeedbackPositionMm.Z, Is.EqualTo(response.FeedbackPositionMm.Z));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rx - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rx)), 
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Ry - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Ry)), 
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rz - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rz)), 
                        Is.AtMost(0.001));
                });
            }
            else
            {
                Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Goal)); // Goal is unspecified by default
                Assert.That(converted.GoalAccepted, Is.EqualTo(null));
                Assert.That(converted.ResultSuccess, Is.EqualTo(null));
                Assert.That(converted.ResultDetails, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionMm, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionDeg, Is.EqualTo(null));
            }
        }

        [Test]
        public void SetOperatingModeActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields
                ? new SetOperatingModeActionResponse()
                {
                    Tag = ActionResponseTag.Result,
                    GoalAccepted = true,
                    ResultSuccess = true,
                    ResultDetails = "details",
                    ResultOperatingMode = OperatingMode.LocalControl,
                }
                : new SetOperatingModeActionResponse();
            RobotArm.SetOperatingModeActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            if (hasFields)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Result));
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultSuccess, Is.EqualTo(response.ResultSuccess));
                    Assert.That(converted.ResultDetails, Is.EqualTo(response.ResultDetails));
                    Assert.That(converted.ResultOperatingMode, Is.EqualTo(RobotArm.Enums.OperatingMode.LocalControl));
                });
            }
            else
            {
                Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Goal)); // Goal is unspecified by default
                Assert.That(converted.GoalAccepted, Is.EqualTo(null));
                Assert.That(converted.ResultSuccess, Is.EqualTo(null));
                Assert.That(converted.ResultDetails, Is.EqualTo(null));
                Assert.That(converted.ResultOperatingMode, Is.EqualTo(null));
            }
        }

        [Test]
        public void RotateRelativeActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields
                ? new RotateRelativeActionResponse()
                {
                    Tag = ActionResponseTag.Result,
                    GoalAccepted = true,
                    ResultSuccess = true,
                    ResultDetails = "details",
                    FeedbackPositionMm = new CartesianPosition { X = 1, Y = 2, Z = 3 },
                    FeedbackPositionDeg = new AngularPosition { Rx = 1, Ry = 2, Rz = 3 },
                }
                : new RotateRelativeActionResponse();
            RobotArm.MotionActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            if (hasFields)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Result));
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultSuccess, Is.EqualTo(response.ResultSuccess));
                    Assert.That(converted.ResultDetails, Is.EqualTo(response.ResultDetails));
                    Assert.That(converted.FeedbackPositionMm.X, Is.EqualTo(response.FeedbackPositionMm.X));
                    Assert.That(converted.FeedbackPositionMm.Y, Is.EqualTo(response.FeedbackPositionMm.Y));
                    Assert.That(converted.FeedbackPositionMm.Z, Is.EqualTo(response.FeedbackPositionMm.Z));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rx - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rx)),
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Ry - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Ry)),
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rz - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rz)),
                        Is.AtMost(0.001));
                });
            }
            else
            {
                Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Goal)); // Goal is unspecified by default
                Assert.That(converted.GoalAccepted, Is.EqualTo(null));
                Assert.That(converted.ResultSuccess, Is.EqualTo(null));
                Assert.That(converted.ResultDetails, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionMm, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionDeg, Is.EqualTo(null));
            }
        }

        [Test]
        public void MoveCustomActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields
                ? new MoveCustomActionResponse() {
                    Tag = ActionResponseTag.Result,
                    GoalAccepted = true,
                    ResultSuccess = true,
                    ResultDetails = "details" }
                : new MoveCustomActionResponse();
            RobotArm.MotionActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            if (hasFields)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Result));
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultSuccess, Is.EqualTo(response.ResultSuccess));
                    Assert.That(converted.ResultDetails, Is.EqualTo(response.ResultDetails));
                });
            }
            else
            {
                Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Goal)); // Goal is unspecified by default
                Assert.That(converted.GoalAccepted, Is.EqualTo(null));
                Assert.That(converted.ResultSuccess, Is.EqualTo(null));
                Assert.That(converted.ResultDetails, Is.EqualTo(null));
            }
        }

        [Test]
        public void MoveToPositionActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields
                ? new MoveToPositionActionResponse()
                {
                    Tag = ActionResponseTag.Result,
                    GoalAccepted = true,
                    ResultSuccess = true,
                    ResultDetails = "details",
                    FeedbackPositionMm = new CartesianPosition { X = 1, Y = 2, Z = 3 },
                    FeedbackPositionDeg = new AngularPosition { Rx = 1, Ry = 2, Rz = 3 },
                }
                : new MoveToPositionActionResponse();
            RobotArm.MotionActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            if (hasFields)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Result));
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultSuccess, Is.EqualTo(response.ResultSuccess));
                    Assert.That(converted.ResultDetails, Is.EqualTo(response.ResultDetails));
                    Assert.That(converted.FeedbackPositionMm.X, Is.EqualTo(response.FeedbackPositionMm.X));
                    Assert.That(converted.FeedbackPositionMm.Y, Is.EqualTo(response.FeedbackPositionMm.Y));
                    Assert.That(converted.FeedbackPositionMm.Z, Is.EqualTo(response.FeedbackPositionMm.Z));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rx - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rx)),
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Ry - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Ry)),
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rz - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rz)),
                        Is.AtMost(0.001));
                });
            }
            else
            {
                Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Goal)); // Goal is unspecified by default
                Assert.That(converted.GoalAccepted, Is.EqualTo(null));
                Assert.That(converted.ResultSuccess, Is.EqualTo(null));
                Assert.That(converted.ResultDetails, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionMm, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionDeg, Is.EqualTo(null));
            }
        }

        [Test]
        public void MoveByMatrixActionResponse_ConversionTest([Values(true, false)] bool hasFields)
        {
            var response = hasFields
                ? new MoveByMatrixActionResponse()
                {
                    Tag = ActionResponseTag.Feedback,
                    GoalAccepted = true,
                    ResultSuccess = true,
                    ResultDetails = "details",
                    FeedbackPositionMm = new CartesianPosition { X = 1, Y = 2, Z = 3 },
                    FeedbackPositionDeg = new AngularPosition { Rx = 1, Ry = 2, Rz = 3 },
                }
                : new MoveByMatrixActionResponse() { Tag = ActionResponseTag.Feedback };
            RobotArm.MotionActionResponse converted = ProtoRobotTypesConverter.FromProto(response);

            Assert.That(converted.Tag, Is.EqualTo(RobotArm.Enums.ActionResponseTag.Feedback));
            if (hasFields)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(converted.GoalAccepted, Is.EqualTo(response.GoalAccepted));
                    Assert.That(converted.ResultSuccess, Is.EqualTo(response.ResultSuccess));
                    Assert.That(converted.ResultDetails, Is.EqualTo(response.ResultDetails));
                    Assert.That(converted.FeedbackPositionMm.X, Is.EqualTo(response.FeedbackPositionMm.X));
                    Assert.That(converted.FeedbackPositionMm.Y, Is.EqualTo(response.FeedbackPositionMm.Y));
                    Assert.That(converted.FeedbackPositionMm.Z, Is.EqualTo(response.FeedbackPositionMm.Z));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rx - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rx)),
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Ry - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Ry)),
                        Is.AtMost(0.001));
                    Assert.That(
                        Math.Abs(converted.FeedbackPositionDeg.Rz - MathHelpers.ConvertRadiansToDegrees(response.FeedbackPositionDeg.Rz)),
                        Is.AtMost(0.001));
                });
            }
            else
            {
                Assert.That(converted.GoalAccepted, Is.EqualTo(null));
                Assert.That(converted.ResultSuccess, Is.EqualTo(null));
                Assert.That(converted.ResultDetails, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionMm, Is.EqualTo(null));
                Assert.That(converted.FeedbackPositionDeg, Is.EqualTo(null));
            }
        }

        [Test]
        public void MovementMatrix_ToProtoConversionTest()
        {
            MovementMatrix matrix = new();
            matrix[0, 3] = 4;
            matrix[3, 0] = -4;

            var converted = ProtoRobotTypesConverter.ToProto(matrix);
            Assert.Multiple(() =>
            {
                // Should be an identity matrix with our changes, so check some nonzero values here:
                Assert.That(converted.A11, Is.EqualTo(Convert.ToSingle(matrix[0, 0])));
                Assert.That(converted.A44, Is.EqualTo(Convert.ToSingle(matrix[0, 0])));
                Assert.That(converted.A14, Is.EqualTo(Convert.ToSingle(matrix[0, 3])));
                Assert.That(converted.A41, Is.EqualTo(Convert.ToSingle(matrix[3, 0])));
            });
        }

        [Test]
        public void JointsPosition_ConversionTest()
        {
            Google.Protobuf.Collections.RepeatedField<double> feedbackJointsPositionsRad = new() { 6.1, 5.2, 4.3, 3.4, 2.5, 1.6 };
            RobotArm.JointsPosition jointsPositionDeg = ProtoRobotTypesConverter.FromProto(feedbackJointsPositionsRad);
            // As protos use radians, and the conversion turns them into degrees,
            // and some tiny errors are introduced by type conversion & cast,
            // we expect that the converted values in degrees
            // will correspond to the initial values in degrees within some error interval
            Assert.Multiple(() =>
            {
                for (int i = 0; i < 6; ++i)
                {
                    Assert.That(Math.Abs(jointsPositionDeg.JArray[i] - MathHelpers.ConvertRadiansToDegrees(feedbackJointsPositionsRad[i])), Is.AtMost(0.001));
                }
            });
        }
    }
}
