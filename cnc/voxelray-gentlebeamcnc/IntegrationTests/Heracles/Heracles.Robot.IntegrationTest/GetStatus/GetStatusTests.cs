using Heracles.Robot.IntegrationTest.TestUtils;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Microsoft.Extensions.Configuration;

namespace Heracles.Robot.IntegrationTest.GetStatus
{
    internal class GetStatusTests : RobotGrpcIntegrationTestsBase
    {
        #region Constants
        static private JointsPosition CandleCoordinates => new([90, -45, 0, 0, 0, 0]);
        static private JointsPosition ImagingExecutionCoordinates => new([90, -45, -45, 0, -90, 90]);
        private const double JointsPositionPrecisionThreshold = 1;
        private const int FailureStatusCheckIntervalMilliseconds = 100;
        private const int RosProcessKillDelayMilliseconds = 500;
        protected int MaxServerRestartTimeoutMilliseconds = 2 * 60 * 1000;

        IConfiguration Config { get; set; }
        #endregion Constants

        private RobotSshUtils SshUtils { get; set; }

        protected override void ApplyConfig(IConfiguration config)
        {
            base.ApplyConfig(config);
            Config = config;
        }

        [SetUp]
        public void GetStatusSetup()
        {
            SshUtils = new RobotSshUtils(Config, RobotGrpcServerUri);
            // Turn remote control on to operate on the service
            bool takeControlStatus = RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            Assert.That(takeControlStatus, Is.True);
        }

        [Test]
        public async Task RestartTest()
        {
            CancellationTokenSource tokenSource = 
                TokenSourceFactory.CreateAutoCancellingTokenSource(MaxServerRestartTimeoutMilliseconds);

            await SshUtils.RestartServerAsync();

            // First, wait for a client shutdown.
            // We probably would better set a watchdog on status change to not skip it if restart goes fast,
            // but the waiting with some short interval like 100ms between checks should be enough as well,
            // and this is much easier to do now.
            bool waitForClientShutdown = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.RosClientFailure,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds);

            Assert.That(waitForClientShutdown, Is.True);

            // Wait for client to restart with Unspecified initial status:
            bool waitForRestart = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.Unspecified,
                tokenSource.Token);
            
