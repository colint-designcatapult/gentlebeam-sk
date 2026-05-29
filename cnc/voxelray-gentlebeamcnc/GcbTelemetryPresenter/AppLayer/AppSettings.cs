using Empyrean.Common.Infra.Settings;

namespace GcbTelemetryPresenter.AppLayer;

internal interface IAppSettings
{ 
    string TelemetryFolder { get; }
}

internal class AppSettings(ISettingsReader reader) : IAppSettings
{
    public string TelemetryFolder { get; init; } = reader.GetString("AppSettings:TelemetryFolder");
}