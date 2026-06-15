using Empyrean.Common.Core.Domain.DataManagement.Common;
using System;
using Xcc.Core.Common;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public class QcSampleHeader(long id) : BaseEntry(id), IQcSampleHeader
    {
        public QcSampleHeader(IQcSampleHeader sample)
            : this(sample.Id)
        {
            sample.CopyProperties(this);
        }
        public QcSampleHeader() : this(NewEntryId) // for compatibility with DummyCommands
        { }

        public long CollimatorConfigurationId { get; set; } = NewEntryId;
        public float EmissionCurrent { get; set; } = 0.0f;
        public float HeaterCurrent { get; set; } = 0.0f;
        public bool Referenced { get; set; } = false;
        public string ApprovedBy { get; set; } = string.Empty;
        public bool IsApproved => !string.IsNullOrEmpty(ApprovedBy);
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public float Duration { get; set; } = 0f;
        public string? PerformedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
