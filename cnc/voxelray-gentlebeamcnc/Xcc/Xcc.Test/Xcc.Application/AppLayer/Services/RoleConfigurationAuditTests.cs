using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.ViewModels;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Enums;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;

namespace Xcc.Test.Xcc.Application.AppLayer.Services;

[TestFixture]
public sealed class RoleConfigurationAuditTests
{
    [Test]
    public void RoleRenameRemainsAuditedWhenLaterPermissionGrantFails()
    {
        var roles = new Mock<IRoleCommands>();
        var permissions = new Mock<IPermissionCommands>();
        var records = new List<string>();
        var writer = new Mock<ILogRepository>();
        writer.Setup(x => x.LogAsync(It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), LogRecordType.User))
            .Callback<string, LogRecordSeverity, LogRecordType>((message, _, _) => records.Add(message))
            .Returns(Task.CompletedTask);
        var actor = new AuthorizedUserStore { AuthorizedUser = new User { Id = 17, Username = "operator" } };
        var persistedName = "Original";
        roles.Setup(x => x.UpdateAsync(It.IsAny<RoleRecord>(), It.IsAny<RoleRecord>()))
            .ReturnsAsync((RoleRecord _, RoleRecord requested) =>
            {
                persistedName = requested.Name;
                return new RoleRecord { Id = 3, Name = persistedName };
            });
        permissions.Setup(x => x.CreateAsync(It.IsAny<PermissionRecord>()))
            .ThrowsAsync(new InvalidOperationException("permission storage unavailable"));
        var model = new UserRolesViewModel(actor, roles.Object, permissions.Object,
            Mock.Of<IUserRepository>(), writer.Object, Mock.Of<IDialogService>(),
            new ActionAuditService(writer.Object, actor), new EventAggregator())
        {
            SelectedUserRole = new UserRole(3, "Original")
        };
        model.UserRoleNameToEdit = "Changed";
        model.UserRoleToEdit!.Permissions.ClinicalData = true;

        model.SaveCommand.Execute();
        Assert.ThrowsAsync<InvalidOperationException>(() => model.UserRoleTask!.Task);

        Assert.That(persistedName, Is.EqualTo("Changed"));
        Assert.That(records, Has.Count.EqualTo(1));
        Assert.That(records.Single(), Does.Contain("id=3").And.Contain("operator"));
        Assert.That(records.Single(), Does.Not.Contain("permission storage unavailable"));
    }
}
