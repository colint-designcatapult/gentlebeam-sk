using System;

namespace Heracles.Robot.Models.Interlock
{
    public interface IInterlockService
    {
        public State State { get; }
        public event EventHandler<State> StateChanged;
    }
}
