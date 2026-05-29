using Prism.Mvvm;

using System;
using System.Collections.Generic;

using Xcc.Core.Constants;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public class SystemConfiguration : BindableBase, Core.Models.ISystemConfiguration
    {
        private ISystemEndPoint _gcbTelemetry = new SystemEndPoint(NetworkProperties.GCB_IPADDRESS + ":" + NetworkProperties.GCB_TELEMETRY_PORT);
        public ISystemEndPoint GcbTelemetry
        {
            get => _gcbTelemetry;
            set => SetProperty(ref _gcbTelemetry, value);
        }

        private ISystemEndPoint _qcbCommands = new SystemEndPoint(NetworkProperties.QCB_IPADDRESS + ":" + NetworkProperties.QCB_COMMANDS_PORT);
        public ISystemEndPoint QcbCommands
        {
            get => _qcbCommands;
            set => SetProperty(ref _qcbCommands, value);
        }

        private ISystemEndPoint _gcbCommands = new SystemEndPoint(NetworkProperties.GCB_IPADDRESS + ":" + NetworkProperties.GCB_COMMANDS_PORT);
        public ISystemEndPoint GcbCommands
        {
            get => _gcbCommands;
            set => SetProperty(ref _gcbCommands, value);
        }

        private ISystemEndPoint _headCam = new SystemEndPoint(NetworkProperties.HEAD_CAMERA_IPADDRESS + ":" + NetworkProperties.HEAD_CAMERA_PORT);
        public ISystemEndPoint HeadCam
        {
            get => _headCam;
            set => SetProperty(ref _headCam, value);
        }

        private ISystemEndPoint _console = new SystemEndPoint("13.14.15.16:31111");
        public ISystemEndPoint Console
        {
            get => _console;
            set => SetProperty(ref _console, value);
        }
        
        private ISystemEndPoint _emrServer = new SystemEndPoint(NetworkProperties.EMR_SERVICE_IPADDRESS + ":" + NetworkProperties.EMR_SERVICE_PORT);
        public ISystemEndPoint EmrServer
        {
            get => _emrServer;
            set => SetProperty(ref _emrServer, value);
        }
        
        public virtual string TargetPointsConfigurationPresetName() { return string.Empty; }

        public void SetProperties(IEnumerable<Tuple<string,ISystemEndPoint>> endPoints)
        {
            foreach (var ep in endPoints)
            {
                var property = this.GetType().GetProperty(ep.Item1);
                if (property == null)
                    continue;

                if (property.CanWrite && 
                    property.PropertyType == typeof(ISystemEndPoint))
                {
                    property.SetValue(this, ep.Item2);
                }
            }
        }
    }
}
