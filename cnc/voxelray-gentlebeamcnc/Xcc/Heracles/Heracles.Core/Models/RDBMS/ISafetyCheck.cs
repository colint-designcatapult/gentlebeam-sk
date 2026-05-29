using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;

namespace Heracles.Core.Models.RDBMS
{
    public interface ISafetyCheck : IQaEntryBase
    {
        Energy Energy { get; set; }
        float Dose { get; set; }
        bool XRayLight { get; set; }
        bool XRaySound { get; set; }
        bool DoorInterlock { get; set; }
        bool EStop { get; set; }
        bool SStop { get; set; }
        bool LiveVideo { get; set; }
        bool LiveAudio { get; set; }
    }
}
