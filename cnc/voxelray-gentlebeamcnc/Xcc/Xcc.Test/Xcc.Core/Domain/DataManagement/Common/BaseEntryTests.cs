using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Common
{
    public class TestEntry : BaseEntry { }
    
    public class BaseEntryTests
    {
        [Test]
        public void BaseEntry_Defaults()
        {
            var sut = new TestEntry();
            Assert.That(sut.Id, Is.EqualTo(-1));
            Assert.That(BaseEntry.NEW_ENTRY_ID, Is.EqualTo(-1));
            Assert.That(BaseEntry.IsBlankEntry(sut), Is.True);
        }

        [Test]
        public void IsBlankId_True(
            [Values(-1)] int id)
        {
            Assert.That(BaseEntry.IsBlankId(id), Is.True);
        }

        [Test]
        public void IsBlankId_False(
            [Values(-3, -2, 0, 1, 2, 3)] int id)
        {
            Assert.That(BaseEntry.IsBlankId(id), Is.False);
        }

        [Test]
        public void IsBlankEntry_True_WithEntry(
            [Values(-1)] int id)
        {
            var sut = new TestEntry{ Id = id };
            Assert.That(BaseEntry.IsBlankEntry(sut), Is.True);
        }

        [Test]
        public void IsBlankEntry_False_WithEntry(
            [Values(-3, -2, 0, 1, 2, 3)] int id)
        {
            var sut = new TestEntry{ Id = id };
            Assert.That(BaseEntry.IsBlankEntry(sut), Is.False);
        }

        [Test]
        public void IsNullOrBlankEntry_True_WithNull()
        {
            IEntry? sut = null;
            Assert.That(BaseEntry.IsNullOrBlankEntry(sut), Is.True);
        }

        [Test]
        public void IsNullOrBlankEntry_True(
            [Values(-1)] int id)
        {
            var sut = new TestEntry{ Id = id };
            Assert.That(BaseEntry.IsNullOrBlankEntry(sut), Is.True);
        }

        [Test]
        public void IsNullOrBlankEntry_False(
            [Values(-3, -2, 0, 1, 2, 3)] int id)
        {
            var sut = new TestEntry{ Id = id };
            Assert.That(BaseEntry.IsNullOrBlankEntry(sut), Is.False);
        }
    }
}