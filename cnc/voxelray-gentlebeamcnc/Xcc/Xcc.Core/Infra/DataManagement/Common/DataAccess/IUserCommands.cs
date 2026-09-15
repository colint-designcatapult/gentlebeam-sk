using Xcc.Core.Domain.DataManagement.Common.Users;
using System.Threading.Tasks;

namespace Xcc.Core.Infra.DataManagement.Common.DataAccess
{
    public interface IUserCommands : IAsyncRootEntryCommands<IUser>
    {
        Task ResetUserLockoutAsync(long userId);
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
