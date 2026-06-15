using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.Models.RobotArm
{
    public class MotionActionResponse
    {
        public ActionResponseTag Tag { get; set; }
        public bool? GoalAccepted { get; set; }
        public CartesianPosition FeedbackPositionMm { get; set; }
        public AngularPosition FeedbackPositionDeg { get; set; }
        public JointsPosition FeedbackJointPositionsDeg { get; set; }
        public bool? ResultSuccess { get; set; }
        public string ResultDetails { get; set; }

        public override string ToString()
        {
            string s = "Tag=" + Tag.ToString();
            if (GoalAccepted.HasValue)
            {
                s += " GoalAccepted=" + GoalAccepted.ToString();
            }
            if (FeedbackPositionMm != null)
            {
                s += " FeedbackPositionMm={" + FeedbackPositionMm.ToString() + "}";
            }
            if (FeedbackPositionDeg != null)
            {
                s += " FeedbackPositionDeg={" + FeedbackPositionDeg.ToString() + "}";
            }
            if (FeedbackJointPositionsDeg != null)
            {
                s += " FeedbackJointPositionsDeg={" + FeedbackJointPositionsDeg.ToString() + "}";
            }
            if (ResultSuccess.HasValue)
            {
                s += " ResultSuccess=" + ResultSuccess.ToString();
            }
            if (ResultDetails != null && ResultDetails.Length > 0)
            {
                s += " ResultDetails=" + ResultDetails;
            }
            return s;
        }

    }
}
