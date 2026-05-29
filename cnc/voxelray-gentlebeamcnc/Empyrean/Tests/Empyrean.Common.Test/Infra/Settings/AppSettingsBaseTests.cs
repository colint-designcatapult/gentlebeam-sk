using Empyrean.Common.Infra.Settings;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Empyrean.Common.Test.Infra.Settings
{
    internal class AppSettingsBaseTests
    {
        private IConfiguration _configuration;
        private Dictionary<string, string> _configValues;

        [SetUp]
        public void Setup()
        {
            _configValues = new Dictionary<string, string>();
            SetupConfiguration(); // With empty values
        }

        private void SetupConfiguration()
        {
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(_configValues)
                .Build();
        }
        
        [Test]
        public void Constructor_ThrowsException_WhenMissed_LogFilename(
            [Values(false, true)] bool isExistsLogPageSize)
        {
            _configValues = new Dictionary<string, string>();
            if (isExistsLogPageSize)
                _configValues.Add("AppSettings:LogPageSize", "100");
            SetupConfiguration();
            
            var exception = Assert.Throws<SettingsConfigurationException>(() => new AppSettingsBase(_configuration));
            Assert.That(exception.Message, Contains.Substring("Failed to parse AppSettings:LogFilename")); 
        }

        [Test]
        public void Constructor_NotThrow_WhenMissed_LogPageSize(
            [Values(false, true)] bool isExistsLogPageSize)
        {
            _configValues = new Dictionary<string, string>
            {
                { "AppSettings:LogFilename", "app.log" }
            };
            SetupConfiguration();

            AppSettingsBase? settings = null;
            Assert.DoesNotThrow(() => settings = new AppSettingsBase(_configuration));

            Assert.Multiple(() =>
            {
                Assert.That(settings?.LogFilename, Is.EqualTo("app.log"));
                Assert.That(settings?.LogPageSize, Is.EqualTo(0));
                Assert.That(settings?.AppLogFolder, Contains.Substring("XccLogs"));
            });
        }
        
        [Test]
        public void Constructor_WithSomeValues()
        {
            _configValues = new Dictionary<string, string>
            {
                { "AppSettings:LogFilename", "app.log" },
                { "AppSettings:LogPageSize", "100" }
            };
            SetupConfiguration();
            
            var settings = new AppSettingsBase(_configuration);

            Assert.That(settings.LogFilename, Is.EqualTo("app.log"));
            Assert.That(settings.LogPageSize, Is.EqualTo(100));
            Assert.That(settings.AppLogFolder, Contains.Substring("XccLogs"));
        }
    }
}