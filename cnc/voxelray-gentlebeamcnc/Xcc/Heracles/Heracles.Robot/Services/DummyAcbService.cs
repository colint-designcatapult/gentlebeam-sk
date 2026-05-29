using Heracles.Core.Enums;
using Heracles.Core.Models;

using System;
using System.Threading.Tasks;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;

namespace Heracles.Robot.Services
{
    /// <summary>
    /// Dummy actuator communication board service
    /// </summary>
    public class DummyAcbService : IAcbService
    {
        public Task<bool> LockImagingCradleActuatorAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> LockRobotActuatorAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> LockTreatmentCradleActuatorAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> PingAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendCommand(AcbActuatorId actuatorId, AcbActuatorCommand actuatorCommand)
        {
            return Task.FromResult(true);
        }

        public void StartListening()
        {
            return;
        }

        public void StopListening()
        {
            return;
        }

        public Task<bool> UnlockImagingCradleActuatorAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> UnlockRobotActuatorAsync()
        {
            return Task.FromResult(true);
        }

        public Task<bool> UnlockTreatmentCradleActuatorAsync()
        {
            return Task.FromResult(true);
        }

        AcbActuatorState _robotActuator = AcbActuatorState.Lock;
        public AcbActuatorState RobotActuator { get => _robotActuator; }
        ActuatorWithSensorsInfo _imageActuator = new ActuatorWithSensorsInfo()
        {
            State = AcbActuatorState.Unlock,
            LightSensorState = AcbLightSensorState.Unknown,
            ProxySensorState = AcbProxySensorState.Detected
        };
        public ActuatorWithSensorsInfo ImageActuator 
        { 
            get => _imageActuator;
        }
        ActuatorWithSensorsInfo _treatmentActuator = new ActuatorWithSensorsInfo()
        {
            State = AcbActuatorState.Unknown,
            LightSensorState = AcbLightSensorState.Unknown,
            ProxySensorState = AcbProxySensorState.Detected
        };
        public ActuatorWithSensorsInfo TreatmentActuator
        {
            get => _treatmentActuator;
        }

        public AcbActuatorState GetActuatorState(AcbActuatorId actuatorId)
        {
            return actuatorId switch
            {
                AcbActuatorId.Robot => RobotActuator,
                AcbActuatorId.Image => ImageActuator.State,
                AcbActuatorId.Treatment => TreatmentActuator.State,
                _ => throw new ArgumentException(actuatorId.ToString())
            };
        }
        public AcbLightSensorState GetLightSensorState(AcbActuatorId actuatorId)
        {
            return actuatorId switch
            {
                AcbActuatorId.Image => ImageActuator.LightSensorState,
                AcbActuatorId.Treatment => TreatmentActuator.LightSensorState,
                _ => throw new ArgumentException(actuatorId.ToString())
            };
        }
        public AcbProxySensorState GetProxySensorState(AcbActuatorId actuatorId)
        {
            return actuatorId switch
            {
                AcbActuatorId.Image => ImageActuator.ProxySensorState,
                AcbActuatorId.Treatment => TreatmentActuator.ProxySensorState,
                _ => throw new ArgumentException(actuatorId.ToString())
            };
        }
        public AcbFootPedalState PedalState { get => AcbFootPedalState.Unknown; }

        public event EventHandler Updated;
    }
}
