using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IPlanSnapshot : IEntry
    {
        DateTime CreationDate { get; set; }
        string Description { get; set; }
        string Location { get; set; }
        IPlan Plan { get; set; }
        long PlanId { get; set; }
        string Type { get; set; }
    }
}
