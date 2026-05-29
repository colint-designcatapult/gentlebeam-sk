using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    class TestUser : IUser
    {
        public long Id { get; set; } = 42;
        public DateTime CreationDate { get; set; } = new DateTime(2024, 1, 1);
        public string Username { get; set; } = "testusername";
        public string FirstName { get; set; } = "testfirstname";
        public string LastName { get; set; } = "testlastname";
        public string MiddleName { get; set; } = "testmiddlename";
        public UserRole Role { get; set; } = new UserRole(42, "testrole");
        public string EmailAddress { get; set; } = "test@gmail.com";
        public DateTime LastAccessed { get; set; } = new DateTime(2025, 1, 1);
        public string Picture { get; set; } = "testpicture";
        public string Password { get; set; } = "testpassword";
        public string Fullname() { return $"{FirstName} {LastName}"; }
    }
    
    public class UserTests
    {
        [Test]
        public void User_Defaults()
        {
            var sut = new User();

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.CreationDate, Is.EqualTo(default(DateTime)));
            Assert.That(sut.Username, Is.EqualTo(string.Empty));
            Assert.That(sut.FirstName, Is.EqualTo(string.Empty));
            Assert.That(sut.LastName, Is.EqualTo(string.Empty));
            Assert.That(sut.MiddleName, Is.EqualTo(string.Empty));
            Assert.That(sut.Role, Is.Not.Null);
            Assert.That(sut.Role.Name, Is.EqualTo("Guest"));
            Assert.That(sut.EmailAddress, Is.EqualTo(string.Empty));
            Assert.That(sut.LastAccessed, Is.EqualTo(default(DateTime)));
            Assert.That(sut.Picture, Is.EqualTo(string.Empty));
            Assert.That(sut.Password, Is.EqualTo(string.Empty));
        }
        
        [Test]
        public void User_FromIUser()
        {
            var testUser = new TestUser();
            
            var sut = new User(testUser);

            Assert.That(sut.Id, Is.EqualTo(testUser.Id));
            Assert.That(sut.CreationDate, Is.EqualTo(testUser.CreationDate));
            Assert.That(sut.Username, Is.EqualTo(testUser.Username));
            Assert.That(sut.FirstName, Is.EqualTo(testUser.FirstName));
            Assert.That(sut.LastName, Is.EqualTo(testUser.LastName));
            Assert.That(sut.MiddleName, Is.EqualTo(testUser.MiddleName));
            Assert.That(sut.Role, Is.Not.Null);
            Assert.That(sut.Role.Id, Is.EqualTo(testUser.Role.Id));
            Assert.That(sut.Role.Name, Is.EqualTo(testUser.Role.Name));
            Assert.That(sut.EmailAddress, Is.EqualTo(testUser.EmailAddress));
            Assert.That(sut.LastAccessed, Is.EqualTo(testUser.LastAccessed));
            Assert.That(sut.Picture, Is.EqualTo(testUser.Picture));
            Assert.That(sut.Password, Is.EqualTo(testUser.Password));
        }
        
        [Test]
        public void User_SettersGetters(
            [Values(0, 1)] long id,
            [Values("2024-01-01", "2025-01-01")] DateTime creationDate,
            [Values("xcc", "username")] string username,
            [Values("joe", "firstname")] string firstName,
            [Values("joey", "lastname")] string lastName,
            [Values("ross", "middlename")] string middleName,
            [Values(0, 1)] long roleId,
            [Values("role", "rolename")] string roleName,
            [Values("email@address", "test@gmail.com")] string emailAddress,
            [Values("2024-02-01", "2025-02-01")] DateTime lastAccessed,
            [Values("pic1", "picture")] string picture,
            [Values("qwerty", "password")] string password
            )
        {
            var sut = new User
            {
                Id = id,
                CreationDate = creationDate,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName,
                Role = new UserRole(roleId, roleName),
                EmailAddress = emailAddress,
                LastAccessed = lastAccessed,
                Picture = picture,
                Password = password
            };

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.CreationDate, Is.EqualTo(creationDate));
            Assert.That(sut.Username, Is.EqualTo(username));
            Assert.That(sut.FirstName, Is.EqualTo(firstName));
            Assert.That(sut.LastName, Is.EqualTo(lastName));
            Assert.That(sut.MiddleName, Is.EqualTo(middleName));
            Assert.That(sut.Role, Is.Not.Null);
            Assert.That(sut.Role.Id, Is.EqualTo(roleId));
            Assert.That(sut.Role.Name, Is.EqualTo(roleName));
            Assert.That(sut.EmailAddress, Is.EqualTo(emailAddress));
            Assert.That(sut.LastAccessed, Is.EqualTo(lastAccessed));
            Assert.That(sut.Picture, Is.EqualTo(picture));
            Assert.That(sut.Password, Is.EqualTo(password));
        }

        [Test]
        public void Fullname()
        {
            var sut = new User
            {
                FirstName = "testname",
                LastName = "testlastname"
            };

            Assert.That(sut.Fullname(), Is.EqualTo("testname testlastname"));
        }
    }
}