using System.Net;
using Empyrean.Common.Application.Models;
using Empyrean.Common.Core.Domain.DataManagement.Common;

namespace Empyrean.Common.Infra.Networking
{
    public interface ISystemEndPoint : IDirtyFlaggedBindableBase, IComparable, IEntry
    {
        public int? IpAddressPart1 { get; set; }
        public int? IpAddressPart2 { get; set; }
        public int? IpAddressPart3 { get; set; }
        public int? IpAddressPart4 { get; set; }
        public int? Port { get; set; }

        public string Ip();
        public string Address();
        public void IpAddressFromStringOrLocal(string ipAddress);
        public bool EndPointFromString(string endPoint);
        public void EndPointFromIpAddress(IPAddress address);
    }
}
