using Moq;
using Xcc.Application.AppLayer.DataAccessControl.ActionAudit;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Enums;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;

namespace Xcc.Test.Xcc.Application.AppLayer.Services;

internal sealed class ActionAuditServiceTests
{
    [Test]
    public void RegisterAction_WithoutActiveUser_DoesNotWriteAudit()
    {
        var log = new RecordingLogWriter();
        var service = new ActionAuditService(log, new AuthorizedUserStore());

        service.RegisterAction("Changed settings");
        service.RegisterAction("Changed settings", "saved");

        Assert.That(log.Records, Is.Empty);
    }

    [TestCase(-1, "operator")]
    [TestCase(0, "operator")]
    [TestCase(17, " ")]
    public void RegisterAction_WithUnrecognizedIdentity_DoesNotWriteAudit(long id, string username)
    {
        var log = new RecordingLogWriter();
        var service = new ActionAuditService(log, new AuthorizedUserStore
        {
            AuthorizedUser = new User { Id = id, Username = username }
        });

        service.RegisterAction("Changed settings");
        service.RegisterAction("Changed settings", "saved");

        Assert.That(log.Records, Is.Empty);
    }

    [Test]
    public void RegisterAction_RecordsKnownUser_AndStopsAfterLogout()
    {
        var log = new RecordingLogWriter();
        var users = new AuthorizedUserStore
        {
            AuthorizedUser = new User { Id = 17, Username = "operator" }
        };
        var service = new ActionAuditService(log, users);

        service.RegisterAction("Changed settings", "saved");
        users.AuthorizedUser = null;
        service.RegisterAction("Changed settings");

        Assert.That(log.Records, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(log.Records[0].Type, Is.EqualTo(LogRecordType.User));
            Assert.That(log.Records[0].Message, Does.Contain("operator").And.Contain("id=17").And.Contain("saved"));
        });
    }

    [TestCase("create")]
    [TestCase("update")]
    [TestCase("delete")]
    public void FailedMutation_PropagatesFailureWithoutAudit(string operation)
    {
        var (proxy, commands, log) = CreateProxy();
        var oldEntry = new User { Id = 23, FirstName = "Before" };
        var newEntry = new User(oldEntry) { FirstName = "After" };
        commands.Setup(value => value.CreateAsync(newEntry)).ThrowsAsync(new InvalidOperationException());
        commands.Setup(value => value.UpdateAsync(oldEntry, newEntry)).ThrowsAsync(new InvalidOperationException());
        commands.Setup(value => value.DeleteAsync(23)).ThrowsAsync(new InvalidOperationException());

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            switch (operation)
            {
                case "create": await proxy.CreateAsync(newEntry); break;
                case "update": await proxy.UpdateAsync(oldEntry, newEntry); break;
                case "delete": await proxy.DeleteAsync(23); break;
            }
        });
        Assert.That(log.Records, Is.Empty);
    }

    [Test]
    public async Task Delete_RecordsOnlyWhenAnEntryWasDeleted()
    {
        var (proxy, commands, log) = CreateProxy();
        commands.SetupSequence(value => value.DeleteAsync(23)).ReturnsAsync(false).ReturnsAsync(true);

        Assert.That(await proxy.DeleteAsync(23), Is.False);
        Assert.That(log.Records, Is.Empty);
        Assert.That(await proxy.DeleteAsync(23), Is.True);
        Assert.That(log.Records, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Update_SuppressesNoOpAndUnappliedChange_ButRecordsAppliedChange()
    {
        var (proxy, commands, log) = CreateProxy();
        var original = new User { Id = 23, FirstName = "Before" };
        var unchanged = new User(original);
        var changed = new User(original) { FirstName = "After" };
        commands.Setup(value => value.UpdateAsync(original, unchanged)).ReturnsAsync(unchanged);
        commands.SetupSequence(value => value.UpdateAsync(original, changed))
            .ReturnsAsync(new User(original))
            .ReturnsAsync(new User(changed));

        await proxy.UpdateAsync(original, unchanged);
        await proxy.UpdateAsync(original, changed);
        Assert.That(log.Records, Is.Empty);
        await proxy.UpdateAsync(original, changed);
        Assert.That(log.Records, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Update_WithoutOriginal_UsesStoredValuesToSuppressNoOp()
    {
        var (proxy, commands, log) = CreateProxy();
        var stored = new User { Id = 23, FirstName = "Before" };
        commands.Setup(value => value.ReadAsync(23)).ReturnsAsync(stored);
        commands.Setup(value => value.UpdateAsync(null!, It.IsAny<User>()))
            .ReturnsAsync((User _, User entry) => new User(entry));

        await proxy.UpdateAsync(null!, new User(stored));
        Assert.That(log.Records, Is.Empty);
        await proxy.UpdateAsync(null!, new User(stored) { FirstName = "After" });
        Assert.That(log.Records, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Create_RecordsPersistedChange_ButReadDoesNot()
    {
        var (proxy, commands, log) = CreateProxy();
        var entry = new User { Id = 23 };
        commands.Setup(value => value.ReadAsync(23)).ReturnsAsync(entry);
        commands.Setup(value => value.CreateAsync(It.IsAny<User>())).ReturnsAsync(entry);

        await proxy.ReadAsync(23);
        Assert.That(log.Records, Is.Empty);
        await proxy.CreateAsync(new User());
        Assert.That(log.Records, Has.Count.EqualTo(1));
        Assert.That(log.Records[0].Message, Does.Contain("id=23"));
    }

    private static (UserDbEntryActionAuditProxy<User> Proxy, Mock<IAsyncСRUDCommands<User>> Commands, RecordingLogWriter Log) CreateProxy()
    {
        var log = new RecordingLogWriter();
        var service = new ActionAuditService(log, new AuthorizedUserStore
        {
            AuthorizedUser = new User { Id = 17, Username = "operator" }
        });
        var commands = new Mock<IAsyncСRUDCommands<User>>();
        return (new UserDbEntryActionAuditProxy<User>(service, commands.Object, "user"), commands, log);
    }

    private sealed class RecordingLogWriter : ILogWriter
    {
        public List<(string Message, LogRecordType Type)> Records { get; } = [];

        public void Log(string message, LogRecordSeverity severity, LogRecordType type) =>
            Records.Add((message, type));

        public Task LogAsync(string message, LogRecordSeverity messageType, LogRecordType type)
        {
            Records.Add((message, type));
            return Task.CompletedTask;
        }
    }
}
