using Moq;
using Prism.Events;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.ViewModels;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Logging;
using Xcc.Infra.UserSessions;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Test.Xcc.Shared.ViewModels;

// Reuse the shared WPF fixture so confirmation dialogs run on its Application dispatcher.
[Apartment(ApartmentState.STA)]
public sealed class UserManagementLockoutTests
{
    private readonly Dictionary<long, User> _stored = [];
    private Mock<IUserRepository> _repository = null!;
    private Mock<IBearerTokenUserSessionManager> _sessions = null!;
    private Mock<IActionAuditService> _audit = null!;
    private AuthorizedUserStore _actors = null!;
    private BearerTokenUserSession _session = null!;
    private UserManagementViewModel _model = null!;
    private Func<long, Task> _reset = null!;
    private Action? _duringConfirmation;
    private bool _confirm;
    private int _confirmations;

    [SetUp]
    public void SetUp()
    {
        _stored.Clear();
        _stored.Add(7, new User { Id = 7, Username = "first", Password = "first-password", FailedLoginAttempts = 10 });
        _stored.Add(8, new User { Id = 8, Username = "second", Password = "second-password", FailedLoginAttempts = 10 });
        _actors = new AuthorizedUserStore
        {
            AuthorizedUser = new User { Id = 1, Username = "admin", Role = new UserRole("Administrator") }
        };
        _session = new BearerTokenUserSession("admin", "session-token", DateTime.Now.AddHours(1), null);
        _sessions = new Mock<IBearerTokenUserSessionManager>();
        _sessions.SetupGet(x => x.UserSession).Returns(() => _session);
        _repository = new Mock<IUserRepository>();
        _repository.Setup(x => x.FetchUsersAsync()).ReturnsAsync(() =>
            (ICollection<IUser>)_stored.Values.Select(user => (IUser)new User(user)).ToArray());
        _repository.Setup(x => x.FetchAllUserRolesAsync()).ReturnsAsync(Array.Empty<UserRole>());
        _reset = id =>
        {
            if (_stored[id].IsLocked)
                _stored[id].FailedLoginAttempts = 0;
            return Task.CompletedTask;
        };
        _repository.Setup(x => x.ResetUserLockoutAsync(It.IsAny<long>())).Returns((long id) => _reset(id));
        _audit = new Mock<IActionAuditService>(MockBehavior.Strict);
        var dialogs = new Mock<IDialogService>();
        dialogs.Setup(x => x.ShowDialog(It.IsAny<string>(), It.IsAny<IDialogParameters>(), It.IsAny<Action<IDialogResult>>()))
            .Callback<string, IDialogParameters, Action<IDialogResult>>((_, _, callback) =>
            {
                _confirmations++;
                _duringConfirmation?.Invoke();
                callback(new DialogResult(_confirm ? ButtonResult.OK : ButtonResult.Cancel));
            });
        _model = new UserManagementViewModel(_actors, _repository.Object, Mock.Of<ILogRepository>(),
            dialogs.Object, _audit.Object, _sessions.Object, new EventAggregator());
        _confirm = true;
        _confirmations = 0;
        _duringConfirmation = null;
        _model.OnNavigatedTo(null!);
        _model.UserTask!.Task.GetAwaiter().GetResult();
        _model.SelectedUser = _model.Users.Single(user => user.Id == 7);
    }

    [TearDown]
    public void TearDown() => _model.OnNavigatedFrom(null!);

    [TestCase("ordinary")]
    [TestCase("missing-actor")]
    [TestCase("locked-actor")]
    [TestCase("locked-session")]
    [TestCase("expired-session")]
    [TestCase("different-session-user")]
    [TestCase("cancelled-session")]
    [TestCase("unlocked-target")]
    [TestCase("no-target")]
    public void InvalidActorSessionOrTargetCannotExecuteEvenDirectly(string state)
    {
        switch (state)
        {
            case "ordinary": _actors.AuthorizedUser = new User { Id = 2, Username = "operator", Role = new UserRole("RTT") }; break;
            case "missing-actor": _actors.AuthorizedUser = null; break;
            case "locked-actor": ((User)_actors.AuthorizedUser!).FailedLoginAttempts = 10; break;
            case "locked-session": _session = _session.Lock(); break;
            case "expired-session": _session = new BearerTokenUserSession("admin", "token", DateTime.Now.AddHours(-1), null); break;
            case "different-session-user": _session = new BearerTokenUserSession("other", "token", DateTime.Now.AddHours(1), null); break;
            case "cancelled-session": _session.Close(); break;
            case "unlocked-target": _model.SelectedUser = new User { Id = 7, FailedLoginAttempts = 9 }; break;
            case "no-target": _model.SelectedUser = null; break;
        }

        Assert.That(_model.ResetLockoutCommand.CanExecute(), Is.False);
        _model.ResetLockoutCommand.Execute();

        Assert.That(_stored[7].IsLocked, Is.True);
        Assert.That(_confirmations, Is.Zero);
        if (state is "ordinary" or "missing-actor")
            Assert.That(_model.IsAdministrator, Is.False);
    }

    [Test]
    public void SessionLockAndActorDemotionUpdateExistingCommand()
    {
        var command = _model.ResetLockoutCommand;
        var invalidations = 0;
        command.CanExecuteChanged += (_, _) => invalidations++;
        Assert.That(command.CanExecute(), Is.True);

        _session = _session.Lock();
        _sessions.Raise(x => x.UserSessionChanged += null,
            new UserSessionEventArgs(_session, UserSessionEventType.Locked));
        Assert.That(command.CanExecute(), Is.False);
        Assert.That(invalidations, Is.GreaterThan(0));

        _actors.AuthorizedUser = new User { Id = 1, Username = "admin", Role = new UserRole("RTT") };
        Assert.That(_model.IsAdministrator, Is.False);
        Assert.That(command.CanExecute(), Is.False);
    }

