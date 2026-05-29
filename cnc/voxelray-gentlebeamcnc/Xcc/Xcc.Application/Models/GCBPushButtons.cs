using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public class GCBPushButtons : IGCBPushButtons
    {
        public bool LaserToggle { get; private set; }

        public bool LightingToggle { get; private set; }

        public bool CameraToggle { get; private set; }

        public bool Function1Toggle { get; private set; }

        public bool Function2Toggle { get; private set; }

        public bool reserved1 { get; private set; }

        public bool reserved2 { get; private set; }

        public bool reserved3 { get; private set; }

        public bool reserved4 { get; private set; }

        public bool reserved5 { get; private set; }

        public bool reserved6 { get; private set; }

        public bool reserved7 { get; private set; }

        public bool reserved8 { get; private set; }

        public bool reserved9 { get; private set; }

        public bool reserved10 { get; private set; }

        public bool reserved11 { get; private set; }

        public bool reserved12 { get; private set; }

        public bool LaserReleaseToggle { get; private set; }

        public bool LightingReleaseToggle { get; private set; }

        public bool CameraReleaseToggle { get; private set; }

        public bool Function1ReleaseToggle { get; private set; }

        public bool Function2ReleaseToggle { get; private set; }
    }
}
