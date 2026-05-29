using Microsoft.Extensions.Configuration;
using Empyrean.Common.Infra.Settings;
using Moq;

namespace Empyrean.Common.Test.Infra.Settings
{
    public class SettingsReaderTests
    {
        private SettingsReader _settingsReader;
        private Dictionary<string, string> _configValues;

        [SetUp]
        public void Setup()
        {
            _configValues = new Dictionary<string, string>();
            SetupSettingsReader(); // With empty values
        }

        private void SetupSettingsReader()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(_configValues)
                .Build();
            _settingsReader = new SettingsReader(configuration);
        }

        [Test]
        public void GetInt()
        {
            _configValues["key"] = "42";
            SetupSettingsReader();

            int expectedValue = 42;

            int result = _settingsReader.GetInt("key");
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetInt_NotExists_Return_Default()
        {
            int result = _settingsReader.GetInt("UNKNOWN");
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void GetDouble()
        {
            _configValues["key"] = "42.2";
            SetupSettingsReader();

            double expectedValue = 42.2;

            double result = _settingsReader.GetDouble("key");
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetDouble_NotExists_Return_Default()
        {
            double result = _settingsReader.GetDouble("UNKNOWN");
            Assert.That(result, Is.EqualTo(0.0));
        }

        [Test]
        public void GetString()
        {
            _configValues["key"] = "test";
            SetupSettingsReader();

            string expectedValue = "test";

            string result = _settingsReader.GetString("key");
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetString_NotExists_Throws()
        {
            var exception = Assert.Throws<SettingsConfigurationException>(() => _settingsReader.GetString("UNKNOWN"));
            Assert.That(exception.Message, Contains.Substring("Failed to parse UNKNOWN"));
        }

        [Test]
        public void GetOptionalInt()
        {
            _configValues["key"] = "42";
            SetupSettingsReader();

            int expectedValue = 42;

            int result = _settingsReader.GetOptionalInt("key", 35);
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOptionalInt_NotExists_Return_Default()
        {
            int result = _settingsReader.GetOptionalInt("UNKNOWN", 35);
            Assert.That(result, Is.EqualTo(35));
        }

        [Test]
        public void GetOptionalLong()
        {
            _configValues["key"] = "42";
            _configValues["key2"] = "99999999999";
            SetupSettingsReader();

            {
                long expectedValue = 42;

                long result = _settingsReader.GetOptionalLong("key", 99999999999000);
                Assert.That(result, Is.EqualTo(expectedValue));
            }

            {
                long expectedValue = 99999999999;

                long result = _settingsReader.GetOptionalLong("key2", 99999999999000);
                Assert.That(result, Is.EqualTo(expectedValue));
            }
        }

        [Test]
        public void GetOptionalLong_NotExists_Return_Default()
        {
            long result = _settingsReader.GetOptionalLong("UNKNOWN", 99999999999000);
            Assert.That(result, Is.EqualTo(99999999999000));
        }

        [Test]
        public void GetOptionalBool()
        {
            _configValues["key"] = "true";
            _configValues["key2"] = "false";
            SetupSettingsReader();

            {
                bool result = _settingsReader.GetOptionalBool("key", false);
                Assert.That(result, Is.True);
            }
            {
                bool result = _settingsReader.GetOptionalBool("key2", false);
                Assert.That(result, Is.False);
            }
        }

        [Test]
        public void GetOptionalBool_NotExists_Return_Default()
        {
            bool result = _settingsReader.GetOptionalBool("UNKNOWN", true);
            Assert.That(result, Is.True);
        }

        [Test]
        public void GetOptionalDouble()
        {
            _configValues["key"] = "42.2";
            SetupSettingsReader();

            double expectedValue = 42.2;
            double result = _settingsReader.GetOptionalDouble("key", 333.333);
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOptionalDouble_NotExists_Return_Default()
        {
            double result = _settingsReader.GetOptionalDouble("UNKNOWN", 333.333);
            Assert.That(result, Is.EqualTo(333.333));
        }

        [Test]
        public void GetOptionalFloat()
        {
            _configValues["key"] = "42.2";
            SetupSettingsReader();

            float expectedValue = 42.2f;
            float result = _settingsReader.GetOptionalFloat("key", 22.2f);
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOptionalFloat_NotExists_Return_Default()
        {
            double result = _settingsReader.GetOptionalFloat("UNKNOWN", 22.22f);
            Assert.That(result, Is.EqualTo(22.22f));
        }

        [Test]
        public void GetOptionalString()
        {
            _configValues["key"] = "test";
            SetupSettingsReader();

            string expectedValue = "test";
            string result = _settingsReader.GetOptionalString("key", "default_value");
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOptionalString_NotExists_Return_Default()
        {
            string result = _settingsReader.GetOptionalString("UNKNOWN", "default_value");
            Assert.That(result, Is.EqualTo("default_value"));
        }

        [Test]
        public void GetOptionalString_2()
        {
            _configValues["key"] = "test";
            SetupSettingsReader();

            string expectedValue = "test";
            string? result = _settingsReader.GetOptionalString("key");
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOptionalString_2_NotExists_Return_Default()
        {
            string? result = _settingsReader.GetOptionalString("UNKNOWN");
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void GetOptionalValue()
        {
            _configValues["key"] = "test";
            SetupSettingsReader();

            string expectedValue = "test";
            string? result = _settingsReader.GetOptionalValue<string>("key");
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOptionalValue_NotExists_Return_Default()
        {
            string? result = _settingsReader.GetOptionalValue<string>("UNKNOWN");
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void GetOptionalValue_WhenException_Return_Default()
        {
            _configValues["key"] = "test";
            SetupSettingsReader();

            int result = _settingsReader.GetOptionalValue<int>("key");
            Assert.That(result, Is.EqualTo(0));
        }
    }
}