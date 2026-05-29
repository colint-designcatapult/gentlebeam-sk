using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Exceptions;

namespace Heracles.Application.Infra.DataManagement.System
{
    public interface IQcRepository
    {
        Task<IEnumerable<IQcSampleHeader>> FetchQcSampleListAsync(long collimatorConfigurationId);
        Task<IEnumerable<QcField>> FetchQcFieldsAsync(long qcSampleId);

        //TODO: maybe we need to get rid of IQcSampleFieldEntry here, with something more lightweight
        Task<(IQcSampleHeader, IEnumerable<QcField>)> CreateQcSampleAsync(IQcSampleHeader sample, IEnumerable<IQcSampleFieldEntry> fields);
        Task<IQcSampleHeader> SwitchReferenceQcSampleAsync(IQcSampleHeader from, IQcSampleHeader to);
        Task<IQcSampleHeader> ApproveQcSampleAsync(IQcSampleHeader qcSample, string username, string password);
    }

    public class QcRepository : IQcRepository
    {
        private readonly IQcSampleCommands qcSampleCommands;
        private readonly IQcSampleFieldCommands qcSampleFieldCommands;
        private readonly IIntensityCommands intensityCommands;

        public QcRepository(
            IQcSampleCommands qcSampleCommands,
            IQcSampleFieldCommands qcSampleFieldCommands,
            IIntensityCommands intensityCommands)
        {
            this.qcSampleCommands = qcSampleCommands;
            this.qcSampleFieldCommands = qcSampleFieldCommands;
            this.intensityCommands = intensityCommands;
        }

        #region public methods
        public async Task<IEnumerable<IQcSampleHeader>> FetchQcSampleListAsync(long collimatorConfigurationId)
        {
            var collection = await qcSampleCommands.ReadListAsync(collimatorConfigurationId);

            return collection.Select(x => new QcSampleHeader(x));
        }

        public async Task<IEnumerable<QcField>> FetchQcFieldsAsync(long qcSampleId)
        {
            var qcFields = new List<QcField>();
            var fieldRecords = await qcSampleFieldCommands.ReadListAsync(qcSampleId);
            foreach (var field in fieldRecords)
            {
                var intensities = await intensityCommands.ReadListAsync(field.Id);
                // TODO: need to order them according to the stored DiodeName values
                // (now we just order them by Id expecting that we stored them by the same ascending order of DiodeName
                qcFields.Add(
                    new QcField(field.Name, [.. intensities.OrderBy(x => x.DiodeName).Select(x => x.IntensityValue)]));
            }

            return qcFields;
        }

        public async Task<(IQcSampleHeader, IEnumerable<QcField>)> CreateQcSampleAsync(IQcSampleHeader sample, IEnumerable<IQcSampleFieldEntry> fields)
        {
            var storedSample = await qcSampleCommands.CreateAsync(sample);

            if (storedSample == null)
                throw new DataServiceException("QcSample write error: R&V didn't return any value");
        
            var storedQcFields = new List<QcField>();
            foreach (var field in fields)
            {
                var storedField = await qcSampleFieldCommands.CreateAsync(
                    new QcSampleField { Name = field.Name, QcSampleId = storedSample.Id }
                );
                    
                if (field.Intensities != null)
                {
                    var intensities = await SaveIntensityValues(storedField.Id, field.Intensities);

                    storedQcFields.Add(
                        new QcField(field.Name, [.. intensities.OrderBy(x => x.DiodeName).Select(x => x.IntensityValue)]));
                }
            }
            return (storedSample, storedQcFields);
        }


        public async Task<IQcSampleHeader> SwitchReferenceQcSampleAsync(IQcSampleHeader from, IQcSampleHeader to)
        {
            IQcSampleHeader referenced = new QcSampleHeader(to)
            {
                Referenced = true
            };

            if (from != null)
            {
                // Update previous value
                IQcSampleHeader unreferenced = new QcSampleHeader(from)
                {
                    Referenced = false
                };

                _ = await qcSampleCommands.UpdateAsync(from, unreferenced);
            }

            return await qcSampleCommands.UpdateAsync(to, referenced);
        }

        public async Task<IQcSampleHeader> ApproveQcSampleAsync(IQcSampleHeader qcSample, string username, string password)
        {
            var approvedSample = await qcSampleCommands.ApproveAsync(qcSample.Id, username, password);
            qcSample.ApprovedBy = approvedSample.ApprovedBy;
            return qcSample;
        }
        #endregion public methods

        #region private methods
        private async Task<ICollection<IIntensity>> SaveIntensityValues(long fieldId, QcReadings intensities)
        {
            var tasks = intensities.Data.Select(
                (value, i) => intensityCommands.CreateAsync(
                    new Intensity { DiodeName = Intensity.GetDiodeName(i), IntensityValue = value, QcSampleFieldId = fieldId }
                ));
            return await Task.WhenAll(tasks);
        }
        #endregion private methods
    }
}
