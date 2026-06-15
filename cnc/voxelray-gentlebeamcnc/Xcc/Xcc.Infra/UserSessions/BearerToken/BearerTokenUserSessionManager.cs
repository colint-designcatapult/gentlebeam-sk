using System;
using System.Threading;

namespace Xcc.Infra.UserSessions.BearerToken
{
    public class BearerTokenUserSessionManager(CancellationToken globalCancellationToken)
        : IBearerTokenUserSessionManager
    {
        #region Events
        public event EventHandler<UserSessionEventArgs>? UserSessionChanged;
        #endregion Events


        #region Properties
        public BearerTokenUserSession UserSession => _userSession;
        // as time may be a bit out of sync with moses,
        // we better have it 1 minute less than 24 hours for now:
        public TimeSpan SessionExpirationTimeout =>
            new TimeSpan(hours: 23, minutes: 59, seconds: 0); 
        #endregion Properties


        #region Public methods
        public void StartUserSession(string username, string bearerToken)
        {
            lock (this)
            {
                if (_userSession != BearerTokenUserSession.NoSession)
                {
                    CloseUserSession(); // make sure previous session was closed - TODO: would it be better to throw an exception instead?
                }

                SetUserSession(
                    new BearerTokenUserSession(username, bearerToken, GetNewExpirationDate(), globalCancellationToken));

                RaiseSessionEvent(_userSession, UserSessionEventType.Open);
            }
        }

        public void ExpireUserSession()
        {
            lock (this)
            {
                AssertSessionExists(_userSession);

                if (_userSession.IsLocked)
                    return; // don't do anything twice

                RaiseSessionEvent(_userSession, UserSessionEventType.Expiration);

                // reset headers to not having bearer token
                SetUserSession(_userSession.Expire());

                RaiseSessionEvent(_userSession, UserSessionEventType.Locked);
            }
        }

        public void LockUserSession()
        {
            lock (this)
            {
                AssertSessionExists(_userSession);

                if (_userSession.IsLocked)
                    return; // don't do anything twice

                UserSessionEventType eventType =
                    _userSession.IsExpired ? UserSessionEventType.Expiration : UserSessionEventType.Lock;

                RaiseSessionEvent(_userSession, eventType);

                // reset headers to not having bearer token
                SetUserSession(_userSession.Lock());

                RaiseSessionEvent(_userSession, UserSessionEventType.Locked);
            }
        }

        public void UnlockUserSession(string username, string bearerToken)
        {
            lock (this)
            {
                AssertSessionExists(_userSession);

                if (username != _userSession.Username)
                {
                    throw new ArgumentException("Unlock session - failed: session username doesn't match");
                }

                // set bearer token to headers:
                SetUserSession(_userSession.Unlock(bearerToken, GetNewExpirationDate()));

                RaiseSessionEvent(_userSession, UserSessionEventType.Unlocked);
            }
        }

        private void AssertSessionExists(BearerTokenUserSession? userSession)
        {
            if (userSession == BearerTokenUserSession.NoSession)
            {
                throw new InvalidOperationException("User session is missing");
            }
        }

        public void CloseUserSession()
        {
            lock (this)
            {
                //AssertSessionExists(_userSession);
                if (_userSession != BearerTokenUserSession.NoSession)
                {
                    RaiseSessionEvent(_userSession, UserSessionEventType.Close);

                    SetUserSession(_userSession.Close());
                }
            }
        }

        #endregion Public methods


        #region Private methods
        protected virtual void SetUserSession(BearerTokenUserSession userSession)
        {
            _userSession = userSession;
        }

        private void RaiseSessionEvent(UserSession userSession, UserSessionEventType eventType)
        {
            UserSessionChanged?.Invoke(this, new UserSessionEventArgs(userSession, eventType));
        }

        private DateTime GetNewExpirationDate()
        {
            return DateTime.Now + SessionExpirationTimeout;
        }
        #endregion Private methods


        // User context: auth token and cancellation token for the streams etc.
        private BearerTokenUserSession _userSession = BearerTokenUserSession.NoSession; 
    }
}
