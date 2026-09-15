using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    
    public class UserTests
    {
        [TestCase(9u, false)]
        [TestCase(10u, true)]
        [TestCase(11u, true)]
        public void LockoutThresholdSurvivesAuthorizedUserSnapshot(uint failedLogins, bool locked)
        {
            var user = new User { FailedLoginAttempts = failedLogins };
            Assert.That(new User(user).IsLocked, Is.EqualTo(locked));
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