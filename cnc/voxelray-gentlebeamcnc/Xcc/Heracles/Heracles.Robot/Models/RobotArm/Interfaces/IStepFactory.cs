using System.Collections.Generic;

namespace Heracles.Robot.Models.RobotArm.Interfaces
{
    public interface IStepFactory
    {
        public IStep Create(string id, string nextIdIfOk, string nextIdIfFailed, string action, IList<string> actionValues, IList<string> actuatorsPrecondition);
    }
}
