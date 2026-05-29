namespace Xcc.Core.Models
{
    public interface IGCBPushButtons
    {
        public bool LaserToggle { get; }

        public bool LightingToggle { get; }

        public bool CameraToggle { get; }

        public bool Function1Toggle { get; }

        public bool Function2Toggle { get; }

        public bool reserved1 { get; }

        public bool reserved2 { get; }

        public bool reserved3 { get; }

        public bool reserved4 { get; }

        public bool reserved5 { get; }

        public bool reserved6 { get; }

        public bool reserved7 { get; }

        public bool reserved8 { get; }

        public bool reserved9 { get; }

        public bool reserved10 { get; }

        public bool reserved11 { get; }

        public bool reserved12 { get; }

        public bool LaserReleaseToggle { get; }

        public bool LightingReleaseToggle { get; }

        public bool CameraReleaseToggle { get; }

        public bool Function1ReleaseToggle { get; }

        public bool Function2ReleaseToggle { get; }
    }
}
