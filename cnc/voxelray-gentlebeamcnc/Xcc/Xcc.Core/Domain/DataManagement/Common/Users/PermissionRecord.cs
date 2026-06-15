namespace Xcc.Core.Domain.DataManagement.Common.Users
{

    public enum PermissionType
    {
        ClinicalData = 1,
        Treatment,
        SystemCalibration,
        QualityAssurance,
        SystemSettings,
        UserManagement,
        Services
    }

    public class PermissionRecord : BaseEntry
    {
        public long RoleId { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public PermissionType Type { get; set; }
    }
}
