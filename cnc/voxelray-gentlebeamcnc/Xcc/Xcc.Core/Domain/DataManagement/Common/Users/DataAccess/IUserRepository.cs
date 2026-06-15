using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xcc.Core.Domain.DataManagement.Common.Users.DataAccess
{
    public interface IUserRepository
    {
        Task<ICollection<UserRole>> FetchAllUserRolesAsync();
        Task<ICollection<IUser>> FetchUsersAsync();
        Task SaveUserAsync(IUser userToSave);
        Task CreateUserAsync(IUser userToCreate);
        Task DeleteUserAsync(long userId);
    }
}
