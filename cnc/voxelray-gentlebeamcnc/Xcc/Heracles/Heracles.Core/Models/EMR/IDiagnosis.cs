using Heracles.Core.Enums;

using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IDiagnosis : IEntry
    {
        DateTime CreationDate { get; set; }
        long PatientId { get; set; }
        string SiteName { get; set; }
        SiteLocation? SiteLocation { get; set; }
        Pathology? Pathology { get; set; }
        string Referring { get; set; }
        IcdCode? IcdCode { get; set; }
        Celltype? SubcellOne { get; set; }
        Celltype? SubcellTwo { get; set; }
        Description? Description { get; set; }
        bool Archived { get; set; }
    }
}
