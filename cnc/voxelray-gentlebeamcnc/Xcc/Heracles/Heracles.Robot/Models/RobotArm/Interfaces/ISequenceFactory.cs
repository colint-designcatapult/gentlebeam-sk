using System.Collections.Generic;

namespace Heracles.Robot.Models.RobotArm.Interfaces
{
    public interface ISequenceFactory
    {
        public ISequence Create(string name, IList<IStep> steps);
    }
}
