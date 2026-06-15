namespace Heracles.Robot.Models.RobotArm
{
    public class CartesianPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public override string ToString()
        {
            return $"{X:0.00}, {Y:0.00}, {Z:0.00}";
        }
    }
}
