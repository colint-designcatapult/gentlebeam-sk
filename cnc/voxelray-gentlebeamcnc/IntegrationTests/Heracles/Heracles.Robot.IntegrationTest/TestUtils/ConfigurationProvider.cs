using Microsoft.Extensions.Configuration;

namespace Heracles.Robot.IntegrationTest.TestUtils
{
    public class ConfigurationProvider
    {
        public static IConfiguration GetConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }
    }
}
