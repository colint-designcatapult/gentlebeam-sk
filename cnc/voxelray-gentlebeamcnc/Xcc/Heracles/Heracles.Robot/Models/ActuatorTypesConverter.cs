using Heracles.Core.Models;
using Heracles.Robot.Models.Sequences;
using System;
using Heracles.Robot.Models.Enums;

namespace Heracles.Robot.Models
{
    public class ActuatorTypesConverter
    {
        public static AcbActuatorId AcbActuatorIdFromString(string s)
        {
            if (s == HeadActuatorName.Flange) return AcbActuatorId.Robot;
            if (s == HeadActuatorName.ImagingCradle) return AcbActuatorId.Image;
            if (s == HeadActuatorName.TreatmentCradle) return AcbActuatorId.Treatment;
            throw new ArgumentException("Unknown argument: " + s);
        }
        public static AcbActuatorCommand AcbActuatorCommandFromString(string s)
        {
            if (s == HeadCommandName.HeadLock) return AcbActuatorCommand.Lock;
            if (s == HeadCommandName.HeadUnlock) return AcbActuatorCommand.Unlock;
            throw new ArgumentException("Unknown argument: " + s);
        }

        public static AcbActuatorState AcbActuatorStateFromString(string s)
        {
            return s switch
            {
                "lock" => AcbActuatorState.Lock,
                "unlock" => AcbActuatorState.Unlock,
                "unknown" => AcbActuatorState.Unknown,
                _ => throw new ArgumentException(s)
            };
        }

        public static AcbProxySensorState AcbProxySensorStateFromString(string s)
        {
            return s switch
            {
                "detected" => AcbProxySensorState.Detected,
                "not_detected" => AcbProxySensorState.NotDetected,
                "unknown" => AcbProxySensorState.Unknown,
                _ => throw new ArgumentException(s)
            };
        }

        public static string AcbStateToString(AcbActuatorState robotActuatorState, ActuatorWithSensorsInfo imagingActuatorStateInfo, ActuatorWithSensorsInfo treatmentActuatorStateInfo)
        {
            string s = string.Empty;

            s += "R" + robotActuatorState switch { AcbActuatorState.Lock => "L", AcbActuatorState.Unlock => "U", _ => "-" };
            if (imagingActuatorStateInfo.LightSensorState == AcbLightSensorState.Interrupt)
                s += "I";
            else if (treatmentActuatorStateInfo.LightSensorState == AcbLightSensorState.Interrupt)
                s += "T";

            s += " I" + imagingActuatorStateInfo.State switch { AcbActuatorState.Lock => "L", AcbActuatorState.Unlock => "U", _ => "-" }
                + imagingActuatorStateInfo.ProxySensorState switch {AcbProxySensorState.Detected => "P", AcbProxySensorState.NotDetected => "N", _ => "-" };

            s += " T" + treatmentActuatorStateInfo.State switch { AcbActuatorState.Lock => "L", AcbActuatorState.Unlock => "U", _ => "-" }
                + treatmentActuatorStateInfo.ProxySensorState switch { AcbProxySensorState.Detected => "P", AcbProxySensorState.NotDetected => "N", _ => "-" };

            return s;
        }
    }
}
