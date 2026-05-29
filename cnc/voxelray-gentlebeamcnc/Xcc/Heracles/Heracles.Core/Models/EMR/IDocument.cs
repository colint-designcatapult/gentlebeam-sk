using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IDocument : IEntry
    {
        DateTime CreationDate { get; set; }
        string Description { get; set; }
        string Path { get; set; }
        IPatient Patient { get; set; }
        long PatientId { get; set; }
        string Type { get; set; }
        IVisit Visit { get; set; }
        long? VisitId { get; set; }
    }
}
