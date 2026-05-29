using Heracles.Core.Models.EMR;
using System;
using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class EmissionTreatmentField : IEmissionTreatmentField
    {
        public EmissionTreatmentField()
        {            
        }
        public EmissionTreatmentField(IEmissionTreatmentField entry)
        {
            GenericExtensions.CopyProperties(entry, this);
        }

        public long Id { get; set; }

        public DateTime CreationDate { get; set; }

        public long ActualTreatmentFieldId { get; set; }

        public double ActualDwellTime { get; set; }
    }
}
