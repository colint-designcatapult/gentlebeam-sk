using Heracles.Robot.IntegrationTest.TestUtils;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Xcc.Application.Models.RobotArm.Enums;

namespace Heracles.Robot.IntegrationTest.Motions
{
    internal class MoveRelativeTests : RobotGrpcIntegrationTestsBase
    {
        private const double RotationPrecisionThreshold = 0.01;
        private const double TranslationPrecisionThreshold = 0.1;
        private const int InvalidTranslation_mm = 1000;
        private const int RobotStopMotionDelayMs = 1000;

        static private JointsPosition ImagingExecutionCoordinates => new([90, -45, -45, 0, -90, 90]);

        /// <summary>
        /// Utility method to generate a CartesianAngularPosition from a specified relative rotation and translation
        /// </summary>
        /// <param name="rotationAxis"></param>
        /// <param name="rotationAngleDeg"></param>
        /// <param name="translationAxis"></param>
        /// <param name="translation_mm"></param>
        /// <returns></returns>
        private CartesianAngularPosition CalcRelativeMotionPosition(Axis rotationAxis, float rotationAngleDeg, Axis translationAxis, float translation_mm)
        {
            return new()
            {
                AngularPositionDeg = RobotArmGrpcService.ConvertRotateRelativeToPosition(
                    rotationAxis,
                    rotationAngleDeg,
                    CoordinateSystem.WorldFrame).AngularPositionDeg,

                CartesianPositionMM = RobotArmGrpcService.ConvertTranslateRelativeToPosition(
                    translationAxis,
                    translation_mm,
                    CoordinateSystem.WorldFrame).CartesianPositionMM
            };
        }
        private CartesianAngularPosition GetPositionAbsError(CartesianAngularPosition pos1, CartesianAngularPosition pos2)
        {
            var pos1a = pos1.AngularPositionDeg;
            var pos2a = pos2.AngularPositionDeg;
            var pos1c = pos1.CartesianPositionMM;
            var pos2c = pos2.CartesianPositionMM;
            return new CartesianAngularPosition
            {
                AngularPositionDeg = new()
                {
                    Rx = Math.Abs(pos1a.Rx - pos2a.Rx),
                    Ry = Math.Abs(pos1a.Ry - pos2a.Ry),
                    Rz = Math.Abs(pos1a.Rz - pos2a.Rz)
                },
                CartesianPositionMM = new()
                {
                    X = Math.Abs(pos1c.X - pos2c.X),
                    Y = Math.Abs(pos1c.Y - pos2c.Y),
                    Z = Math.Abs(pos1c.Z - pos2c.Z),
                }
            };
        }

        [TestCase(Axis.X, 3, Axis.X, 0)] // rotation by 3 deg only
        [TestCase(Axis.Y, -5, Axis.X, 0)] // rotation by 5 deg only
        [TestCase(Axis.Z, 7, Axis.X, 0)] // rotation by 7 deg only
        [TestCase(Axis.X, 0, Axis.X, -10)] // translation by 10 mm only
        [TestCase(Axis.X, 0, Axis.Y, 20)] // translation by 20 mm only
        [TestCase(Axis.X, 0, Axis.Z, -40)] // translation by 40 mm only
        [TestCase(Axis.X, -3, Axis.Y, 10)] // combined relative pos 
        [TestCase(Axis.Y, 4, Axis.Z, -11)] // combined relative pos 
        [TestCase(Axis.Z, -5, Axis.X, 12)] // combined relative pos 
        public void PositiveTest(
            Axis rotationAxis, float rotationAngleDeg,
            Axis translationAxis, float translation_mm)
        {
            MotionActionResult = false;
            bool setModeStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(setModeStatus, Is.True);

            // Move to test start position:
            bool imagingExecutionMotionStatus = RobotArmGrpcService.MoveCustomAction(ImagingExecutionCoordinates);
            Assert.That(imagingExecutionMotionStatus, Is.True);

            // Now try some reasonable relative motion:
            CartesianAngularPosition relativeMotionPosition = CalcRelativeMotionPosition(rotationAxis, rotationAngleDeg, translationAxis, translation_mm);

            bool relativeMotionStatus = RobotArmGrpcService.MoveToPositionAction(relativeMotionPosition);

            var finalPosition = RobotArmGrpcService.CartesianAngularPosition;

            var positionError = GetPositionAbsError(relativeMotionPosition, finalPosition);
            // We need to estimate how large the final angle to the desired position is:
            Assert.Multiple(() =>
            {
                Assert.That(MotionActionResult ?? false, Is.True);
                Assert.That(positionError.AngularPositionDeg.Rx, Is.AtMost(RotationPrecisionThreshold));
                Assert.That(positionError.AngularPositionDeg.Ry, Is.AtMost(RotationPrecisionThreshold));
                Assert.That(positionError.AngularPositionDeg.Rz, Is.AtMost(RotationPrecisionThreshold));
                Assert.That(positionError.CartesianPositionMM.X, Is.AtMost(TranslationPrecisionThreshold));
                Assert.That(positionError.CartesianPositionMM.Y, Is.AtMost(TranslationPrecisionThreshold));
                Assert.That(positionError.CartesianPositionMM.Z, Is.AtMost(TranslationPrecisionThreshold));
            });
        }


