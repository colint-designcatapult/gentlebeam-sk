using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserRoleRecordTests
    {
        [Test]
        public void UserRoleRecord_Defaults()
        {
            var sut = new UserRoleRecord();

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.UserId, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.RoleId, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
        }
        
        [Test]
        public void UserRoleRecord_SettersGetters(
            [Values(0, 1)] long id,
            [Values(0, 1)] long userId,
            [Values(0, 1)] long roleId)
        {
            var sut = new UserRoleRecord{ Id = id, UserId = userId, RoleId = roleId};

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.UserId, Is.EqualTo(userId));
            Assert.That(sut.RoleId, Is.EqualTo(roleId));
        }
    }
}