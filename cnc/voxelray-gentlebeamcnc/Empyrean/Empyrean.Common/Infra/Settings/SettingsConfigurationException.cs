namespace Empyrean.Common.Infra.Settings;

public class SettingsConfigurationException : Exception
{
    public SettingsConfigurationException(string message) : base(message) { }
    public SettingsConfigurationException(string message, Exception inner)
        : base(message, inner) { }
}