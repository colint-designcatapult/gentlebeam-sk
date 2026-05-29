using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Infra.DataManagement.Common
{
    public class UserRepository(
        IUserCommands userCommands,
        IUserRoleMappingCommands userRoleMappingCommands,
        IRoleCommands roleCommands,
        IPermissionCommands permissionCommands)
        : IUserRepository
    {
        public async Task<ICollection<IUser>> FetchUsersAsync()
        {
            var users = await userCommands.ReadAllAsync();

            foreach (var user in users)
            {
                user.Role = await FetchUserRoleAsync(user.Id);
            }

            return users;
        }

        public async Task SaveUserAsync(IUser userToSave)
        {
            await userCommands.UpdateAsync(null!, userToSave);
            var userRoleMapping = await userRoleMappingCommands.ReadAsync(userToSave.Id);
            userRoleMapping.RoleId = userToSave.Role.Id;

            await userRoleMappingCommands.UpdateAsync(null!, userRoleMapping);
        }

        public async Task CreateUserAsync(IUser userToCreate)
        {
            var storedUser = await userCommands.CreateAsync(userToCreate);

            var userRoleMapping = new UserRoleRecord
            {
                UserId = storedUser.Id,
                UserEmail = storedUser.EmailAddress,
                RoleId = userToCreate.Role.Id
            };

            await userRoleMappingCommands.CreateAsync(userRoleMapping);
        }

        public async Task DeleteUserAsync(long userId)
        {
            var userRoleMapping = await userRoleMappingCommands.ReadAsync(userId);
            await userRoleMappingCommands.DeleteAsync(userRoleMapping.Id);
            await userCommands.DeleteAsync(userId);
        }

        public async Task<UserRole> FetchUserRoleAsync(long userId)
        {
            var roleMappings = await userRoleMappingCommands.ReadListAsync(userId);
            var roleRecord = await roleCommands.ReadAsync(roleMappings.First().RoleId);
            var permissions = await FetchRolePermissionsAsync(roleMappings.First().RoleId);

            return new UserRole(roleRecord.Id, roleRecord.Name)
            {
                Permissions = permissions
            };
        }

        public async Task<ICollection<UserRole>> FetchAllUserRolesAsync()
        {
            var records = await roleCommands.ReadAllAsync();
            var roleList = new List<UserRole>();
            foreach (var roleRecord in records) 
            {
                var role = new UserRole(roleRecord.Id, roleRecord.Name);

                role.Permissions = await FetchRolePermissionsAsync(role.Id);

                roleList.Add(role);
            }

            return roleList;
        }

        private async Task<UserPermissions> FetchRolePermissionsAsync(long roleId)
        {
            var permissions = new UserPermissions();
            var permissionRecords = await permissionCommands.ReadListAsync(roleId);


            foreach (var record in permissionRecords)
            {
                permissions.UpdatePermission(new UserPermission(record));
            }
            return permissions;
        }
    }
}
