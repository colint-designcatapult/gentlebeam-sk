namespace Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserRoleRecord : BaseEntry
    {
        public long UserId { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public string UserEmail { get; set; } = string.Empty;
        public long RoleId { get; set; } = BaseEntry.NEW_ENTRY_ID;
    }
}
