using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Interlock
{
    public class KeyboardInterlock : InterlockBase, IInterlockService, IDisposable
    {
        #region constructors
        public KeyboardInterlock(ILogRepository logWriter) : base(logWriter) 
        {
            logWriter.LogAsync($"KeyboardInterlock: Started", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
        }
        #endregion constructors

        #region Properties
        private IList<Key> keysCombination = new List<Key>() { Key.LeftCtrl }; 
        #endregion Properties

        #region Methods
        protected override State GetState()
        {
            try
            {
                bool pressed = true;
                foreach (var key in keysCombination)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        pressed = pressed && Keyboard.IsKeyDown(key);
                    });
                    if (!pressed)
                    {
                        break;
                    }
                }
                State state = pressed ? State.Allow : State.Deny;

                return state;
            } catch (Exception ex)
            {
            }

            return State.Deny;
        }
        #endregion Methods
    }
}
