using Xcc.Application.AppLayer.Model;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.AppLayer.Service
{
    public interface IActionAuditService
    {
        void RegisterAction(string actionDescription);
        void RegisterAction(string actionDescription, string actionDetails);
    }

    public class ActionAuditService : IActionAuditService
    {
        private ILogWriter LogWriter { get; }
        private IAuthorizedUserStore AuthorizedUserStore { get; }

        public void RegisterAction(string actionDescription)
        {
            var activeUser = AuthorizedUserStore.AuthorizedUser;
            if (activeUser is null || activeUser.Id <= 0 || string.IsNullOrWhiteSpace(activeUser.Username))
                return;

            string message = $"User Action. {actionDescription} by {activeUser.Username} (user id={activeUser.Id})";
            _ = LogWriter.LogAsync(message, LogRecordSeverity.Info, LogRecordType.User);
        }

        public void RegisterAction(string actionDescription, string actionDetails)
        {
            var activeUser = AuthorizedUserStore.AuthorizedUser;
            if (activeUser is null || activeUser.Id <= 0 || string.IsNullOrWhiteSpace(activeUser.Username))
                return;

            string message = $"User Action. {actionDescription} by {activeUser.Username} (user id={activeUser.Id}): {actionDetails}";
            _ = LogWriter.LogAsync(message, LogRecordSeverity.Info, LogRecordType.User);
        }


        public ActionAuditService(
            ILogWriter logWriter,
            IAuthorizedUserStore authorizedUserStore)
        {
            LogWriter = logWriter;
            AuthorizedUserStore = authorizedUserStore;
        }
    }
}
