using System;
using Heracles.Robot.Models.RobotArm.Enums;
using Xcc.Application.Models.RobotArm.Enums;

namespace Heracles.Robot.Models
{
    public class RobotTypesConverter
    {
        public static Axis AxisFromString(string s)
        {
            if (Axis.X.ToString() == s) return Axis.X;
            if (Axis.Y.ToString() == s) return Axis.Y;
            if (Axis.Z.ToString() == s) return Axis.Z;
            throw new ArgumentException("Unknown argument: " + s);
        }
        public static OperatingMode OperatingModeFromString(string s)
        {
            if ("Remote control" == s) return OperatingMode.RemoteControl;
            if ("Local control" == s) return OperatingMode.LocalControl;
            throw new ArgumentException("Unknown argument: " + s);
        }
    }
}
