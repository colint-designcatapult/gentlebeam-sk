using Heracles.Core.Models;
using System.Collections.Generic;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public interface IQcSample : IQcSampleHeader
    {
        IList<QcReportField> Fields { get; }
        void ApplyReference(IQcSample? reference);

        void SetFields(IEnumerable<QcReportField> refReportFields);
    }
}
