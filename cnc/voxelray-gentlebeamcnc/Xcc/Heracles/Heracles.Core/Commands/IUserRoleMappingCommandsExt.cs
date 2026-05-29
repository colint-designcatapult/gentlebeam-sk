using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Core.Commands
{
    public interface IUserRoleMappingCommandsExt : IUserRoleMappingCommands
    {
        Task<ICollection<UserRoleRecord>> ReadListAsync(string userEmail);
    }
}
