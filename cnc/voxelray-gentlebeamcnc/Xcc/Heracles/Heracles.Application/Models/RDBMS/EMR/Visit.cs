using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Visit : IVisit
    {
        public long Id { get; set; }
        public DateTime CreationDate { get; set; }
        public long PatientId { get; set; }
        public VisitType Type { get; set; }

        public Visit()
        { }

        public Visit(IVisit visit)
        {
            if (visit != null)
            {
                visit.CopyProperties(this);
            }
        }
    }
}
