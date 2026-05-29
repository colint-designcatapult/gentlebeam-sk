using Heracles.Robot.Models.RobotArm.Enums;

namespace Heracles.Robot.Models.RobotArm
{
    public class PingActionResponse
    {
        public ActionResponseTag Tag { get; set; }
        public bool? GoalAccepted { get; set; }
        public int? FeedbackPongId { get; set; }
        public int? ResultPongsTotal { get; set; }

        public override string ToString()
        {
            string s = "Tag=" + Tag.ToString();
            if (GoalAccepted.HasValue)
            {
                s += " GoalAccepted=" + GoalAccepted.ToString();
            }
            if (FeedbackPongId.HasValue)
            {
                s += " FeedbackPongId=" + FeedbackPongId.ToString();
            }
            if (ResultPongsTotal.HasValue)
            {
                s += " ResultPongsTotal=" + ResultPongsTotal.ToString();
            }
            return s;
        }
    }
}
