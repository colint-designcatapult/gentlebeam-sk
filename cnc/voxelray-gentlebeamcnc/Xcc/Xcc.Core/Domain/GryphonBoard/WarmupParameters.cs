using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct WarmupParameters
    {
        public readonly WarmupType WarmupType;
        public readonly float HeaterCurrentSetpoint;
        public readonly long ActiveHeadId;

        private WarmupParameters(WarmupType type, float heaterCurrentSetpoint, long activeHeadId = 0)
        {
            WarmupType = type;
            HeaterCurrentSetpoint = heaterCurrentSetpoint;
            ActiveHeadId = activeHeadId;
        }

        public static WarmupParameters Conditioning(float heaterCurrentSetpoint, long activeHeadId = 0)
        {
            return new WarmupParameters(WarmupType.Full, heaterCurrentSetpoint, activeHeadId);
        }

        public static WarmupParameters FastWarmup(float heaterCurrentSetpoint, long activeHeadId = 0)
        {
            return new WarmupParameters(WarmupType.Fast, heaterCurrentSetpoint, activeHeadId);
        }
    }
}