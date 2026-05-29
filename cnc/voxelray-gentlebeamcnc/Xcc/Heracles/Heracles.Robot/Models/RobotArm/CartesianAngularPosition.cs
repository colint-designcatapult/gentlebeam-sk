namespace Heracles.Robot.Models.RobotArm
{
    public class CartesianAngularPosition
    {
        public CartesianPosition CartesianPositionMM { get; set; }
        public AngularPosition AngularPositionDeg { get; set; }
        public override string ToString()
        {
            string s = string.Empty;
            if (CartesianPositionMM != null) {
                s = "{" + CartesianPositionMM.ToString()  + "} ";
            }
            if (AngularPositionDeg != null) {
                s += "{" + AngularPositionDeg.ToString() + "}";
            }
            return s;
        }
    }
}
