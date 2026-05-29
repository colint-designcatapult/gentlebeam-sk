namespace Heracles.Robot.Models.RobotArm.Interfaces
{
    public interface IStep
    {
        public string Name { get; }
        public string Id { get; }
        public string NextIdIfOk { get; }
        public string NextIdIfFailed { get; }
        public bool Do();
        public bool CheckActuatorsPrecondition();
    }
}
