namespace Heracles.Robot.Models.RobotArm
{
    public class AngularPosition
    {
        public float Rx { get; set; }
        public float Ry { get; set; }
        public float Rz { get; set; }
        public override string ToString()
        {
            return $"{Rx:0.00}, {Ry:0.00}, {Rz:0.00}";
        }

    }
}
