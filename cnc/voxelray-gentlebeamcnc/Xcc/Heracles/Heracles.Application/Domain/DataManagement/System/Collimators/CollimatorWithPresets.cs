using System.Collections.Generic;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public class CollimatorWithPresets : Collimator, ICollimatorWithPresets
    {

        public CollimatorWithPresets(ICollimator collimator = null, ICollimatorConfiguration collimatorConfiguration = null, IList<IPresetConfiguration> presets = null)
            :base(collimator)
        {
            if (collimatorConfiguration != null)
                Configuration = collimatorConfiguration;
        }

    }
}
