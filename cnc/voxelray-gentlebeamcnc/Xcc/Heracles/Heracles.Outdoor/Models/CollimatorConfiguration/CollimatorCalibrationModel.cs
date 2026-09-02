using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Xcc.Core.Logging;

namespace Heracles.External.Models.CollimatorConfiguration
{
    public interface ICollimatorCalibrationModel
    {
        Task<CollimatorCalibrationInfoStore> FetchCalibrationDataAsync(bool forceRefresh = false);
    }

    public class CollimatorCalibrationModel : ICollimatorCalibrationModel
    {
        private readonly ICollimatorModel _collimatorModel;
        private readonly ICollimatorCalibrationRepository _calibrationRepository;
        private readonly ILogWriter _logWriter;
        private readonly CollimatorCalibrationInfoStore _calibrationStore;
        private Task<CollimatorCalibrationInfoStore> _fetchDataTask;
        private readonly object _fetchTaskLock = new();
        private readonly SemaphoreSlim _fetchSemaphore = new(1, 1);

        public CollimatorCalibrationModel(
            ICollimatorModel collimatorModel,
            ICollimatorCalibrationRepository calibrationRepository,
            ILogWriter logWriter,
            CollimatorCalibrationInfoStore calibrationStore)
        {
            _collimatorModel = collimatorModel;
            _calibrationRepository = calibrationRepository;
            _logWriter = logWriter;
            _calibrationStore = calibrationStore;

            _collimatorModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ICollimatorModel.CollimatorConfigurations))
                {
                    OnCollimatorModelUpdate();
                }
            };
        }

        public Task<CollimatorCalibrationInfoStore> FetchCalibrationDataAsync(bool forceRefresh = false)
        {
            lock (_fetchTaskLock)
            {
                if (forceRefresh || _fetchDataTask == null || _fetchDataTask.IsFaulted)
                {
                    _fetchDataTask = RunFetchTask();
                }

                return _fetchDataTask;
            }
        }

        private async Task<CollimatorCalibrationInfoStore> RunFetchTask()
        {
            await _fetchSemaphore.WaitAsync();
            try
            {
                // We get all actual applicators, so we need to exclude any QC applicator configuration from the list.
                var properConfigs = _collimatorModel.CollimatorConfigurations
                    .Where(c => c.Type != Core.Enums.TargetType.TargetType_QC_Collimator)
                    .ToList();
                var fetchTasks = properConfigs.Select(FetchSingleCalibrationSafelyAsync).ToList();
                await Task.WhenAll(fetchTasks);

                var fetchedConfigurations = properConfigs
                    .Zip(fetchTasks.Select(task => task.Result))
                    .Where(pair => pair.Second != null)
                    .ToDictionary(pair => pair.First.Id, pair => pair.Second);
                _calibrationStore.Replace(fetchedConfigurations);

                if (properConfigs.Count > 0 && fetchedConfigurations.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Failed to load calibration data for all {properConfigs.Count} applicator configurations.");
                }

                return _calibrationStore;
            }
            finally
            {
                _fetchSemaphore.Release();
            }
        }

        private async Task<ICollimatorCalibrationInfo> FetchSingleCalibrationSafelyAsync(
            ICollimatorConfiguration baseConfiguration)
        {
            try
            {
                return await _calibrationRepository.FetchConfigurationInfoAsync(baseConfiguration);
            }
            catch (Exception ex)
            {
                _ = _logWriter.LogAsync(
                    $"Cannot load calibration data for applicator configuration " +
                    $"id={baseConfiguration.Id}, type={baseConfiguration.Type}, energy={baseConfiguration.Energy}: " +
                    ex.GetBaseException().Message,
                    Xcc.Core.Enums.LogRecordSeverity.Warn,
                    Xcc.Core.Enums.LogRecordType.System);
                return null;
            }
        }

        private void OnCollimatorModelUpdate()
        {
            lock (_fetchTaskLock)
            {
                _fetchDataTask = null;
            }
        }


    }
}
