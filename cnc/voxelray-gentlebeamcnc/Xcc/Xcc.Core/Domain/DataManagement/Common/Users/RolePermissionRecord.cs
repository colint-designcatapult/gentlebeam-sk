namespace Xcc.Core.Domain.DataManagement.Common.Users;

public class RolePermissionRecord : BaseEntry
{
    public long PermissionId { get; set; } = BaseEntry.NEW_ENTRY_ID;
    public long RoleId { get; set; } = BaseEntry.NEW_ENTRY_ID;

    public PermissionType PermissionType { get; set; }
}