using System;
using System.Collections.Generic;
using Heracles.Core.Enums;
using Heracles.Robot.Models.Enums;

namespace Heracles.Robot.Models
{
    public enum AcbActuatorState : int
    {
        Unknown = 0,
        Lock = 1, //  Forward(Close)
        Unlock = 2 // Reverse(Open)
    }
    public enum AcbProxySensorState : int
    {
        Unknown = 0,
        Detected = 1,
        NotDetected = 2
    }

    public enum AcbLightSensorState : int
    {
        Unknown = 0,
        Interrupt = 1, // Lock
        NotInterrpupt = 2 // Unlock
    }

    public enum AcbFootPedalState : int
    {
        Unknown = 0,
        Down = 1,
        Up = 2
    }

    public struct ActuatorStateInfo
    {
        public AcbActuatorState ActuatorState;
        public AcbProxySensorState ProxySensorState;
        public AcbLightSensorState LightSensorState;
        public AcbFootPedalState FootPedalState;
    }
    public struct ActuatorWithSensorsInfo
    {
        public AcbActuatorState State = AcbActuatorState.Unknown;
        public AcbLightSensorState LightSensorState = AcbLightSensorState.Unknown;
        public AcbProxySensorState ProxySensorState = AcbProxySensorState.Unknown;
        public ActuatorWithSensorsInfo()
        { }
        public ActuatorWithSensorsInfo(ActuatorStateInfo status)
        {
            State = status.ActuatorState;
            LightSensorState = status.LightSensorState;
            ProxySensorState = status.ProxySensorState;
        }
    }

    public struct AcbActuatorStatusResponse
    {
        public DateTime Timestamp;
        public IDictionary<AcbActuatorId, ActuatorStateInfo> ActuatorStates;

        public AcbActuatorStatusResponse()
        {
            Timestamp = DateTime.Now;
            ActuatorStates = new Dictionary<AcbActuatorId, ActuatorStateInfo>();
        }

        public bool IsExpired(long timeoutMs)
        {
            return Timestamp.AddMilliseconds(timeoutMs) < DateTime.Now;
        }
        public static bool IsNullOrExpired(AcbActuatorStatusResponse? status, long timeoutMs)
        {
            return status is null || status.Value.IsExpired(timeoutMs);
        }
    }

    /// <summary>
    /// Actuator control board
    /// </summary>
    public interface IAcbMessageConverter
    {
        byte[] GenerateActuatorCommandMessage(AcbActuatorId id, AcbActuatorCommand command);

        byte[] GenerateActuatorStatusPollMessage();
        AcbActuatorStatusResponse ParseStatusPollResponse(byte[] response);

    }
}
