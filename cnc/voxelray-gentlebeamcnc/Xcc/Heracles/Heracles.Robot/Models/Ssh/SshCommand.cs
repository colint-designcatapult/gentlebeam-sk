using Renci.SshNet;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Ssh
{

    public class ConnectionInfo
    {
        public string Host;
        public int Port;
        public string User;
        public string Password;
    }

    public class Command
    {
        #region Constructors
        public Command(ConnectionInfo connectionInfo, string commandString, ILogWriter logWriter)
        {
            _commandString = commandString;
            _logWriter = logWriter;
            _connectionInfo = connectionInfo;
        }
        #endregion Constructors
        #region Properties
        string _commandString;
        ILogWriter _logWriter;
        ConnectionInfo _connectionInfo;
        #endregion Properties
        #region Events
        public event EventHandler<string> OnNewLine;
        public event EventHandler<int?> OnFinish;
        #endregion Events
        #region Methods
        public async Task ExecuteAsync()
        {
            try
            {
                using (var client = new SshClient(_connectionInfo.Host, _connectionInfo.Port, _connectionInfo.User, _connectionInfo.Password))
                {
                    client.Connect();

                    using (SshCommand command = client.CreateCommand(_commandString))
                    {
                        Task executeTask = command.ExecuteAsync(CancellationToken.None);

                        using (StreamReader sr = new StreamReader(command.OutputStream))
                        {
                            while (!sr.EndOfStream)
                            {
                                var line = await sr.ReadLineAsync();
                                OnNewLine?.Invoke(this, line);
                            }
                        }

                        await executeTask;

                        OnFinish?.Invoke(this, command.ExitStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logWriter.LogAsync($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                OnFinish?.Invoke(this, null);
            }
        }
        #endregion Methods

    }
}
