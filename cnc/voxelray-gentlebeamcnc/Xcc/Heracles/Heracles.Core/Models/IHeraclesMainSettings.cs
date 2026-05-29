using Empyrean.Common.Infra.Settings;

using System.Net.NetworkInformation;

using Xcc.Core.Models;

namespace Heracles.Core.Models
{
    public interface IImagingSettings
    {
        public ISystemEndPoint ImagingEndpoint { get; }
        public string? PathToDeepColorApp { get; }
        public int HttpRequestTimeout { get; }
    }


    public interface IHeraclesCoreSettings : ICoreSettings
    {
        public ISystemEndPoint RobotGrpcServerEndPoint { set; get; }
        public ISystemEndPoint PhotoAcousticEndPoint { get; }
        public ISystemEndPoint AcbCommandsEndPoint { get; set; }
    }

    public interface IHeraclesMainSettings : ITextLogSettings, IHeraclesCoreSettings, IXRaySettings, IAcbSettings, IDebugSettings, IImagingSettings
    {
        public bool ImagingEmulator { get; }
        public ISystemEndPoint RobotGrpcServerEndPoint { set; get; }
        public Uri RobotGrpcServerUri { get; }

        // RobotGrpcServerMac was needed for WakeOnLan service that we don't utilize anymore
        [Obsolete]
        public PhysicalAddress RobotGrpcServerMac { get; }
        
        public double RobotSafeZoneThresholdZmm { set; get; }
        public double RobotSafeZoneThresholdYmm { set; get; }
        
        public ISystemEndPoint PhotoAcousticEndPoint { get; }
    }

    public interface IHeraclesExternalSettings : ITextLogSettings, IHeraclesCoreSettings, IXRaySettings, IWarmUpSettings, IDebugSettings
    {
    }
}
