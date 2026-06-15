using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Xcc.Core.Logging;

namespace Heracles.External.Models.CollimatorConfiguration
{
    public interface ICollimatorCalibrationModel
    {
        Task<CollimatorCalibrationInfoStore> FetchCalibrationDataAsync();
    }

    public class CollimatorCalibrationModel : ICollimatorCalibrationModel
    {
        private readonly ICollimatorModel _collimatorModel;
        private readonly ICollimatorCalibrationRepository _calibrationRepository;
        private readonly ILogWriter _logWriter;
        private readonly CollimatorCalibrationInfoStore _calibrationStore;
        private Task<CollimatorCalibrationInfoStore> _fetchDataTask;

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

        public Task<CollimatorCalibrationInfoStore> FetchCalibrationDataAsync()
        {
            if (_fetchDataTask == null || _fetchDataTask.IsFaulted)
            {
                _fetchDataTask = RunFetchTask();
            }

            return _fetchDataTask;
        }

        private Task<CollimatorCalibrationInfoStore> RunFetchTask()
        {
            return Task.Run(async () =>
            {
                // We get all actual applicators, so we need to exclude any QC applicator configuration from the list
                var properConfigs = _collimatorModel.CollimatorConfigurations?
                    .Where(c => c.Type != Core.Enums.TargetType.TargetType_QC_Collimator)
                    .ToList();
                var fetchTasks = properConfigs.Select(x => _= FetchSingleCalibrationSafelyAsync(x)).ToList();
                await Task.WhenAll(fetchTasks);

                // Select just ones that succeeded and put them into store:
                var configsWithActualResult = properConfigs.Zip(fetchTasks.Select(t => t.Result)).Where(c => c.Second != null);
                foreach (var (config, result) in configsWithActualResult)
                {
                    _calibrationStore[config.Id] = result;
                }

                // Return updated store state to indicate that we're done
                return _calibrationStore;
            });
        }

        private Task<ICollimatorCalibrationInfo> FetchSingleCalibrationSafelyAsync(ICollimatorConfiguration baseConfiguration)
        {
            return Task.Run(async () => {
                try
                {
                    return await _calibrationRepository.FetchConfigurationInfoAsync(baseConfiguration);
                }
                catch //(Exception ex) 
                {
                    //_ = _logWriter.LogAsync(
                    //    $"Cannot load calibration data for the applicator configuration id={baseConfiguration.Id}: {ex.Message}",
                    //    Xcc.Core.Enums.LogRecordSeverity.Warn, Xcc.Core.Enums.LogRecordType.System);
                    return null;
                }
            });
        }

        private void OnCollimatorModelUpdate()
        {
            Task previousTask = _fetchDataTask;
            // TODO: we'd better stop previous task,
            // but now we just wait for current one to finish and run fetch again
            _fetchDataTask = Task.Run(async () => {
                if (previousTask != null)
                {
                    try
                    {
                        await previousTask;
                    }
                    catch (Exception ex)
                    {
                        _ = _logWriter.LogAsync($"FetchDataTask failed: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
                    }
                }

                return await RunFetchTask();
            });
        }


    }
}
