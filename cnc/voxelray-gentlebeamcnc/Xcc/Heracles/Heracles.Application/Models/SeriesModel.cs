using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;

using Prism.Events;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Core.Logging;

namespace Heracles.Application.Models
{
    public interface ISeriesModel : INotifyPropertyChanged
    {
        ISeries Series { get; }

        IList<ISeries> SeriesList { get; }

        Task<ISeries> CreateSeriesAsync(ISeries series);

        Task<ISeries> UpdateLastSeriesAsync(ISeries series);

        Task<ISeries> FetchLatestSeriesByDiagnosisId(long diagnosisId);

        Task<ISeries> SendDicomAsync(int index, ISeries series, byte[] data, long seriesId);

        Task<ISeries> SendDicomFilesAsync(string[] files, ISeries series);
    }

    public class SeriesModel : BindableBase, ISeriesModel
    {
        private const int ChunkSize = 256 * 1024; // todo: can be configured from AppSettings

        public SeriesModel(
            ILogWriter logWriter,
            IEmrSeriesCommands emrSeriesCommands,
            ITreatmentInfoStore treatmentInfoStore)
        {
            LogWriter = logWriter;
            EmrSeriesCommands = emrSeriesCommands;

            treatmentInfoStore.DiagnosisChanged += TreatmentInfoStore_DiagnosisChanged;
        }

        public ILogWriter LogWriter { get; }
        public IEmrSeriesCommands EmrSeriesCommands { get; }
        public ISeries Series { get => _seriesList?.LastOrDefault(); }

        private IList<ISeries> _seriesList;
        public IList<ISeries> SeriesList 
        { 
            get => _seriesList;
            private set => SetProperty(ref _seriesList, value);            
        }

        private void TreatmentInfoStore_DiagnosisChanged(object sender, IDiagnosis diagnosis)
        {
            if (diagnosis == null)
            {
                SeriesList = null;
            }
            else
                FetchLatestSeriesByDiagnosisId(diagnosis.Id).ConfigureAwait(false);
        }

        public async Task<ISeries> FetchLatestSeriesByDiagnosisId(long diagnosisId)
        {
            var collection = await EmrSeriesCommands.ReadListAsync(diagnosisId);
            SeriesList = collection.OrderBy(s => s.Id).ToList();

            RaisePropertyChanged(nameof(SeriesList));

            return Series;
        }

        public async Task<ISeries> CreateSeriesAsync(ISeries series)
        {
            var created = await EmrSeriesCommands.CreateAsync(series);
            if (SeriesList == null)
            {
                SeriesList = new List<ISeries>();
            }

            SeriesList.Add(created);

            RaisePropertyChanged(nameof(SeriesList));

            return created;
        }

        public async Task<ISeries> UpdateLastSeriesAsync(ISeries series)
        {
            var lastSeries = Series;

            if (lastSeries.Id != series.Id)
            {
                throw new ArgumentException("Wrong last series id");
            }

            var updated = await EmrSeriesCommands.UpdateAsync(Series, series);

            var index = SeriesList.IndexOf(lastSeries);

            SeriesList[index] = updated;

            RaisePropertyChanged(nameof(SeriesList));

            return updated;
        }

        public async Task<ISeries> SendDicomAsync(int index, ISeries series, byte[] data, long seriesId)
        {
            await EmrSeriesCommands.SendDicomDataAsync(index, data, ChunkSize, series.Id);

            return series;
        }

        public async Task<ISeries> SendDicomFilesAsync(string[] files, ISeries series)
        {
            await EmrSeriesCommands.SendDicomFilesAsync(files, ChunkSize, series.Id);

            return series;
        }
    }
}
