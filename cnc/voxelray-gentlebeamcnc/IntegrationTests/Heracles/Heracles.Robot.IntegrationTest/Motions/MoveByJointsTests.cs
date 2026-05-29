using Heracles.Robot.IntegrationTest.TestUtils;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.IntegrationTest.Motions
{
    internal class MoveByJointsTests : RobotGrpcIntegrationTestsBase
    {
        private const double JointsPositionPrecisionThreshold = 0.01;
        private const int RobotStopMotionDelayMs = 1000;

        static private JointsPosition CandleCoordinates => new([90, -45, 0, 0, 0, 0]);
        static private JointsPosition ImagingExecutionCoordinates => new([90, -45, -45, 0, -90, 90]);

        static private JointsPosition[] InvalidCoordinates = {
            new JointsPosition([90, -45, 0, 0, 0]), // 5-joint
            new JointsPosition([90, 90, 90, 90, 90, 90]), // causing planning failure 
            new JointsPosition([90, -45, 0, 0, 0, 365]) // causing planning failure 
            };

        [Test]
        public void PositiveTest()
        {
            MotionActionResult = false;
            bool setModeStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(setModeStatus, Is.True);

            bool candleMotionStatus = RobotArmGrpcService.MoveCustomAction(CandleCoordinates);
            Assert.That(candleMotionStatus, Is.True);

            var targetPosition = ImagingExecutionCoordinates;
            bool imagingExecutionMotionStatus = RobotArmGrpcService.MoveCustomAction(targetPosition);

            var finalPosition = RobotArmGrpcService.JointsPositionDeg;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(imagingExecutionMotionStatus, Is.True);
                Assert.That(MotionActionResult ?? false, Is.True);
                Assert.That(
                    finalPosition.IsEqualTo(targetPosition, precision: JointsPositionPrecisionThreshold),
                    Is.True,
                    message: "Final position does not match the target");
            }
        }

        [TestCaseSource(nameof(InvalidCoordinates))]
        public void NegativeTest(JointsPosition invalidJointPosition)
        {
            MotionActionResult = false;
            bool setModeStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(setModeStatus, Is.True);

            var initialPosition = RobotArmGrpcService.JointsPositionDeg;
            bool invalidMotionStatus = RobotArmGrpcService.MoveCustomAction(invalidJointPosition);
                
            var finalPosition = RobotArmGrpcService.JointsPositionDeg;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(invalidMotionStatus, Is.False);
                Assert.That(
                    finalPosition.IsEqualTo(initialPosition, precision: JointsPositionPrecisionThreshold),
                    Is.True,
                    message: "Robot moved from the initial position, but it should not");
            }
        }

        [Test]
        public void PositiveTestWithStop()
        {
            MotionActionResult = false;
            bool setModeStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(setModeStatus, Is.True);

            bool candleMotionStatus = RobotArmGrpcService.MoveCustomAction(CandleCoordinates);
            Assert.That(candleMotionStatus, Is.True);

            // Run a task to stop motion after some delay:
            Task.Run(async () =>
            {
                await Task.Delay(RobotStopMotionDelayMs);
                RobotArmGrpcService.Stop();
            });

            // The following motion will be interrupted, with final position not matching to the target
            var targetPosition = ImagingExecutionCoordinates;
            bool stoppedMotionStatus = RobotArmGrpcService.MoveCustomAction(targetPosition);
            
            var finalPosition = RobotArmGrpcService.JointsPositionDeg;

            Assert.Multiple(() =>
            {
                Assert.That(stoppedMotionStatus, Is.False);
                Assert.That(MotionActionResult ?? false, Is.False);
                Assert.That(
                    finalPosition.IsEqualTo(targetPosition, precision: JointsPositionPrecisionThreshold),
                    Is.False,
                    message: "Final position does not match the target");
            });
        }
    }
}