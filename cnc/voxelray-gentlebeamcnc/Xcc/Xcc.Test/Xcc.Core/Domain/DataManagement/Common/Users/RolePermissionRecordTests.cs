using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class RolePermissionRecordTests
    {
        [Test]
        public void RolePermissionRecord_Defaults()
        {
            var sut = new RolePermissionRecord();

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.PermissionId, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.RoleId, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.PermissionType, Is.EqualTo(default(PermissionType)));
        }
        
        [Test]
        public void RolePermissionRecord_SettersGetters(
            [Values(0, 1)] long id,
            [Values(0, 1)] long permId,
            [Values(0, 1)] long roleId,
            [Values(PermissionType.ClinicalData, PermissionType.Treatment, PermissionType.SystemCalibration,
                PermissionType.QualityAssurance, PermissionType.SystemSettings, PermissionType.UserManagement,
                PermissionType.Services)] PermissionType type)
        {
            var record = new RolePermissionRecord{ Id = id, PermissionId = permId, RoleId = roleId, PermissionType = type };

            Assert.That(record.Id, Is.EqualTo(id));
            Assert.That(record.PermissionId, Is.EqualTo(permId));
            Assert.That(record.RoleId, Is.EqualTo(roleId));
            Assert.That(record.PermissionType, Is.EqualTo(type));
        }
    }
}