using Microsoft.Extensions.Configuration;
using Moq;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Robot.IntegrationTest.TestUtils
{
    class RobotSshUtils
    {
        #region Fields
        #region Constants
        protected const string Pm2RosProcessDefaultAlias = "HeraclesFake";
        protected const string KillServerCommand = "pkill -9 kuka_linux_serv";
        protected const string KillClientCommand = "pkill -9 grpc_server";
        #endregion Constants

        private Models.Ssh.ConnectionInfo _connectionInfo = new()
        {
            Host = "192.168.56.1",
            Port = 22,
            User = "user",
            Password = "123456"
        };
        private Mock<ILogWriter> _logServiceMock;

        #endregion Fields
        #region Properties
        public string Pm2Alias { get; protected set; } = Pm2RosProcessDefaultAlias;
        public string Pm2StopCommand => $"pm2 stop {Pm2Alias}";
        public string Pm2RestartCommand => $"pm2 restart {Pm2Alias}";
        protected ILogWriter LogService => _logServiceMock.Object;
        #endregion

        public RobotSshUtils(IConfiguration config, Uri robotServerUri)
        {
            var host = config["AppSettings:EndPoints:RobotSshConnection:Host"] ?? robotServerUri.Host;
            var portStr = config["AppSettings:EndPoints:RobotSshConnection:Port"];
            var port = (portStr != null) ? int.Parse(portStr) : robotServerUri.Port;
            var user = config["AppSettings:EndPoints:RobotSshConnection:Username"];
            var password = config["AppSettings:EndPoints:RobotSshConnection:Password"];
            _connectionInfo = new Models.Ssh.ConnectionInfo
            {
                Host = host,
                Port = port,
                User = user,
                Password = password
            };
            Pm2Alias = config["AppSettings:EndPoints:RobotSshConnection:Pm2Alias"] ?? Pm2Alias;
            _logServiceMock = new Mock<ILogWriter>();
        }

        public Task ExecuteSshCommandAsync(string command)
        {
            var cmd = GetRobotSshCommand(command);
            return cmd.ExecuteAsync();
        }

        public Task KillServerAsync()
        {
            return ExecuteSshCommandAsync(KillServerCommand);
        }

        public Task KillClientAsync()
        {
            return ExecuteSshCommandAsync(KillClientCommand);
        }

        public Task RestartServerAsync()
        {
            return ExecuteSshCommandAsync(Pm2RestartCommand);
        }

        private Models.Ssh.Command GetRobotSshCommand(string commandString)
        {
            return new Models.Ssh.Command(_connectionInfo, commandString, LogService);
        }
    }
}
