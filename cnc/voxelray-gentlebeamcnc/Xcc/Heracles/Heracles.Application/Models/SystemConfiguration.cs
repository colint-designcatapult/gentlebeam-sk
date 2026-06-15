using Heracles.Core.Constants;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Prism.Events;
using Xcc.Application.Models;

namespace Heracles.Application.Models
{
    public class SystemConfiguration : Xcc.Application.Models.SystemConfiguration, Heracles.Core.Models.ISystemConfiguration
    {
        private XRayHeadConfigurationMode _xRayHeadConfiguration;
        public XRayHeadConfigurationMode XRayHeadConfiguration
        {
            get { return _xRayHeadConfiguration; }
            set
            {
                if (_xRayHeadConfiguration != value)
                {
                    SetProperty(ref _xRayHeadConfiguration, value);
                }
            }
        }

        public SystemConfiguration(IHeraclesMainSettings heraclesMainSettings)
        {
            XRayHeadConfiguration = XRayHeadConfigurationMode.SixtyOne;

            EmrServer = heraclesMainSettings.DataCommandsEndPoint ??
                new SystemEndPoint(Xcc.Core.Constants.NetworkProperties.EMR_SERVICE_IPADDRESS + ":" + NetworkProperties.EMR_SERVICE_PORT);

            GcbCommands = heraclesMainSettings.GCBCommandsEndPoint ??
                new SystemEndPoint(NetworkProperties.GCB_IPADDRESS + ":" + NetworkProperties.GCB_COMMANDS_PORT);

            GcbTelemetry = heraclesMainSettings.GCBTelemetryEndPoint ??
                new SystemEndPoint(NetworkProperties.GCB_IPADDRESS + ":" + NetworkProperties.GCB_TELEMETRY_PORT);

            //HeadCam = new SystemEndPoint(NetworkProperties.HEAD_CAMERA_IPADDRESS + ":" + NetworkProperties.HEAD_CAMERA_PORT);
            //Console = new SystemEndPoint(13, 14, 15, 16, 31111);

            //ExternAppSlaveListener = new SystemEndPoint(NetworkProperties.EXTERN_APP_SLAVE_IPADDRESS + ":" + NetworkProperties.EXTERN_APP_SLAVE_PORT);
            //ExternAppMasterListener = new SystemEndPoint(NetworkProperties.MCC_IPADDRESS + ":" + NetworkProperties.EXTERN_APP_MASTER_PORT);
        }

        public override string TargetPointsConfigurationPresetName()
        {
            string name = string.Empty;

            if (XRayHeadConfiguration == XRayHeadConfigurationMode.SixtyOne)
                name = TargetPointsConfigurationPresetNames.TARGET_CONFIG_PRESET_61;
            else if (XRayHeadConfiguration == XRayHeadConfigurationMode.Seven)
                name = TargetPointsConfigurationPresetNames.TARGET_CONFIG_PRESET_7;

            return name;
        }

        #region private methods


        #endregion

    }
}
