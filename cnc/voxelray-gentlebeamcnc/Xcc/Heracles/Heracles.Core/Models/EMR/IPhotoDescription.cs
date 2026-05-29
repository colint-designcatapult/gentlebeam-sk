using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IPhotoDescription : IEntry
    {        
        DateTime CreationDate { get; set; }
        string Description { get; set; }
        string Path { get; set; }
        PhotoType Type { get; set; }
        TemplateType TemplateType { get; set; }
        long DiagnosisId { get; set; }
        long VisitId { get; set; }
        string Location { get; set; }
    }

    public interface IPhoto : IPhotoDescription
    {
        byte[] Data { get; set; }
    }
}