using System;
using System.Windows.Threading;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Interlock
{
    public class InterlockBase : IInterlockService, IDisposable
    {
        #region Consts
        private const int TimerIntervalMs = 50;
        #endregion Consts

        #region Constructors
        public InterlockBase(ILogRepository logWriter)
        {
            _logWriter = logWriter;
            _logWriter.LogAsync($"InterlockBase: Started", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);

            state = State.Deny;
            OnStateChanged();

            timer = new System.Timers.Timer(TimerIntervalMs);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }
        #endregion Constructors

        #region Properties
        public event EventHandler<State> StateChanged;
        public State State => state;

        private State state = State.Deny;
        private DispatcherTimer dispatcherTimer = null;
        private System.Timers.Timer timer = null;
        private ILogRepository _logWriter;
        #endregion Properties

        #region Methods
        public void Dispose()
        {
            timer.Stop();
            timer.Dispose();
            _logWriter.LogAsync($"InterlockBase: Disposed", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
        }

        protected virtual State GetState()
        {
            return State.Deny;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            State oldState = state;
            state = GetState();
            if (oldState != state)
            {
                OnStateChanged();
            }
        }
        private void OnStateChanged()
        {
            _logWriter.LogAsync($"AcbInterlock: State changed to {state.ToString()}", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
            StateChanged?.Invoke(this, state);
        }
        #endregion Methods
    }
}
