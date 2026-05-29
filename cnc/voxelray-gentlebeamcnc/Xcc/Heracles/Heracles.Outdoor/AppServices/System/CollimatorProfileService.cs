using Heracles.Application.AppLayer.Collimators;
using Heracles.Core.Enums;
using Heracles.External.Models.CollimatorConfiguration;

namespace Heracles.External.AppServices.System
{
    public interface ICollimatorProfileService
    {
        Task<ICollimatorCalibrationInfo?> FindCollimatorProfileAsync(TargetType type, Energy energy);
    }

    public class CollimatorProfileService : ICollimatorProfileService
    {
        public CollimatorProfileService(
            ICollimatorModel collimatorModel,
            ICollimatorCalibrationModel collimatorCalibrationModel)
        {
            CollimatorModel = collimatorModel;
            CollimatorCalibrationModel = collimatorCalibrationModel;
        }

        public ICollimatorModel CollimatorModel { get; }
        public ICollimatorCalibrationModel CollimatorCalibrationModel { get; }

        public async Task<ICollimatorCalibrationInfo?> FindCollimatorProfileAsync(TargetType type, Energy energy)
        {
            var configuration = 
                CollimatorModel.CollimatorConfigurations
                .Where(c => c.Type == type && c.Energy == energy)
                .FirstOrDefault();
            if (configuration != null)
            {
                var presets = await CollimatorCalibrationModel.FetchCalibrationDataAsync();
                return presets[configuration.Id];
            }
            else
            {
                return null;
            }
        }
    }
}
