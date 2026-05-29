using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserRoleTests
    {
        [Test]
        public void UserRole_Guest()
        {
            var sut = UserRole.Guest;

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.Name, Is.EqualTo("Guest"));
            Assert.That(sut.Description, Is.EqualTo(string.Empty));
            Assert.That(sut.Permissions, Is.Not.Null);
            Assert.That(sut.Permissions.Count(), Is.EqualTo(Enum.GetValues<PermissionType>().Length));
        }
        
        [Test]
        public void UserRole_From_Name(
            [Values("name1", "name with spaces")] string name)
        {
            var sut = new UserRole(name);

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.Name, Is.EqualTo(name));
            Assert.That(sut.Description, Is.EqualTo(string.Empty));
            Assert.That(sut.Permissions, Is.Not.Null);
            Assert.That(sut.Permissions.Count(), Is.EqualTo(Enum.GetValues<PermissionType>().Length));
            Assert.That(sut.BuiltIn, Is.False);
        }
        
        [Test]
        public void UserRole_From_BuildInName(
            [Values(
                UserRole.BuiltInNames.Administrator, 
                UserRole.BuiltInNames.Rtt, 
                UserRole.BuiltInNames.Service,
                UserRole.BuiltInNames.Physicist
            )] string name)
        {
            
            var sut = new UserRole(name);

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.Name, Is.EqualTo(name));
            Assert.That(sut.Description, Is.EqualTo(string.Empty));
            Assert.That(sut.Permissions, Is.Not.Null);
            Assert.That(sut.Permissions.Count(), Is.EqualTo(Enum.GetValues<PermissionType>().Length));
            Assert.That(sut.BuiltIn, Is.True);
        }
        
        [Test]
        public void UserRole_From_IdName(
            [Values(0, 1)] long id,
            [Values("name1", "name with spaces")] string name)
        {
            var sut = new UserRole(id, name);

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.Name, Is.EqualTo(name));
            Assert.That(sut.Description, Is.EqualTo(string.Empty));
            Assert.That(sut.Permissions, Is.Not.Null);
            Assert.That(sut.Permissions.Count(), Is.EqualTo(Enum.GetValues<PermissionType>().Length));
        }
        
        [Test]
        public void UserRole_From_UserRole(
            [Values(0, 1)] long id,
            [Values("name1", "name with spaces")] string name)
        {
            var copy = new UserRole(id, name); 
            
            var sut = new UserRole(copy);

            Assert.That(sut.Id, Is.EqualTo(copy.Id));
            Assert.That(sut.Name, Is.EqualTo(copy.Name));
            Assert.That(sut.Description, Is.EqualTo(copy.Description));
            
            var sutPermissions = sut.Permissions.OrderBy(p => p.Type).ToList();
            var expectedPermissions = copy.Permissions.OrderBy(p => p.Type).ToList();
            Assert.That(sutPermissions.Count, Is.EqualTo(expectedPermissions.Count));

            for (int i = 0; i < expectedPermissions.Count; i++)
            {
                Assert.That(sutPermissions[i].Id, Is.EqualTo(expectedPermissions[i].Id), $"id not equals at {i}");
                Assert.That(sutPermissions[i].RoleId, Is.EqualTo(expectedPermissions[i].RoleId), $"RoleId not equals at {i}");
                Assert.That(sutPermissions[i].Type, Is.EqualTo(expectedPermissions[i].Type), $"Type not equals at {i}");
                Assert.That(sutPermissions[i].Value, Is.EqualTo(expectedPermissions[i].Value), $"Value not equals at {i}");
            }
        }
    }
}