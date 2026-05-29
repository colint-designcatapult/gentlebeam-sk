using System.Collections.Generic;

namespace Heracles.Robot.Models.RobotArm.Interfaces
{
    public interface ISequencesProvider
    {
        public ISequence Provide(string name);
        public IList<string> SequenceNames { get; }
    }
}
