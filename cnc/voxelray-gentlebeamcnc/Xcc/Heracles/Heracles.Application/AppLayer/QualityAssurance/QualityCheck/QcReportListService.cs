using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Heracles.Application.AppLayer.QualityAssurance.QualityCheck
{
    public class QcReportListService(
        IQcReportListModel qcReportList,
        IQcRepository qcRepository)
    {
        public async Task<ObservableCollection<QcSampleBindable>> FetchSampleReportListAsync(
            ICollimatorConfiguration? collimatorConfiguration)
        {
            qcReportList.Clear();

            if (collimatorConfiguration is null)
                return qcReportList.Items;

            var sampleHeaders = (await qcRepository.FetchQcSampleListAsync(collimatorConfiguration.Id));
            sampleHeaders = sampleHeaders.OrderByDescending(x => x.Id).ToList();
            var bindableSamples = sampleHeaders.Select(x => new QcSampleBindable(x));
            qcReportList.SetList(collimatorConfiguration, bindableSamples);

            var refSample = sampleHeaders.FirstOrDefault(x => x.Referenced);
            if (refSample is not null)
            {
                var refReportFields = await FetchQcReportFieldsAsync(refSample);

                qcReportList.ReferencedSample?.SetFields(refReportFields);
            }

            return qcReportList.Items;
        }

        public async Task<IQcSample> FetchQcSampleDataAsync(IQcSample sample)
        {
            // Fetch data only if it wasn't fetched before:
            if (sample.Fields == null)
            {
                var sampleData = await FetchQcReportFieldsAsync(sample);
                sample.SetFields(sampleData);
            }
            return sample;
        }

        public async Task<IQcSample> SetAsReferenceAsync(IQcSample selectedQcSample)
        {
            // Find a referenced item:
            var previousReference = qcReportList.ReferencedSample;
            var sampleToMakeReference = qcReportList.Items.First(s => s.Id == selectedQcSample.Id);

            // Apply action (now we don't use updated object from Moses)
            _ = await qcRepository.SwitchReferenceQcSampleAsync(from: previousReference, to: sampleToMakeReference);

            // Now update entries in the list to make it consistent:
            return qcReportList.SetAsReferenced(sampleToMakeReference);
        }

        private async Task<IEnumerable<QcReportField>> FetchQcReportFieldsAsync(IQcSampleHeader sample)
        {
            var fields = await qcRepository.FetchQcFieldsAsync(sample.Id);
            return fields.Select(x => new QcReportField(x, sample.EmissionCurrent)).OrderBy(x => x.FieldName);
        }
    }
}
