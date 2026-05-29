using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.System;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class QcReportService(
        IQcReportListModel qcModel,
        IQcRepository qcRepository,
        IAuthorizedUserStore userStore,
        ILogWriter logWriter)
    {
        public async Task SaveQcSampleReportAsync(QualityCheckPlan plan)
        {
            try
            {
                var savedSamples = await SaveQcEntriesAsync(plan.Fields);
                if (savedSamples is not null && savedSamples.Count > 0)
                {
                    // We get only those samples that match the current applicator configuration:
                    var matchingSavedSamples = savedSamples.Where(
                        s => s.CollimatorConfigurationId == qcModel.CurrentCollimatorConfigurationId).ToList();

                    foreach (var sample in matchingSavedSamples)
                    {
                        qcModel.AddNewSample(sample);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataServiceException("Failed to save QC data", ex);
            }
        }

        /// <summary>
        /// Creates all the necessary QcSamples by the list of emitted fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private async Task<ICollection<QcSampleBindable>> SaveQcEntriesAsync(IEnumerable<IQcSampleFieldEntry> fields)
        {
            var samples = new List<QcSampleBindable>();

            // We need to split the fields by their configurations,
            // to create one QcSample per configuration
            var configIds = fields.Select(f => f.Configuration.Id).Distinct();
            foreach (var configId in configIds)
            {
                var oneSampleFields = fields
                    .Where(f => f.Configuration.Id == configId)
                    .OrderBy(x => x.Name)
                    .ToList();

                if (oneSampleFields.Count == 0)
                {
                    continue; // Just in case
                }

                var firstField = fields.First();
                var sampleConfig = firstField.Configuration;

                var sample = new QcSampleHeader
                {
                    CollimatorConfigurationId = configId,
                    EmissionCurrent = (float)CurrentCalculator.CalculateCurrent(sampleConfig.Energy),
                    HeaterCurrent = (float)firstField.FilamentSetpoint,
                    PerformedBy = userStore.AuthorizedUser.EmailAddress,
                    Referenced = false,
                    Duration = (float)oneSampleFields.First().DwellTime
                };

                var savedSample = await CreateQcSampleAsync(sample, oneSampleFields);

                samples.Add(savedSample);
                _ = logWriter.LogAsync($"QC saved: id = {savedSample.Id} by {userStore.AuthorizedUser.EmailAddress}", LogRecordSeverity.Info, LogRecordType.System);
            }

            return samples;
        }

        private async Task<QcSampleBindable> CreateQcSampleAsync(IQcSampleHeader sampleHeader, List<IQcSampleFieldEntry> oneSampleFields)
        {
            var (storedSample, storedFields) = await qcRepository.CreateQcSampleAsync(sampleHeader, oneSampleFields);
            
            var sample = new QcSampleBindable(storedSample);
            
            var reportFields = storedFields.Select(x => new QcReportField(x, sample.EmissionCurrent)).OrderBy(x => x.FieldName);            
            sample.SetFields(reportFields);

            return sample;
        }
    }
}
