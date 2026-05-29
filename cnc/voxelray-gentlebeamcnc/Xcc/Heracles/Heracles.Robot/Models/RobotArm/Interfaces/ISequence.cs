using System;

namespace Heracles.Robot.Models.RobotArm.Interfaces
{
    public interface ISequence
    {
        public string Name { get; }
        public string CurrentStepName { get; }
        public string NextStepName { get; }
        
        public bool DoNextStep();
        public bool Do();
        public bool CanDoNextStep { get; }
        public void Reset();

        public event EventHandler StepDone;
    }
}
