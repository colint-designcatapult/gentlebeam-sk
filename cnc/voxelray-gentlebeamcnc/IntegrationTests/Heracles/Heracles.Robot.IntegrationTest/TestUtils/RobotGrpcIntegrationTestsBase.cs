using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Heracles.Robot.Services;
using Microsoft.Extensions.Configuration;

namespace Heracles.Robot.IntegrationTest.TestUtils
{
    [TestFixture]
    internal abstract class RobotGrpcIntegrationTestsBase
    {

        private string? _robotGrpcEndpoint = "127.0.0.1:50051";
        private RobotServiceUtils _robotServiceUtils;
        protected RobotArmGrpcService RobotArmGrpcService => _robotServiceUtils.RobotArmGrpcService;
        protected Uri RobotGrpcServerUri => _robotServiceUtils.RobotGrpcServerUri;

        private JointsPosition? _testInitialJointPosition = null;
        protected bool? MotionActionResult { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IConfiguration config = ConfigurationProvider.GetConfiguration();
            _robotGrpcEndpoint = config["AppSettings:EndPoints:RobotGrpcServerEndPoint"];

            ApplyConfig(config);
        }

        protected virtual void ApplyConfig(IConfiguration config)
        {
        }

        [SetUp]
        public void Setup()
        {
            _robotServiceUtils = new RobotServiceUtils(_robotGrpcEndpoint);

            bool? isFake = RobotArmGrpcService.IsFakeHardware();
            if (isFake is null)
            {
                throw new Exception("RobotGrpcIntegrationTestsBase: initialization error. Running tests on an unknown robot is not allowed.");
            }
            else if (isFake == false)
            {
                throw new Exception("RobotGrpcIntegrationTestsBase: initialization error. Running tests on a real robot is not allowed.");
            }

            // Turn opmode forth and back to ensure that emulator responds with an actual mode
            RobotArmGrpcService.SetOperatingMode(OperatingMode.LocalControl);
            RobotArmGrpcService.SetOperatingMode(OperatingMode.RemoteControl);
            _testInitialJointPosition = RobotArmGrpcService.JointsPositionDeg;

            if (RobotArmGrpcService.SetOperatingMode(OperatingMode.LocalControl) == false)
            {
                throw new Exception("RobotGrpcIntegrationTestsBase: initialization error. Cannot set initial robot operating mode");
            }

            RobotArmGrpcService.MotionActionFeedback += OnMotionActionFeedback;
        }

        [TearDown]
        public void TearDown()
        {
            if (_testInitialJointPosition != null)
            {
                // Try to restore initial position
                RobotArmGrpcService.MoveCustomAction(_testInitialJointPosition);
            }
            RobotArmGrpcService.Stop();
            RobotArmGrpcService.Dispose();
        }

        private void OnMotionActionFeedback(object? sender, MotionActionResponse e)
        {
            MotionActionResult = e.ResultSuccess;
        }

    }
}
