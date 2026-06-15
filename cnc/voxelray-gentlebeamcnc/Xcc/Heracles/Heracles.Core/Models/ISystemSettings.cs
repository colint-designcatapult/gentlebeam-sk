using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models
{
    public interface ISystemSettings : IEntry
    {
        IEndPointsConfiguration EndPointsConfiguration { get; set; }
        string DeviceSerial { get; set; }
    }
}
