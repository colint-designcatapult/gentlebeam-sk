using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Infra.Networking
    /// </summary>
    [Obsolete]
    public class SystemEndPoint : DirtyFlaggedBindableBase, ISystemEndPoint
    {
        #region Constructors
        public SystemEndPoint(long entryId, string endPoint) : this(endPoint)
        {
            Id = entryId;
        }

        public SystemEndPoint(ISystemEndPoint ep)
        {
            IPAddressPart1 = ep.IPAddressPart1;
            IPAddressPart2 = ep.IPAddressPart2;
            IPAddressPart3 = ep.IPAddressPart3;
            IPAddressPart4 = ep.IPAddressPart4;
            Port = ep.Port;
            IsModified = false;
        }

        public SystemEndPoint(string endPoint)
        {
            if (endPoint == null)
                return;

            int ip_address_len = endPoint.IndexOf(":", StringComparison.Ordinal);

            if (ip_address_len > 0 && ip_address_len + 1 < endPoint.Length)
            {
                int.TryParse(endPoint.Substring(ip_address_len + 1), out int port);

                Port = port;
            }

            if (ip_address_len < 0)
                ip_address_len = endPoint.Length;

            Byte[] address = IPAddress.Parse(endPoint.Substring(0, ip_address_len)).GetAddressBytes();

            IPAddressPart1 = (int)(uint)address[0];
            IPAddressPart2 = (int)(uint)address[1];
            IPAddressPart3 = (int)(uint)address[2];
            IPAddressPart4 = (int)(uint)address[3];
            IsModified = false;
        }

        public SystemEndPoint(string ip, string port)
        {
            IPAddressFromStringOrLocal(ip);

            int parsedPort;
            if (Int32.TryParse(port, out parsedPort))
                Port = parsedPort;
            else
                Port = 0; // todo: check it
        }
        #endregion

        #region Properties
        public static SystemEndPoint LocalHost => new("127.0.0.1");

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        private int? _iPAddressPart1;
        [Required]
        [NumericRange(byte.MinValue, byte.MaxValue)]
        public int? IPAddressPart1
        {
            get => _iPAddressPart1;
            set
            {
                SetPropertyWithDirtyFlag(ref _iPAddressPart1, value);
                Validate(value);
            }
        }

        private int? _iPAddressPart2;
        [Required]
        [NumericRange(byte.MinValue, byte.MaxValue)]
        public int? IPAddressPart2
        {
            get => _iPAddressPart2;
            set
            {
                SetPropertyWithDirtyFlag(ref _iPAddressPart2, value);
                Validate(value);
            }
        }

        private int? _iPAddressPart3;
        [Required]
        [NumericRange(byte.MinValue, byte.MaxValue)]
        public int? IPAddressPart3
        {
            get => _iPAddressPart3;
            set
            {
                SetPropertyWithDirtyFlag(ref _iPAddressPart3, value);
                Validate(value);
            }
        }

        private int? _iPAddressPart4;
        [Required]
        [NumericRange(byte.MinValue, byte.MaxValue)]
        public int? IPAddressPart4
        {
            get => _iPAddressPart4;
            set
            {
                SetPropertyWithDirtyFlag(ref _iPAddressPart4, value);
                Validate(value);
            }
        }

        private int? _port;
        [Required]
        [NumericRange(ushort.MinValue, ushort.MaxValue)]
        public int? Port
        {
            get => _port;
            set
            {
                SetPropertyWithDirtyFlag(ref _port, value);
                Validate(value);
            }
        }

        #endregion
        
        public string Ip()
        {
            var sb = new StringBuilder(IPAddressPart1.ToString());
            sb.Append(".");
            sb.Append(IPAddressPart2.ToString());
            sb.Append(".");
            sb.Append(IPAddressPart3.ToString());
            sb.Append(".");
            sb.Append(IPAddressPart4.ToString());

            return sb.ToString();
        }

        public string Address()
        {
            var sb = new StringBuilder(Ip());
            sb.Append(":");
            sb.Append(Port.ToString());

            return sb.ToString();
        }

        public void IPAddressFromStringOrLocal(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
                Byte[] address = IPAddress.Parse(ipAddress).GetAddressBytes();

                IPAddressPart1 = (int)(uint)address[0];
                IPAddressPart2 = (int)(uint)address[1];
                IPAddressPart3 = (int)(uint)address[2];
                IPAddressPart4 = (int)(uint)address[3];
            }
            else
            {
                IPAddressPart1 = 127;
                IPAddressPart2 = 0;
                IPAddressPart3 = 0;
                IPAddressPart4 = 1;
            }
        }

        public bool EndPointFromString(string endPoint)
        {
            Byte[] address = IPAddress.Parse(endPoint.Substring(0, endPoint.IndexOf(":"))).GetAddressBytes();

            IPAddressPart1 = (int)(uint)address[0];
            IPAddressPart2 = (int)(uint)address[1];
            IPAddressPart3 = (int)(uint)address[2];
            IPAddressPart4 = (int)(uint)address[3];

            return int.TryParse(endPoint.Substring(endPoint.IndexOf(":") + 1), out int Port);
        }

        public void EndPointFromIPAddress(IPAddress? address)
        {
            if (address != null)
            {
                IPAddressPart1 = address.GetAddressBytes()[0];
                IPAddressPart2 = address.GetAddressBytes()[1];
                IPAddressPart3 = address.GetAddressBytes()[2];
                IPAddressPart4 = address.GetAddressBytes()[3];
            }
        }

        public int CompareTo(object? obj)
        {
            ISystemEndPoint? endPoint = obj as ISystemEndPoint;

            return endPoint?.IPAddressPart1 == IPAddressPart1 &&
                   endPoint?.IPAddressPart2 == IPAddressPart2 &&
                   endPoint?.IPAddressPart3 == IPAddressPart3 &&
                   endPoint?.IPAddressPart4 == IPAddressPart4 &&
                   endPoint?.Port == Port ? 0 : -1;
        }

        public override bool Equals(object? obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return Address().GetHashCode();
        }



        public static SystemEndPoint Create(string endPoint)
        {
            return new SystemEndPoint(endPoint);
        }
    }
}