        [Test]
        public void NegativeTest()
        {
            MotionActionResult = false;
            bool setModeStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(setModeStatus, Is.True);

            CartesianAngularPosition impossibleRelativeMotionPosition = CalcRelativeMotionPosition(
                rotationAxis: Axis.X, rotationAngleDeg: 180,
                translationAxis: Axis.Z, translation_mm: InvalidTranslation_mm);

            var initialJointsPosition = RobotArmGrpcService.JointsPositionDeg;

            bool impossibleRelativeMotionStatus = RobotArmGrpcService.MoveToPositionAction(impossibleRelativeMotionPosition);

            var finalJointsPosition = RobotArmGrpcService.JointsPositionDeg;
            Assert.Multiple(() =>
            {
                Assert.That(impossibleRelativeMotionStatus, Is.False);
                Assert.That(MotionActionResult, Is.False);
                Assert.That(finalJointsPosition.IsEqualTo(initialJointsPosition, precision: RotationPrecisionThreshold));
            });
        }

        [Test]
        public void PositiveTestWithStop()
        {
            MotionActionResult = false;
            bool setModeStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(setModeStatus, Is.True);

            // Move to test start position:
            bool imagingExecutionMotionStatus = RobotArmGrpcService.MoveCustomAction(ImagingExecutionCoordinates);
            Assert.That(imagingExecutionMotionStatus, Is.True);

            // Get some reasonable motion with rotation & translation,
            // long enough to stop in the middle of it:
            CartesianAngularPosition relativeMotionPosition = CalcRelativeMotionPosition(
                rotationAxis : Axis.Z,
                rotationAngleDeg: 180,
                translationAxis : Axis.Y, 
                translation_mm : 30);

            // Run a task to stop motion after 0.5 sec:
            Task.Run(async () =>
            {
                await Task.Delay(RobotStopMotionDelayMs);
                RobotArmGrpcService.Stop();
            });

            // The following motion will be interrupted, with final position not matching to the target
            bool stoppedRelativeMotionStatus = RobotArmGrpcService.MoveToPositionAction(relativeMotionPosition);

            Assert.Multiple(() =>
            {
                Assert.That(stoppedRelativeMotionStatus, Is.False);
                Assert.That(MotionActionResult ?? false, Is.False);
            });

            var finalPosition = RobotArmGrpcService.CartesianAngularPosition;
            var positionError = GetPositionAbsError(relativeMotionPosition, finalPosition);
            // We need to estimate how large the final angle to the desired position is:
            Assert.Multiple(() =>
            {
                Assert.That(positionError.CartesianPositionMM.Y, Is.GreaterThan(TranslationPrecisionThreshold));
                Assert.That(positionError.AngularPositionDeg.Rz, Is.GreaterThan(RotationPrecisionThreshold));
            });
        }
    }
}

