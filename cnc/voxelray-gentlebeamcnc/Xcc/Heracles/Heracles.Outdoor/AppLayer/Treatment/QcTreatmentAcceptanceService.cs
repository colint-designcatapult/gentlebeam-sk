using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System;

namespace Heracles.External.AppLayer.Treatment
{
    public enum QcAcceptanceStatus
    {
        Accepted = 0,
        Failed = 1,
        Missing = 2,
        NoReference = 3,
    }
    /// <summary>
    /// This service is to evaluate QC acceptance condition for running treatments
    /// </summary>
    /// <param name="qcRepository"></param>
    public class QcTreatmentAcceptanceService(
        IQcRepository qcRepository)
    {
        public const double QcDeviationThreshold = 5; // 5% deviation is critical

        /// <summary>
        /// Tests for matching Qc deviation limits for a particular head configuration (energy)
        /// </summary>
        /// <returns>true if </returns>
        public async Task<QcAcceptanceStatus> QcDeviationAcceptanceTestAsync(long configurationId)
        {
            var samples = await qcRepository.FetchQcSampleListAsync(configurationId);
            samples = samples.OrderBy(x => x.CreationDate);

            var qcHeaderReferenced = samples.FirstOrDefault(x => x.Referenced);
            var qcHeaderLatest = samples.LastOrDefault();

            // We need to have non-zero sample within last 24 hours
            if (qcHeaderLatest is null || qcHeaderLatest.CreationDate < DateTime.Now.AddHours(-24))
            {
                return QcAcceptanceStatus.Missing;
            }
            else if (qcHeaderLatest.IsApproved) // OK if it was approved, no matter how big deviations are
            {
                return QcAcceptanceStatus.Accepted;
            }
            else if (qcHeaderReferenced is null) // We need to compare against a reference, so it has to present
            {
                return QcAcceptanceStatus.NoReference;
            }
            else
            {
                QcSampleBindable qcSampleReferenced = await GetQcSampleWithDataAsync(qcHeaderReferenced);
                QcSampleBindable qcSampleLatest = await GetQcSampleWithDataAsync(qcHeaderLatest);
                qcSampleLatest.ApplyReference(qcSampleReferenced);

                bool isAcceptable = qcSampleLatest.IsDeviationAcceptable(QcDeviationThreshold);
                return isAcceptable ? QcAcceptanceStatus.Accepted : QcAcceptanceStatus.Failed;
            }
        }

        private async Task<QcSampleBindable> GetQcSampleWithDataAsync(IQcSampleHeader sample)
        {
            var fields = await qcRepository.FetchQcFieldsAsync(sample.Id);
            var reportFields = fields.Select(x => new QcReportField(x, sample.EmissionCurrent)).OrderBy(x => x.FieldName).ToList();
            return new QcSampleBindable(sample, reportFields);
        }
    }
}
