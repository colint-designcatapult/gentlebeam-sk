using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Empyrean.Common.Application.Common;
using Empyrean.Common.Application.Models;
using Empyrean.Common.Core.Domain.DataManagement.Common;

namespace Empyrean.Common.Infra.Networking
{
    public class SystemEndPoint : DirtyFlaggedBindableBase, ISystemEndPoint
    {
        #region Constructors
        public SystemEndPoint(long entryId, string endPoint) : this(endPoint)
        {
            Id = entryId;
        }

        public SystemEndPoint(ISystemEndPoint ep)
        {
            IpAddressPart1 = ep.IpAddressPart1;
            IpAddressPart2 = ep.IpAddressPart2;
            IpAddressPart3 = ep.IpAddressPart3;
            IpAddressPart4 = ep.IpAddressPart4;
            Port = ep.Port;
            IsModified = false;
        }

        public SystemEndPoint(string? endPoint)
        {
            if (endPoint == null)
                return;

            int ipAddressLen = endPoint.IndexOf(":", StringComparison.Ordinal);

            if (ipAddressLen > 0 && ipAddressLen + 1 < endPoint.Length)
            {
                int.TryParse(endPoint.Substring(ipAddressLen + 1), out int port);

                Port = port;
            }

            if (ipAddressLen < 0)
                ipAddressLen = endPoint.Length;

            Byte[] address = IPAddress.Parse(endPoint.Substring(0, ipAddressLen)).GetAddressBytes();

            IpAddressPart1 = (int)(uint)address[0];
            IpAddressPart2 = (int)(uint)address[1];
            IpAddressPart3 = (int)(uint)address[2];
            IpAddressPart4 = (int)(uint)address[3];
            IsModified = false;
        }

        public SystemEndPoint(string ip, string port)
        {
            IpAddressFromStringOrLocal(ip);

            int parsedPort;
            if (Int32.TryParse(port, out parsedPort))
                Port = parsedPort;
            else
                Port = 0; // todo: check it
        }
        #endregion

        #region Properties
        public static SystemEndPoint LocalHost => new("127.0.0.1");

        public long Id { get; set; } = BaseEntry.NewEntryId;

        private int? _iPAddressPart1;
        [Required]
        [NumericRange(byte.MinValue, byte.MaxValue)]
        public int? IpAddressPart1
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
        public int? IpAddressPart2
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
        public int? IpAddressPart3
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
        public int? IpAddressPart4
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
            var sb = new StringBuilder(IpAddressPart1.ToString());
            sb.Append(".");
            sb.Append(IpAddressPart2.ToString());
            sb.Append(".");
            sb.Append(IpAddressPart3.ToString());
            sb.Append(".");
            sb.Append(IpAddressPart4.ToString());

            return sb.ToString();
        }

        public string Address()
        {
            var sb = new StringBuilder(Ip());
            sb.Append(":");
            sb.Append(Port.ToString());

            return sb.ToString();
        }

        public void IpAddressFromStringOrLocal(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
                Byte[] address = IPAddress.Parse(ipAddress).GetAddressBytes();

                IpAddressPart1 = (int)(uint)address[0];
                IpAddressPart2 = (int)(uint)address[1];
                IpAddressPart3 = (int)(uint)address[2];
                IpAddressPart4 = (int)(uint)address[3];
            }
            else
            {
                IpAddressPart1 = 127;
                IpAddressPart2 = 0;
                IpAddressPart3 = 0;
                IpAddressPart4 = 1;
            }
        }

        public bool EndPointFromString(string endPoint)
        {
            Byte[] address = IPAddress.Parse(endPoint.Substring(0, endPoint.IndexOf(":"))).GetAddressBytes();

            IpAddressPart1 = (int)(uint)address[0];
            IpAddressPart2 = (int)(uint)address[1];
            IpAddressPart3 = (int)(uint)address[2];
            IpAddressPart4 = (int)(uint)address[3];

            return int.TryParse(endPoint.Substring(endPoint.IndexOf(":") + 1), out int Port);
        }

        public void EndPointFromIpAddress(IPAddress? address)
        {
            if (address != null)
            {
                IpAddressPart1 = address.GetAddressBytes()[0];
                IpAddressPart2 = address.GetAddressBytes()[1];
                IpAddressPart3 = address.GetAddressBytes()[2];
                IpAddressPart4 = address.GetAddressBytes()[3];
            }
        }

        public int CompareTo(object? obj)
        {
            ISystemEndPoint? endPoint = obj as ISystemEndPoint;

            return endPoint?.IpAddressPart1 == IpAddressPart1 &&
                   endPoint?.IpAddressPart2 == IpAddressPart2 &&
                   endPoint?.IpAddressPart3 == IpAddressPart3 &&
                   endPoint?.IpAddressPart4 == IpAddressPart4 &&
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
