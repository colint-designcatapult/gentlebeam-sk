using System.IO;
using Empyrean.Common.Infra.Settings;
using Xcc.Core.Logging;
using Xcc.Infra.Networking.Udp;

namespace Xcc.Infra.GryphonBoard.Comm.Udp
{
    public class GcbTelemetryConnectionLoggingProxy : UdpConnectionLoggingProxy, IGcbTelemetryConnection
    {
        public GcbTelemetryConnectionLoggingProxy(
            GcbTelemetryConnection connection,
            ILogRepository logWriter,
            ITextLogSettings textLogSettings)
            : base(connection,
                  logWriter,
                  LoggedPackets.All,
                  GetLogSubfolder(textLogSettings.AppLogFolder),
                  filenamePrefix: LogSubfolder,
                  timeoutMs: 0,
                  newFileEvery: NewFileEvery.Day)
        {
        }

        private static string GetLogSubfolder(string appLogFolder)
        {
            return Path.Join(appLogFolder, LogSubfolder);
        }

        public const string LogSubfolder = "GcbTelemetryLogs";
    }

}
