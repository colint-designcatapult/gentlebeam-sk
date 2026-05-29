using System;
using System.Timers;
using Xcc.Infra.UserSessions;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Application.AppLayer.UserSessions
{
    public class SessionExpirationWatchdog
    {
        public SessionExpirationWatchdog(IBearerTokenUserSessionManager sessionManagement)
        {
            _timer = new Timer();
            _timer.Elapsed += (s, e) => OnSessionExpirationDate();
            _timer.AutoReset = false;
            _sessionManagement = sessionManagement;
            _sessionManagement.UserSessionChanged += (_, e) => OnUserSessionEvent(e);
        }

        private Timer _timer;
        private readonly IBearerTokenUserSessionManager _sessionManagement;

        #region Public methods
        public void OnUserSessionEvent(UserSessionEventArgs args)
        {
            switch (args.EventType)
            {
                case UserSessionEventType.Open:
                case UserSessionEventType.Unlocked:
                    StartTimer(args.ExpirationDate - DateTime.Now);
                    break;
                default:
                    StopTimer();
                    break;
            }
        }
        public void StartTimer(TimeSpan timeSpan)
        {
            lock (this)
            {
                StopTimer();
                _timer.Interval = timeSpan.TotalMilliseconds;
                _timer.Start();
            }
        }

        public void StopTimer()
        {
            lock (this)
            {
                _timer.Stop();
            }
        }
        #endregion Public methods

        #region Private methods

        private void OnSessionExpirationDate()
        {
            _sessionManagement.ExpireUserSession();
        }
        #endregion Private methods

    }
}
