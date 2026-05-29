using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class PhotoDescription : BaseEntry, IPhotoDescription
    {
        public DateTime CreationDate { get; set; }

        public long DiagnosisId { get; set; } = Empyrean.Common.Core.Domain.DataManagement.Common.BaseEntry.NewEntryId;

        public long VisitId { get; set; } = Empyrean.Common.Core.Domain.DataManagement.Common.BaseEntry.NewEntryId;

        public string Description { get; set; } = string.Empty;

        public PhotoType Type { get; set; }

        public TemplateType TemplateType { get; set; }

        public string Path { get; set; } = string.Empty;



        public string Thumbnail { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}