    [Test]
    public void CancelledConfirmationLeavesLockoutAndPasswordUnchanged()
    {
        _confirm = false;
        _model.ResetLockoutCommand.Execute();
        Assert.That(_stored[7].IsLocked, Is.True);
        Assert.That(_stored[7].Password, Is.EqualTo("first-password"));
        Assert.That(_model.ResetLockoutCommand.CanExecute(), Is.True);
    }

    [Test]
    public void ConfirmedTargetIsStableAcrossSelectionChangesAndAwait()
    {
        var pending = new TaskCompletionSource();
        _reset = async id =>
        {
            await pending.Task;
            _stored[id].FailedLoginAttempts = 0;
        };
        _duringConfirmation = () => _model.SelectedUser = _model.Users.Single(user => user.Id == 8);
        _model.ResetLockoutCommand.Execute();
        Assert.That(_model.ResetLockoutCommand.CanExecute(), Is.False);
        Assert.That(_model.UserTask!.IsNotCompleted, Is.True);
        _model.UserToEdit!.FirstName = "Unsaved other user";

        pending.SetResult();
        _model.UserTask.Task.GetAwaiter().GetResult();

        Assert.That(_stored[7].IsLocked, Is.False);
        Assert.That(_stored[8].IsLocked, Is.True);
        Assert.That(_stored[7].Password, Is.EqualTo("first-password"));
        Assert.That(_model.Users.Single(user => user.Id == 7).IsLocked, Is.False);
        Assert.That(_model.SelectedUser!.Id, Is.EqualTo(8));
        Assert.That(_model.UserToEdit!.FirstName, Is.EqualTo("Unsaved other user"));
        Assert.That(_model.UserToEdit.IsModified, Is.True);
        Assert.That(_stored[8].FirstName, Is.Empty);
        Assert.That(_model.ResetLockoutCommand.CanExecute(), Is.True);
        Assert.That(_audit.Invocations, Is.Empty);
    }

    [Test]
    public void SessionChangedDuringConfirmationCannotResetUnderAnotherActor()
    {
        _duringConfirmation = () => _session = new BearerTokenUserSession("admin", "replacement-token", DateTime.Now.AddHours(1), null);
        _model.ResetLockoutCommand.Execute();
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _model.UserTask!.Task);
        Assert.That(_stored[7].IsLocked, Is.True);
    }

    [Test]
    public void FailedResetKeepsLockedStateAndRetryUsesConfirmedTarget()
    {
        _reset = _ => Task.FromException(new InvalidOperationException("server denied reset"));
        _model.ResetLockoutCommand.Execute();
        Assert.ThrowsAsync<InvalidOperationException>(() => _model.UserTask!.Task);
        Assert.That(_model.UserTask!.IsSuccessfullyCompleted, Is.False);
        Assert.That(_model.SelectedUser!.IsLocked, Is.True);
        _model.SelectedUser = _model.Users.Single(user => user.Id == 8);
        _reset = id => { _stored[id].FailedLoginAttempts = 0; return Task.CompletedTask; };

        _model.RetryUserTaskCommand!.Execute();
        _model.UserTask.Task.GetAwaiter().GetResult();
        Assert.That(_stored[7].IsLocked, Is.False);
        Assert.That(_stored[8].IsLocked, Is.True);
        Assert.That(_model.Users.Single(user => user.Id == 7).IsLocked, Is.False);
        Assert.That(_audit.Invocations, Is.Empty);
    }

    [Test]
    public void RefreshFailureDoesNotReportSuccessOrRepeatCommittedReset()
    {
        _repository.Setup(x => x.FetchUsersAsync()).ThrowsAsync(new InvalidOperationException("refresh failed"));
        _model.ResetLockoutCommand.Execute();
        Assert.ThrowsAsync<InvalidOperationException>(() => _model.UserTask!.Task);
        Assert.That(_stored[7].IsLocked, Is.False);
        Assert.That(_model.SelectedUser!.IsLocked, Is.True);
        Assert.That(_model.UserTask!.IsSuccessfullyCompleted, Is.False);

        // A subsequent lockout must not be cleared by retrying the failed list refresh.
        _stored[7].FailedLoginAttempts = 10;
        _repository.Setup(x => x.FetchUsersAsync()).ReturnsAsync(() =>
            (ICollection<IUser>)_stored.Values.Select(user => (IUser)new User(user)).ToArray());
        _model.RetryUserTaskCommand!.Execute();
        _model.UserTask.Task.GetAwaiter().GetResult();
        Assert.That(_stored[7].IsLocked, Is.True);
        Assert.That(_model.SelectedUser.IsLocked, Is.True);
        Assert.That(_audit.Invocations, Is.Empty);
    }

    [Test]
    public void StaleLockedSelectionRefreshesNoOpWithoutClientSuccessAudit()
    {
        _stored[7].FailedLoginAttempts = 3;
        _model.ResetLockoutCommand.Execute();
        _model.UserTask!.Task.GetAwaiter().GetResult();
        Assert.That(_stored[7].FailedLoginAttempts, Is.EqualTo(3));
        Assert.That(_model.SelectedUser!.IsLocked, Is.False);
        Assert.That(_model.ResetLockoutCommand.CanExecute(), Is.False);
        Assert.That(_stored[7].Password, Is.EqualTo("first-password"));
        Assert.That(_audit.Invocations, Is.Empty);
    }
}
