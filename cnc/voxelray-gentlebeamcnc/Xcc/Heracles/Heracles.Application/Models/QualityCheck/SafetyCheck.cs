using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;
using Xcc.Application.Domain.QualityAssurance;

namespace Heracles.Application.Models.QualityCheck
{
    public class SafetyCheck : BaseQaEntry, ISafetyCheck
    {
        public static readonly Energy SupportedEnergy = Energy.Energy_50; // todo: should it really be constant?
        public SafetyCheck()
        {            
            Energy = SupportedEnergy;
        }

        public Energy Energy { set; get; }
        public float Dose { get; set; } = 0.0f;
        public bool XRayLight { get; set; } = false;
        public bool XRaySound { get; set; } = false;
        public bool DoorInterlock { get; set; } = false;
        public bool EStop { get; set; } = false;
        public bool SStop { get; set; } = false;
        public bool LiveVideo { get; set; } = false;
        public bool LiveAudio { get; set; } = false;
    }
}
