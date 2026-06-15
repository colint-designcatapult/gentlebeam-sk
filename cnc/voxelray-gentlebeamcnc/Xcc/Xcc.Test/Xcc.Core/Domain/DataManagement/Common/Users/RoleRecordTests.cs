using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common.Users
{
    public class RoleRecordTests
    {
        [Test]
        public void RoleRecord_Defaults()
        {
            var sut = new RoleRecord();

            Assert.That(sut.Id, Is.EqualTo(BaseEntry.NEW_ENTRY_ID));
            Assert.That(sut.Name, Is.EqualTo(string.Empty));
            Assert.That(sut.Description, Is.EqualTo(string.Empty));
        }
        
        [Test]
        public void RoleRecord_SettersGetters(
            [Values(0, 1)] long id,
            [Values("name1", "name with spaces")] string name,
            [Values("desc", "some")] string desc)
        {
            var sut = new RoleRecord{ Id = id, Name = name, Description = desc };

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.Name, Is.EqualTo(name));
            Assert.That(sut.Description, Is.EqualTo(desc));
        }
    }
}