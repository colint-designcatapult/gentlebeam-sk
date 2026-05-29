using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using Xcc.Core.Common;
using Empyrean.Common.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Series : ISeries
    {
        public Series()
        {
            Init();
        }

        public Series(ISeries s)
        {
            Set(s);
        }

        public long Id { get; set; } = BaseEntry.NewEntryId;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public long DiagnosisId { get; set; } = BaseEntry.NewEntryId;

        public long VisitId { get; set; } = BaseEntry.NewEntryId;

        public string Location { get; set; } = string.Empty;

        public string Name { get; set; } = null!;

        public double LesionDepth { get; set; } = 0.0;

        public string Modality { get; set; } = string.Empty;

        public ImageType Type { get; set; }

        public string Description { get; set; } = string.Empty;

        public int NumberOfInstances { get; set; }

        public virtual ICollection<IPhotoDescription> Images { get; set; } = new List<IPhotoDescription>();

        private void Init()
        {
            CreationDate = DateTime.Now;
            Name = string.Empty;
            Modality = string.Empty;
            Id = BaseEntry.NewEntryId;
            DiagnosisId = BaseEntry.NewEntryId;
            VisitId = BaseEntry.NewEntryId;
            Location = string.Empty;
            LesionDepth = 0.0d;
            Type = ImageType.Unspecified;
        }

        private void Set(ISeries s)
        {
            if (s == null)
                Init();
            else
                s.CopyProperties(this);
        }
    }
}
