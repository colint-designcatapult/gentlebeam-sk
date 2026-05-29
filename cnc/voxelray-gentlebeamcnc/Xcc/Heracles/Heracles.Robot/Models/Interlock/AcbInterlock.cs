using Heracles.Application.Services;
using System;
using Heracles.Robot.Services;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Interlock
{
    public class AcbInterlock : InterlockBase, IInterlockService, IDisposable
    {
        #region Constructors
        public AcbInterlock(IAcbService acbService, ILogRepository logWriter) : base(logWriter)
        {
            _acbService = acbService;
            logWriter.LogAsync($"AcbInterlock: Started", Xcc.Core.Enums.LogRecordSeverity.Info, Xcc.Core.Enums.LogRecordType.System);
        }
        #endregion Constructors

        #region Properties
        private IAcbService _acbService;
        #endregion Properties

        #region Methods
        protected override State GetState()
        {
            try
            {
                State state = _acbService.PedalState == AcbFootPedalState.Down ? State.Allow : State.Deny;
                return state;
            }
            catch (Exception ex)
            {
            }

            return State.Deny;
        }
        #endregion Methods
    }
}
