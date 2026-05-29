using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Infra.Persistence.DataAccess.Dummy;

namespace Heracles.Application.Test.Commands.DummyCommands
{
    internal class DummyRootEntryCommandsTests
    {
        internal class TestDataClass : BaseEntry
        {
            public long Data { get; set; } = 0;
        }

        internal class TestDummyCommands : DummyRootEntryCommands<TestDataClass, TestDataClass>
        {
        }

        TestDummyCommands commands = null!;
        TestDataClass dataEntry = null!;

        [SetUp]
        public void Setup()
        {
            commands = new TestDummyCommands();
            dataEntry = commands.CreateAsync(new TestDataClass()).GetAwaiter().GetResult();
        }

        [Test]
        public void ReadListAsync_PresentDataTest()
        {
            var emptyList = commands.ReadAllAsync().GetAwaiter().GetResult();
            Assert.That(emptyList, Is.Not.Null);
            Assert.That(emptyList, Is.Not.Empty);
        }

        [Test]
        public void CreateAsyncTest()
        {
            var newDataEntry = commands.CreateAsync(new TestDataClass()).GetAwaiter().GetResult();
            Assert.That(newDataEntry, Is.Not.Null);
            Assert.That(newDataEntry.Id, Is.Not.EqualTo(dataEntry.Id)); // should be unique
        }

        [Test]
        public void ReadAsync_ForExistentObject_Test()
        {
            var entry = commands.ReadAsync(dataEntry.Id).GetAwaiter().GetResult();
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Data, Is.EqualTo(dataEntry.Data));
        }

        [Test]
        public void ReadAsync_ForMissingObject_Test()
        {
            Assert.Throws<DataServiceException>(() => commands.ReadAsync(dataEntry.Id + 1).GetAwaiter().GetResult());
        }

        [Test]
        public void UpdateAsync_ForExistentObject_Test()
        {
            TestDataClass newValue = new();
            dataEntry.CopyProperties(newValue);
            newValue.Data += 1;

            var updatedOutput = commands.UpdateAsync(null!, newValue).GetAwaiter().GetResult();
            Assert.That(updatedOutput, Is.Not.Null);
            Assert.That(updatedOutput.Data, Is.EqualTo(newValue.Data));
        }

        [Test]
        public void UpdateAsync_ForMissingObject_Test()
        {
            TestDataClass newValue = new();
            dataEntry.CopyProperties(newValue);
            newValue.Id += 1;

            Assert.Throws<DataServiceException>(() => commands.UpdateAsync(null!, newValue).GetAwaiter().GetResult());
        }

        [Test]
        public void DeleteAsync_ForExistentObject_Test()
        {
            bool result = commands.DeleteAsync(dataEntry.Id).GetAwaiter().GetResult();
            Assert.That(result, Is.True);

            var parentList = commands.ReadAllAsync().GetAwaiter().GetResult();
            Assert.That(parentList, Is.Not.Null);
            Assert.That(parentList, Is.Empty);
        }

        [Test]
        public void DeleteAsync_ForMissingObject_Test()
        {
            Assert.Throws<DataServiceException>(() => commands.DeleteAsync(dataEntry.Id + 1).GetAwaiter().GetResult());
        }
    }
}
