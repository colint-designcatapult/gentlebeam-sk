namespace Heracles.External.Models.CollimatorConfiguration
{
    public class CollimatorCalibrationInfoStore
    {
        public CollimatorCalibrationInfoStore()
        {
        }

        public ICollimatorCalibrationInfo this[long configurationId]
        {
            get
            {
                lock (_lock)
                {
                    ICollimatorCalibrationInfo info = null;
                    _configurations.TryGetValue(configurationId, out info);
                    return info;
                }
            }
            set
            {
                lock (_lock)
                {
                    _configurations[configurationId] = value;
                }
            }
        }
        public void Replace(IReadOnlyDictionary<long, ICollimatorCalibrationInfo> configurations)
        {
            lock (_lock)
            {
                _configurations.Clear();
                foreach (var configuration in configurations)
                {
                    _configurations.Add(configuration.Key, configuration.Value);
                }
            }
        }

        public CollimatorCalibrationInfoStore Filter(Func<ICollimatorCalibrationInfo, bool> predicate)
        {
            var filtered = new CollimatorCalibrationInfoStore();

            lock (_lock)
            {
                foreach (var config in _configurations)
                {
                    if (predicate(config.Value))
                        filtered[config.Key] = config.Value;
                }
            }

            return filtered;
        }

        private readonly object _lock = new object();
        private readonly Dictionary<long, ICollimatorCalibrationInfo> _configurations = new Dictionary<long, ICollimatorCalibrationInfo>();

    }
}