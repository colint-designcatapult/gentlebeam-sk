using Heracles.Core.Models;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.Settings
{
    public class SystemSettings : ISystemSettings
    {
        public IEndPointsConfiguration EndPointsConfiguration { get; set; } = new EndPointsConfiguration();

        public string DeviceSerial { get; set; } = string.Empty;
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public SystemSettings()
        { }

        public SystemSettings(ISystemSettings settings)
        {
            Id = settings.Id;
            DeviceSerial = settings.DeviceSerial;
            if (settings.EndPointsConfiguration != null)
            {
                EndPointsConfiguration = new EndPointsConfiguration(settings.EndPointsConfiguration);
            }
        }

    }
}
