using Xcc.Core.Domain.DataManagement.System.QualityAssurance;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public interface IQcSampleHeader : IQaEntryBase
    {
        long CollimatorConfigurationId { get; set; }
        float EmissionCurrent { get; set; }
        float HeaterCurrent { get; set; }
        bool Referenced { get; set; }
        string ApprovedBy { get; set; }
        string Notes { get; set; }
        bool IsApproved { get; }
    }
}
