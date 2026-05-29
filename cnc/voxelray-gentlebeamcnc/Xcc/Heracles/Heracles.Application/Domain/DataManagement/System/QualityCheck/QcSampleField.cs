using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Core.Enums;
using Heracles.Core.Models.RDBMS;
using System;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public class QcSampleField(long id) : BaseEntry(id), IQcSampleField
    {
        public QcSampleField() : this(NewEntryId)
        { }
        public DateTime CreationDate { set; get; }
        public long QcSampleId { set; get; }
        public TreatmentFieldName Name { set; get; }
    }
}
