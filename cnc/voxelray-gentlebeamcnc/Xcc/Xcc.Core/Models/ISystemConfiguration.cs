
using System;
using System.Collections.Generic;

namespace Xcc.Core.Models
{
    public interface ISystemConfiguration
    {
        public ISystemEndPoint EmrServer { get; set; }
        public ISystemEndPoint QcbCommands { get; set; }
        public ISystemEndPoint GcbCommands { get; set; }
        public ISystemEndPoint GcbTelemetry { get; set; }

        public string TargetPointsConfigurationPresetName();
        void SetProperties(IEnumerable<Tuple<string,ISystemEndPoint>> endPoints);
    }
}
