using System;
using System.Net;
using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Core.Models
{
    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Infra.Networking
    /// </summary>
    [Obsolete]
    public interface ISystemEndPoint : IDirtyFlaggedBindableBase, IComparable, IEntry
    {
        public int? IPAddressPart1 { get; set; }
        public int? IPAddressPart2 { get; set; }
        public int? IPAddressPart3 { get; set; }
        public int? IPAddressPart4 { get; set; }
        public int? Port { get; set; }

        public string Ip();
        public string Address();
        public void IPAddressFromStringOrLocal(string ipAddress);
        public bool EndPointFromString(string endPoint);
        public void EndPointFromIPAddress(IPAddress address);
    }
}


