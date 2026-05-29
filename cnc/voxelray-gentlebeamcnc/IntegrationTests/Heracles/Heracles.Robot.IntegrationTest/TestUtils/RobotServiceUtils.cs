using Heracles.Core.Models;
using Moq;
using System.Diagnostics;
using Heracles.Robot.Models.RobotArm.Enums;
using Xcc.Core.Logging;
using Heracles.Robot.Services;

namespace Heracles.Robot.IntegrationTest.TestUtils
{
    public class RobotServiceUtils
    {
        private const string DEFAULT_ROBOT_GRPC_SERVER_URI_STRING = "http://127.0.0.1:50051";

        private Uri _robotGrpcServerUri = new(DEFAULT_ROBOT_GRPC_SERVER_URI_STRING);
        public Uri RobotGrpcServerUri => _robotGrpcServerUri;

        private Mock<ILogWriter> _logServiceMock;
        protected ILogWriter LogService => _logServiceMock.Object;

        private Mock<IHeraclesMainSettings> _robotTestAppSettings;
        protected IHeraclesMainSettings RobotTestHeraclesMainSettings => _robotTestAppSettings.Object;

        public RobotArmGrpcService RobotArmGrpcService;

        public RobotServiceUtils(string? robotGrpcEndpoint)
        {
            if (robotGrpcEndpoint is not null)
            {
                _robotGrpcServerUri = new Uri($"http://{robotGrpcEndpoint}");
            }
            _logServiceMock = new();

            _robotTestAppSettings = new();
            _robotTestAppSettings.SetupGet(s => s.RobotGrpcServerUri).Returns(_robotGrpcServerUri);

            RobotArmGrpcService = new RobotArmGrpcService(_logServiceMock.Object, _robotTestAppSettings.Object);
        }

        static public Task<bool> WaitForServerStatus(
            RobotArmGrpcService robotArmGrpcService,
            Status statusToWait,
            CancellationToken cancellationToken,
            int pause_ms = 1000)
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var currentStatus = robotArmGrpcService.Status;
                        if (currentStatus == statusToWait)
                        {
                            Debug.WriteLine($"Robot reached awaited status: {statusToWait}");
                            return true;
                        }
                        else
                        {
                            Debug.WriteLine($"Robot status differs from awaited: wait for {statusToWait}, current={currentStatus}");
                            await Task.Delay(pause_ms, cancellationToken);
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    catch (TaskCanceledException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                    }
                }
            });
        }
        /// <summary>
        /// Sets operating mode by trying setting another mode first
        /// to make sure the mode was actually switched
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        static public bool SafeSetServiceOperatingMode(RobotArmGrpcService service, OperatingMode mode)
        {
            var initialMode = (mode == OperatingMode.LocalControl) ? OperatingMode.RemoteControl : OperatingMode.LocalControl;
            service.SetOperatingMode(initialMode);
            return service.SetOperatingMode(mode);
        }
    }
}
