using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Xcc.Core.Domain.DataManagement.Common.Users.DataAccess
{
    public interface IUserRepository
    {
        Task<ICollection<UserRole>> FetchAllUserRolesAsync();
        Task<ICollection<IUser>> FetchUsersAsync();
        Task SaveUserAsync(IUser userToSave, Action<string>? auditCommittedChange = null);
        Task CreateUserAsync(IUser userToCreate, Action<string>? auditCommittedChange = null);
        Task DeleteUserAsync(long userId, Action<string>? auditCommittedChange = null);
        Task ResetUserLockoutAsync(long userId);
    }
}
