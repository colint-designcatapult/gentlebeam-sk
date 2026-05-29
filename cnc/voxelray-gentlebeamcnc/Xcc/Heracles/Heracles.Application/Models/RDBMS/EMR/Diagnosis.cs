using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Diagnosis : IDiagnosis
    {
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; }
        public long PatientId { get; set; }
        public string SiteName { get; set; }
        public SiteLocation? SiteLocation { get; set; }
        public Pathology? Pathology { get; set; }
        public IcdCode? IcdCode { get; set; }
        public Celltype? SubcellOne { get; set; }
        public Celltype? SubcellTwo { get; set; }
        public Description? Description { get; set; }
        public string Referring { get; set; }
        public bool Archived { get; set; }

        public Diagnosis()
        {
        }
        public Diagnosis(IDiagnosis d)
        {
            if (d != null)
            {
                GenericExtensions.CopyProperties(d, this);
            }
        }
    }
}
