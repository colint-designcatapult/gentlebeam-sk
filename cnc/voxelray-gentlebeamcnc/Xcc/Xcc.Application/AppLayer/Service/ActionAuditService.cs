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
            string message = (activeUser is null)
                ? $"User Action. {actionDescription} by an unauthorized user"
                : $"User Action. {actionDescription} by {activeUser.Username} (user id={activeUser.Id})";
            // TODO: we should make a queue of these records, just in case if network/server goes down
            _ = LogWriter.LogAsync(message, LogRecordSeverity.Info, LogRecordType.User);
        }

        public void RegisterAction(string actionDescription, string actionDetails)
        {
            var activeUser = AuthorizedUserStore.AuthorizedUser;
            string message = (activeUser is null)
                ? $"User Action. {actionDescription} by an unauthorized user: {actionDetails}"
                : $"User Action. {actionDescription} by {activeUser.Username} (user id={activeUser.Id}): {actionDetails}";
            // TODO: we should make a queue of these records, just in case if network/server goes down
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
