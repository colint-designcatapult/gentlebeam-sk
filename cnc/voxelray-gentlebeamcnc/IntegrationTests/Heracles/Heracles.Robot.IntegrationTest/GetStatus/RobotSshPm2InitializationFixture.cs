using Heracles.Robot.IntegrationTest.TestUtils;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.IntegrationTest.GetStatus
{
    [SetUpFixture]
    internal class RobotSshPm2InitializationFixture
    {
        private const int FailureStatusCheckIntervalMilliseconds = 100;
        private const int MaxServerRestartTimeoutMilliseconds = 2 * 60 * 1000;


        public string Pm2Alias { get; protected set; }
        public string GoToScriptFolderCommand => $"cd {_workingFolder}";
        public string Pm2StopAllCommand => $"pm2 stop all";
        public string Pm2DeleteAlias => $"pm2 delete {Pm2Alias}";
        public string Pm2StartWithAliasCommand => $"pm2 start {_scriptToRun} -n {Pm2Alias}";
        
        private string? _workingFolder = null;
        private string? _scriptToRun = null;
        

        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            Debug.WriteLine("Start setting up robot status tests infrastructure");
            IConfiguration config = TestUtils.ConfigurationProvider.GetConfiguration();
            _workingFolder = config["AppSettings:EndPoints:RobotSshConnection:WorkingFolder"];
            _scriptToRun = config["AppSettings:EndPoints:RobotSshConnection:ScriptToRun"];
            
            var robotGrpcEndpoint = config["AppSettings:EndPoints:RobotGrpcServerEndPoint"];
            var RobotServiceUtils = new RobotServiceUtils(robotGrpcEndpoint);
            
            var SshUtils = new RobotSshUtils(config, RobotServiceUtils.RobotGrpcServerUri);

            Pm2Alias = SshUtils.Pm2Alias;

            Debug.WriteLine("Setup robot integration tests infrastructure");
            await SshUtils.ExecuteSshCommandAsync(Pm2StopAllCommand);
            await SshUtils.ExecuteSshCommandAsync(Pm2DeleteAlias);

            CancellationTokenSource tokenSource =
                TokenSourceFactory.CreateAutoCancellingTokenSource(MaxServerRestartTimeoutMilliseconds);

            bool shutdownResult = await RobotServiceUtils.WaitForServerStatus(
                RobotServiceUtils.RobotArmGrpcService,
                Status.RosClientFailure,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds
                );
            if (!shutdownResult)
            {
                throw new Exception("Cannot initialize Robot infrastructure: Robot server stop via PM2 failed.");
            }

            await SshUtils.ExecuteSshCommandAsync($"{GoToScriptFolderCommand} && {Pm2StartWithAliasCommand}");
            bool startupResult = await RobotServiceUtils.WaitForServerStatus(
                RobotServiceUtils.RobotArmGrpcService,
                Status.Unspecified,
                tokenSource.Token, pause_ms: FailureStatusCheckIntervalMilliseconds
                );
            if (!startupResult)
            {
                throw new Exception("Cannot initialize Robot infrastructure: Robot server start via PM2 failed.");
            }
            Debug.WriteLine("Setup for robot status tests complete");
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            Debug.WriteLine("TeadDown robot status tests infrastructure");
        }
    }
}
