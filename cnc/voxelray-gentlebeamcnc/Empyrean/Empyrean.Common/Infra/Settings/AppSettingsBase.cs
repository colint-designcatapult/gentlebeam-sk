using Microsoft.Extensions.Configuration;

namespace Empyrean.Common.Infra.Settings
{
    public class AppSettingsBase : ITextLogSettings
    {
        public AppSettingsBase(IConfiguration configuration)
        {
            SettingsReader reader = new SettingsReader(configuration);

            LogFilename = reader.GetString("AppSettings:LogFilename");
            LogPageSize = reader.GetInt("AppSettings:LogPageSize");
            AppLogFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "XccLogs");
        }

        public string LogFilename { get; }
        public int LogPageSize { get; }
        public string AppLogFolder { get; }
    }
}