            bool? isFake = RobotArmGrpcService.IsFakeHardware();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(waitForRestart, Is.True);
                Assert.That(isFake, Is.EqualTo(true));
            }
            // Switch mode forth and back to get real feedback (due to some issue in status setting logic)
            bool takeControlStatus = 
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl);
            Assert.That(takeControlStatus, Is.True);
        }

        [Test]
        public async Task RosServerFailureStatusTest()
        {
            // Make sure that the RosServer isn't failed yet
            Assert.That(RobotArmGrpcService.Status, Is.Not.EqualTo(Status.RosServerFailure));

            CancellationTokenSource tokenSource =
                TokenSourceFactory.CreateAutoCancellingTokenSource(MaxServerRestartTimeoutMilliseconds);

            // Kill the server and make sure that we see it in the status:
            await SshUtils.KillServerAsync();
            bool waitForServerFailure = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.RosServerFailure,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds);
            Assert.That(waitForServerFailure, Is.True);

            // Now restart the entire ROS stuff and wait for it going alive again
            await SshUtils.RestartServerAsync();
            bool waitForRestart = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.Unspecified,
                tokenSource.Token);

            bool? isFake = RobotArmGrpcService.IsFakeHardware();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(waitForRestart, Is.True);
                Assert.That(isFake, Is.EqualTo(true));
            }

            // Switch mode forth and back to get real feedback (due to some issue in status setting logic)
            bool takeControlStatus =
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl);
            Assert.That(takeControlStatus, Is.True);
        }

        [Test]
        public async Task RosClientFailureStatusTest()
        {
            // Make sure that the RosClient isn't failed yet
            Assert.That(RobotArmGrpcService.Status, Is.Not.EqualTo(Status.RosClientFailure));

            CancellationTokenSource tokenSource =
                TokenSourceFactory.CreateAutoCancellingTokenSource(MaxServerRestartTimeoutMilliseconds);

            // Kill the client and make sure that we see it in the status:
            await SshUtils.KillClientAsync();
            bool waitForClientFailure = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.RosClientFailure,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds);
            Assert.That(waitForClientFailure, Is.True);

            // Now restart the entire ROS stuff and wait for it going alive again
            await SshUtils.RestartServerAsync();
            bool waitForRestart = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.Unspecified,
                tokenSource.Token);

            bool? isFake = RobotArmGrpcService.IsFakeHardware();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(waitForRestart, Is.True);
                Assert.That(isFake, Is.EqualTo(true));
            }

            // Switch mode forth and back to get real feedback (due to some issue in status setting logic)
            bool takeControlStatus =
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl);
            Assert.That(takeControlStatus, Is.True);
        }

        [Test]
        public async Task RosServerFailureStopsMotionTest()
        {
            // Make sure that the RosServer isn't failed yet
            Assert.That(RobotArmGrpcService.Status, Is.Not.EqualTo(Status.RosServerFailure));
            // Make also sure that we set the proper opmode
            Assert.That(
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl), 
                Is.True);
            // Move to initial position
            bool initialPositionMotionStatus = RobotArmGrpcService.MoveCustomAction(CandleCoordinates);
            Assert.Multiple(() =>
            {
                Assert.That(initialPositionMotionStatus, Is.True);
                Assert.That(MotionActionResult ?? false, Is.True);
            });


            CancellationTokenSource tokenSource =
                TokenSourceFactory.CreateAutoCancellingTokenSource(MaxServerRestartTimeoutMilliseconds);

            // Run a task to kill server after ~0.5 sec
            Task serverRestartTask = Task.Run(async () =>
            {
                await Task.Delay(RosProcessKillDelayMilliseconds);
                await SshUtils.KillServerAsync();
            });

            // Start the motion to the target position:
            var targetPosition = ImagingExecutionCoordinates;
            bool motionUntilFailureStatus = RobotArmGrpcService.MoveCustomAction(targetPosition);

            // Make sure that we see server failure in the status:
            bool waitForServerFailure = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.RosServerFailure,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds);
            
            Assert.Multiple(() =>
            {
                Assert.That(waitForServerFailure, Is.True);
                Assert.That(motionUntilFailureStatus, Is.False);
                Assert.That(MotionActionResult ?? false, Is.False);
            });

            // Now restart the entire ROS stuff and wait for it going alive again
            await SshUtils.RestartServerAsync();
            bool waitForRestart = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.Unspecified,
                tokenSource.Token);

            bool? isFake = RobotArmGrpcService.IsFakeHardware();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(waitForRestart, Is.True);
                Assert.That(isFake, Is.EqualTo(true));
            }

            // Switch mode forth and back to get real feedback (due to some issue in status setting logic)
            bool takeControlStatus =
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl);
            // Finally, check the actual position to make sure the robot was stopped
            var finalPosition = RobotArmGrpcService.JointsPositionDeg;
            Assert.Multiple(() =>
            {
                Assert.That(takeControlStatus, Is.True);
                Assert.That(finalPosition.IsEqualTo(targetPosition, precision: 1), Is.False);
            });
        }

        [Test]
        public async Task RosClientFailureStopsMotionTest()
        {
            // Make sure that the RosServer isn't failed yet
            Assert.That(RobotArmGrpcService.Status, Is.Not.EqualTo(Status.RosClientFailure));
            // Make also sure that we set the proper opmode
            Assert.That(
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl), 
                Is.True);
            // Move to initial position
            bool initialPositionMotionStatus = RobotArmGrpcService.MoveCustomAction(CandleCoordinates);
            Assert.Multiple(() =>
            {
                Assert.That(initialPositionMotionStatus, Is.True);
                Assert.That(MotionActionResult ?? false, Is.True);
            });


            CancellationTokenSource tokenSource =
                TokenSourceFactory.CreateAutoCancellingTokenSource(MaxServerRestartTimeoutMilliseconds);

            // Run a task to kill server after ~0.5 sec
            Task serverRestartTask = Task.Run(async () =>
            {
                await Task.Delay(RosProcessKillDelayMilliseconds);
                await SshUtils.KillClientAsync();
            });

            // Start the motion to the target position:
            var targetPosition = ImagingExecutionCoordinates;
            bool motionUntilFailureStatus = RobotArmGrpcService.MoveCustomAction(targetPosition);

            // Make sure that we see server failure in the status:
            bool waitForServerFailure = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.RosClientFailure,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds);

            Assert.Multiple(() =>
            {
                Assert.That(waitForServerFailure, Is.True);
                Assert.That(motionUntilFailureStatus, Is.False);
                Assert.That(MotionActionResult ?? false, Is.False);
            });

            // Now restart the entire ROS stuff and wait for it going alive again
            await SshUtils.RestartServerAsync();
            bool waitForRestart = await RobotServiceUtils.WaitForServerStatus(
                RobotArmGrpcService,
                Status.Unspecified,
                tokenSource.Token);

            bool? isFake = RobotArmGrpcService.IsFakeHardware();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(waitForRestart, Is.True);
                Assert.That(isFake, Is.EqualTo(true));
            }

            // Switch mode forth and back to get real feedback (due to some issue in status setting logic)
            bool takeControlStatus =
                RobotServiceUtils.SafeSetServiceOperatingMode(RobotArmGrpcService, OperatingMode.RemoteControl);
            // Finally, check the actual position to make sure the robot was stopped
            var finalPosition = RobotArmGrpcService.JointsPositionDeg;
            Assert.Multiple(() =>
            {
                Assert.That(takeControlStatus, Is.True);
                Assert.That(
                    finalPosition.IsEqualTo(targetPosition, precision: JointsPositionPrecisionThreshold), 
                    Is.False);
            });
        }

    }
}
