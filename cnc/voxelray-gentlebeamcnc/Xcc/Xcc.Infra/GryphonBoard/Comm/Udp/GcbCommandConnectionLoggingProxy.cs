using System.IO;
using Empyrean.Common.Infra.Settings;
using Xcc.Core.Logging;
using Xcc.Infra.Networking.Udp;

namespace Xcc.Infra.GryphonBoard.Comm.Udp
{
    public class GcbCommandConnectionLoggingProxy : UdpConnectionLoggingProxy, IGcbCommandConnection
    {
        public GcbCommandConnectionLoggingProxy(
            GcbCommandConnection connection,
            ILogRepository logWriter,
            ITextLogSettings logSettings)
            : base(connection,
                  logWriter,
                  LoggedPackets.All,
                  GetLogSubfolder(logSettings.AppLogFolder),
                  filenamePrefix: LogSubfolder,
                  timeoutMs: 0,
                  newFileEvery: NewFileEvery.Day)
        {
        }

        private static string GetLogSubfolder(string appLogFolder)
        {
            return Path.Join(appLogFolder, LogSubfolder);
        }

        public const string LogSubfolder = "GcbCommandLogs";
    }

}
