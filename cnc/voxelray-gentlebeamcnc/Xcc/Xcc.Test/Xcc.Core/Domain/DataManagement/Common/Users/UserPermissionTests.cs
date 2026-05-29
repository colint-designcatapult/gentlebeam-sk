using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserPermissionTests
    {
        [Test]
        public void UserPermission_From_PermissionType(
            [Values(PermissionType.ClinicalData, PermissionType.Treatment, PermissionType.SystemCalibration,
                PermissionType.QualityAssurance, PermissionType.SystemSettings, PermissionType.UserManagement,
                PermissionType.Services)] PermissionType type)
        {
            var sut = new UserPermission(type);

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.RoleId, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.Type, Is.EqualTo(type));
            Assert.That(sut.Value, Is.False);
        }
        
        [Test]
        public void UserPermission_From_PermissionRecord(
            [Values(0, 1)] long id,
            [Values(0, 1)] long roleId,
            [Values(PermissionType.ClinicalData, PermissionType.Treatment, PermissionType.SystemCalibration,
                PermissionType.QualityAssurance, PermissionType.SystemSettings, PermissionType.UserManagement,
                PermissionType.Services)] PermissionType type)
        {
            var permissionRecord = new PermissionRecord { Id = id, RoleId = roleId, Type = type };
            
            var sut = new UserPermission(permissionRecord);

            Assert.That(sut.Id, Is.EqualTo(permissionRecord.Id));
            Assert.That(sut.RoleId, Is.EqualTo(permissionRecord.RoleId));
            Assert.That(sut.Type, Is.EqualTo(permissionRecord.Type));
            Assert.That(sut.Value, Is.True);
        }
        
        [Test]
        public void UserPermission_From_UserPermission(
            [Values(0, 1)] long id,
            [Values(0, 1)] long roleId,
            [Values(PermissionType.ClinicalData, PermissionType.Treatment, PermissionType.SystemCalibration,
                PermissionType.QualityAssurance, PermissionType.SystemSettings, PermissionType.UserManagement,
                PermissionType.Services)] PermissionType type,
            [Values(false, true)] bool value)
        {
            var userPermission = new UserPermission(type) { Id = id, RoleId = roleId, Type = type, Value = value };
            
            var sut = new UserPermission(userPermission);

            Assert.That(sut.Id, Is.EqualTo(userPermission.Id));
            Assert.That(sut.RoleId, Is.EqualTo(userPermission.RoleId));
            Assert.That(sut.Type, Is.EqualTo(userPermission.Type));
            Assert.That(sut.Value, Is.EqualTo(userPermission.Value));
        }
        
        [Test]
        public void UserPermission_SettersGetters(
            [Values(0, 1)] long id,
            [Values(0, 1)] long roleId,
            [Values(PermissionType.ClinicalData, PermissionType.Treatment, PermissionType.SystemCalibration,
                PermissionType.QualityAssurance, PermissionType.SystemSettings, PermissionType.UserManagement,
                PermissionType.Services)] PermissionType type,
            [Values(false, true)] bool value)
        {
            var sut = new UserPermission(type) { Id = id, RoleId = roleId, Type = type, Value = value };

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.RoleId, Is.EqualTo(roleId));
            Assert.That(sut.Type, Is.EqualTo(type));
            Assert.That(sut.Value, Is.EqualTo(value));
        }
    }
}