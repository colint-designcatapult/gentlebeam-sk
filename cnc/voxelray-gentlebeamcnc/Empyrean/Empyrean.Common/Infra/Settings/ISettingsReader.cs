
namespace Empyrean.Common.Infra.Settings
{
    public interface ISettingsReader
    {
        /// <summary>
        /// Gets requires value of type <b>int</b> from the configuration.
        /// </summary>
        /// <exception cref="SettingsConfigurationException">Thrown, when the key not found in the configuration or value can't be converted to int.</exception>
        int GetInt(string key);

        /// <summary>
        /// Gets required value of type <b>double</b> from the configuration.
        /// </summary>
        /// <exception cref="SettingsConfigurationException">Thrown, when the key not found in the configuration or value can't be converted to double.</exception>
        double GetDouble(string key);

        /// <summary>
        /// Gets required value of type <b>string</b> from the configuration.
        /// </summary>
        /// <exception cref="SettingsConfigurationException">Thrown, when the key not found in the configuration or value can't be converted to string.</exception>
        string GetString(string key);

        /// <summary>
        /// Gets required value of type <b>T</b> from the configuration.
        /// </summary>
        /// <exception cref="SettingsConfigurationException">Thrown, when the key not found in the configuration or value can't be converted to T.</exception>
        T GetValue<T>(string key);


        /// <summary>
        /// Gets <b>int</b> value from the configuration. Returned value can't be <b>null</b>.<br/>
        /// If the key is not found in the configuration, the default value will be returned.
        /// </summary>
        int GetOptionalInt(string key, int defaultValue);

        /// <summary>
        /// Gets <b>long</b> value from the configuration. Returned value can't be <b>null</b>.<br/>
        /// If the key is not found in the configuration, the default value will be returned.
        /// </summary>
        long GetOptionalLong(string key, long defaultValue);

        /// <summary>
        /// Gets <b>bool</b> value from the configuration. Returned value can't be <b>null</b>.<br/>
        /// If the key is not found in the configuration, the default value will be returned.
        /// </summary>
        bool GetOptionalBool(string key, bool defaultValue);

        /// <summary>
        /// Gets <b>double</b> value from the configuration. Returned value can't be <b>null</b>.<br/>
        /// If the key is not found in the configuration, the default value will be returned.
        /// </summary>
        double GetOptionalDouble(string key, double defaultValue);

        /// <summary>
        /// Gets <b>float</b> value from the configuration. Returned value can't be <b>null</b>.<br/>
        /// If the key is not found in the configuration, the default value will be returned.
        /// </summary>
        float GetOptionalFloat(string key, float defaultValue);

        /// <summary>
        /// Gets <b>string</b> value from the configuration. Returned value can't be <b>null</b>.<br/>
        /// If the key is not found in the configuration, the default value will be returned.
        /// </summary>
        string GetOptionalString(string key, string defaultValue);

        /// <summary>
        /// Gets <b>string?</b> value from the configuration. Returned value can be <b>null</b>.
        /// </summary>
        string? GetOptionalString(string key);

        /// <summary>
        /// Gets value of the type <b>T?</b> from the configuration. Returned value can be <b>null</b>.
        /// </summary>
        T? GetOptionalValue<T>(string key);
    }
}