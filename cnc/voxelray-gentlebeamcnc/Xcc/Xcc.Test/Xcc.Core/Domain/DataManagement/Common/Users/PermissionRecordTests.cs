using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class PermissionRecordTests
    {
        [Test]
        public void PermissionRecord_Defaults()
        {
            var sut = new PermissionRecord();

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.RoleId, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.Type, Is.EqualTo(default(PermissionType)));
        }
        
        [Test]
        public void PermissionRecord_SettersGetters(
            [Values(0, 1)] long id,
            [Values(0, 1)] long roleId,
            [Values(PermissionType.ClinicalData, PermissionType.Treatment, PermissionType.SystemCalibration,
                PermissionType.QualityAssurance, PermissionType.SystemSettings, PermissionType.UserManagement,
                PermissionType.Services)] PermissionType type)
        {
            var sut = new PermissionRecord { Id = id, RoleId = roleId, Type = type };

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.RoleId, Is.EqualTo(roleId));
            Assert.That(sut.Type, Is.EqualTo(type));
        }
    }
}