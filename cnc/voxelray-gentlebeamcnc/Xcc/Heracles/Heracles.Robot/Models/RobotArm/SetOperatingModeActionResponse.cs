using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.Models.RobotArm
{
    public class SetOperatingModeActionResponse
    {
        public ActionResponseTag Tag { get; set; }
        public bool? GoalAccepted { get; set; }
        public bool? ResultSuccess { get; set; }
        public string ResultDetails { get; set; }
        public OperatingMode? ResultOperatingMode { get; set; }

        public override string ToString()
        {
            string s = "Tag=" + Tag.ToString();
            if (GoalAccepted.HasValue)
            {
                s += " GoalAccepted=" + GoalAccepted.ToString();
            }
            if (ResultSuccess.HasValue)
            {
                s += " ResultSuccess=" + ResultSuccess.ToString();
            }
            if (ResultDetails != null && ResultDetails.Length > 0)
            {
                s += " ResultDetails=" + ResultDetails;
            }
            // temporary workaround: do not show result operation mode in the case of fail, since in the current server implementation it may have an incorrect value
            if (ResultOperatingMode.HasValue && ResultSuccess.HasValue && (ResultSuccess.Value == true))
            {
                s += " ResultOperatingMode=" + ResultOperatingMode.ToString();
            }
            return s;
        }
    }
}
