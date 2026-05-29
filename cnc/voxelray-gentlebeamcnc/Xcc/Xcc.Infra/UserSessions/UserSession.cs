using System;
using System.Threading;

namespace Xcc.Infra.UserSessions
{
    public class UserSession(string username, DateTime expirationDate, CancellationToken? globalToken)
    {
        public string Username => username;
        public DateTime ExpirationDate => expirationDate;
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsExpired => ExpirationDate <= DateTime.Now;

        public TimeSpan ExpiresAfter => ExpirationDate - DateTime.Now;
        public bool ExpiresIn(TimeSpan timeSpan) => ExpiresAfter <= timeSpan;

        protected CancellationToken? GlobalCancellationToken => globalToken;

        public void Close()
        {
            _cancellationTokenSource.Cancel();
        }

        private CancellationTokenSource _cancellationTokenSource = GetCancellationTokenSource(globalToken);

        private static CancellationTokenSource GetCancellationTokenSource(CancellationToken? token)
        {
            return (token != null)
                ? CancellationTokenSource.CreateLinkedTokenSource(token.Value) 
                : new CancellationTokenSource();
        }
    }
}
