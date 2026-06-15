using Heracles.Application.AppLayer.Collimators;
using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Heracles.Application.Models.Supervision
{

    /// <summary>
    /// Class to monitor collimator Serial changes in GCB telemetry 
    /// and manage active collimator instance appropriately
    /// </summary>
    public class CollimatorWatchdog
    {
        private readonly ICollimatorModel _collimatorModel;
        private readonly IDebugSettings _debugSettings; // TODO: temporary, remove after Gcb will deliver actual collimator Id
        private string _telemetryCollimatorSerial;

        public CollimatorWatchdog(
            ICollimatorModel collimatorModel, 
            IDebugSettings debugSettings,
            IEventAggregator eventAggregator)
        {
            _collimatorModel = collimatorModel;
            _debugSettings = debugSettings;

            // Make subscriptions to telemetry and collimator model events to update ActualCollimator on any change there
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnTelemetryChanged, ThreadOption.UIThread);

            collimatorModel.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ICollimatorModel.Collimators))
                {
                    UpdateActualCollimator();
                }
            };
        }

        private void UpdateActualCollimator()
        {
            var currentActiveCollimator = _collimatorModel.ActiveCollimator;

            // We update on change in serial only:
            // either the collimator wasn't exist, and now the serial is not null so we're able to set one,
            // or we have collimator disconnected/changed the serial, so we need to switch Actual
            if (currentActiveCollimator?.Serial != _telemetryCollimatorSerial)
            {
                _collimatorModel.SetActiveCollimator(_telemetryCollimatorSerial);
            }
        }

        private void OnTelemetryChanged(ISystemTelemetry telemetry)
        {
            if (telemetry == null)
                return;

            string newSerialValue = null;
            if (!string.IsNullOrEmpty(_debugSettings.DummyCollimatorSerial))
            {
                // This is debug functionality to overwrite collimator serial with value from appSettings:
                newSerialValue = _debugSettings.DummyCollimatorSerial;
            }
            else
            {
                newSerialValue = (telemetry.CollimatorSerial != 0) ? telemetry.CollimatorSerial.ToString("X") : null;
            }

            if (newSerialValue != _telemetryCollimatorSerial)
            {
                _telemetryCollimatorSerial = newSerialValue;
                UpdateActualCollimator();
            }
        }
    }
}