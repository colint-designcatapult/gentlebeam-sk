using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Core.Infra.DataManagement.Common.DataAccess
{
    public interface IUserCommands : IAsyncRootEntryCommands<IUser>
    {
    }

    public interface IUserRoleMappingCommands : IAsyncChildEntryCommands<UserRoleRecord>
    {
    }

    public interface IRoleCommands : IAsyncRootEntryCommands<RoleRecord>
    {
    }

    public interface IPermissionCommands : IAsyncChildEntryCommands<PermissionRecord>
    {
    }
}
