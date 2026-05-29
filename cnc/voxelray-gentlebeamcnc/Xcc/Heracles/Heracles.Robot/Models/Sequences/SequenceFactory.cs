using Heracles.Robot.Models.RobotArm.Interfaces;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Sequences
{
    public class SequenceFactory : ISequenceFactory
    {
        ILogRepository _logWriter;
        public SequenceFactory(ILogRepository logWriter)
        {
            _logWriter = logWriter;
        }
        ISequence ISequenceFactory.Create(string name, System.Collections.Generic.IList<IStep> steps)
        {
            return new Sequence(name, steps, _logWriter);
        }
    }
}
