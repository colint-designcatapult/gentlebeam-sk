using Heracles.Core.Models.EMR;
using System;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class PlanSnapshot : IPlanSnapshot
    {
        public long Id { get; set; }

        public DateTime CreationDate { get; set; }

        public long PlanId { get; set; }

        public string Description { get; set; } = null!;

        public string Type { get; set; } = null!;

        public string Location { get; set; } = null!;

        public virtual IPlan Plan { get; set; } = null!;
    }
}
