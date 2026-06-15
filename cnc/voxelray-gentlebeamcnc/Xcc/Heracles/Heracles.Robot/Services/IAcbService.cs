using System;
using System.Threading.Tasks;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;

namespace Heracles.Robot.Services
{
    public interface IAcbService
    {
        void StartListening();
        void StopListening();

        Task<bool> PingAsync();
        Task<bool> SendCommand(AcbActuatorId actuatorId, AcbActuatorCommand actuatorCommand);
        AcbActuatorState GetActuatorState(AcbActuatorId actuatorId);
        AcbLightSensorState GetLightSensorState(AcbActuatorId actuatorId);
        AcbProxySensorState GetProxySensorState(AcbActuatorId actuatorId);

        AcbActuatorState RobotActuator { get; }
        ActuatorWithSensorsInfo ImageActuator { get; }
        ActuatorWithSensorsInfo TreatmentActuator { get; }
        AcbFootPedalState PedalState { get; }

        event EventHandler Updated;
    }
}
