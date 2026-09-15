using Heracles.Core.Commands;
using Moq;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Application.Test.Models;

[TestFixture(false)]
[TestFixture(true)]
public sealed class UserConfigurationAuditTests(bool applicationRepository)
{
    private Mock<IUserCommands> _users = null!;
    private Mock<IUserRoleMappingCommandsExt> _mappings = null!;
    private IUserRepository _repository = null!;
    private readonly List<string> _audit = [];
    private User _stored = null!;

    [SetUp]
    public void SetUp()
    {
        _audit.Clear();
        _users = new();
        _mappings = new();
        _stored = new User { Id = 8, Username = "target", Password = "old-secret", Role = new UserRole(3, "role") };
        _users.Setup(x => x.ReadAsync(8)).ReturnsAsync(() => new User(_stored));
        _users.Setup(x => x.UpdateAsync(It.IsAny<IUser>(), It.IsAny<IUser>()))
            .ReturnsAsync((IUser _, IUser requested) => _stored = new User(requested));
        _mappings.Setup(x => x.ReadAsync(8)).ReturnsAsync(() => new UserRoleRecord { Id = 4, UserId = 8, RoleId = 3 });
        _mappings.Setup(x => x.UpdateAsync(It.IsAny<UserRoleRecord>(), It.IsAny<UserRoleRecord>()))
            .ReturnsAsync((UserRoleRecord _, UserRoleRecord requested) => requested);
        var roles = Mock.Of<IRoleCommands>();
        var permissions = Mock.Of<IPermissionCommands>();
        _repository = applicationRepository
            ? new Heracles.Application.Models.UserRepository(_users.Object, _mappings.Object, roles, permissions)
            : new Xcc.Infra.DataManagement.Common.UserRepository(_users.Object, _mappings.Object, roles, permissions);
    }

    [Test]
    public void UserChangeIsAuditedBeforeLaterRoleWriteFails_WithoutCredentials()
    {
        _mappings.Setup(x => x.UpdateAsync(It.IsAny<UserRoleRecord>(), It.IsAny<UserRoleRecord>()))
            .ThrowsAsync(new InvalidOperationException("role write failed"));
        var requested = new User(_stored) { Password = "new-secret" };

        Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveUserAsync(requested, _audit.Add));

        Assert.That(_stored.Password, Is.EqualTo("new-secret"));
        Assert.That(_audit, Has.Count.EqualTo(1));
        Assert.That(_audit.Single(), Does.Contain("id=8").And.Not.Contain("new-secret").And.Not.Contain("old-secret"));
    }

    [Test]
    public async Task NoOpAndUnappliedUserChangesProduceNoAudit()
    {
        await _repository.SaveUserAsync(new User(_stored), _audit.Add);
        _users.Setup(x => x.UpdateAsync(It.IsAny<IUser>(), It.IsAny<IUser>())).ReturnsAsync(() => new User(_stored));
        await _repository.SaveUserAsync(new User(_stored) { FirstName = "not-persisted" }, _audit.Add);
        Assert.That(_audit, Is.Empty);
    }

    [Test]
    public async Task RoleChangeAuditsBothUserRecordAndMapping()
    {
        await _repository.SaveUserAsync(new User(_stored) { Role = new UserRole(5, "other") }, _audit.Add);
        Assert.That(_audit, Has.Count.EqualTo(2));
        Assert.That(_audit[0], Does.Contain("id=8"));
        Assert.That(_audit[1], Does.Contain("userId=8").And.Contain("roleId=5"));
    }

    [Test]
    public void CreatedUserIsAuditedWhenRoleCreationFails()
    {
        _users.Setup(x => x.CreateAsync(It.IsAny<IUser>())).ReturnsAsync(new User(_stored));
        _mappings.Setup(x => x.CreateAsync(It.IsAny<UserRoleRecord>())).ThrowsAsync(new InvalidOperationException());
        Assert.ThrowsAsync<InvalidOperationException>(() => _repository.CreateUserAsync(new User(_stored), _audit.Add));
        Assert.That(_audit, Has.Count.EqualTo(1));
        Assert.That(_audit.Single(), Does.Contain("id=8"));
    }

    [Test]
    public async Task DeleteAuditsOnlyRecordsActuallyRemoved()
    {
        _mappings.Setup(x => x.DeleteAsync(4)).ReturnsAsync(true);
        _users.Setup(x => x.DeleteAsync(8)).ReturnsAsync(false);
        await _repository.DeleteUserAsync(8, _audit.Add);
        Assert.That(_audit, Has.Count.EqualTo(1));
        Assert.That(_audit.Single(), Does.Contain("Remove user role").And.Contain("userId=8"));
    }
}
