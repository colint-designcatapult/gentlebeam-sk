using System.Collections.Generic;
using System;
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

        public Task ResetUserLockoutAsync(long userId) => userCommands.ResetUserLockoutAsync(userId);

        public async Task SaveUserAsync(IUser userToSave, Action<string>? auditCommittedChange = null)
        {
            var previous = auditCommittedChange is null ? null : new User(await userCommands.ReadAsync(userToSave.Id));
            var hasRequestedChanges = previous is not null && HasEditableChanges(previous, userToSave);
            var storedUser = await userCommands.UpdateAsync(null!, userToSave);
            if (hasRequestedChanges && previous is not null && HasEditableChanges(previous, storedUser))
                auditCommittedChange?.Invoke($"Update user configuration id={storedUser.Id}");

            var userRoleMapping = await userRoleMappingCommands.ReadAsync(userToSave.Id);
            var previousRoleId = userRoleMapping.RoleId;
            userRoleMapping.RoleId = userToSave.Role.Id;
            var storedMapping = await userRoleMappingCommands.UpdateAsync(null!, userRoleMapping);
            if (storedMapping.RoleId != previousRoleId)
                auditCommittedChange?.Invoke($"Change user role userId={userToSave.Id} roleId={storedMapping.RoleId}");
        }

        public async Task CreateUserAsync(IUser userToCreate, Action<string>? auditCommittedChange = null)
        {
            var storedUser = await userCommands.CreateAsync(userToCreate);
            if (storedUser.Id > 0)
                auditCommittedChange?.Invoke($"Create user configuration id={storedUser.Id}");

            var userRoleMapping = new UserRoleRecord
            {
                UserId = storedUser.Id,
                UserEmail = storedUser.EmailAddress,
                RoleId = userToCreate.Role.Id
            };

            var storedMapping = await userRoleMappingCommands.CreateAsync(userRoleMapping);
            if (storedMapping.Id > 0)
                auditCommittedChange?.Invoke($"Assign user role userId={storedUser.Id} roleId={storedMapping.RoleId}");
        }

        public async Task DeleteUserAsync(long userId, Action<string>? auditCommittedChange = null)
        {
            var userRoleMapping = await userRoleMappingCommands.ReadAsync(userId);
            if (await userRoleMappingCommands.DeleteAsync(userRoleMapping.Id))
                auditCommittedChange?.Invoke($"Remove user role userId={userId} roleId={userRoleMapping.RoleId}");
            if (await userCommands.DeleteAsync(userId))
                auditCommittedChange?.Invoke($"Delete user configuration id={userId}");
        }

        private static bool HasEditableChanges(IUser before, IUser after) =>
            before.Username != after.Username || before.Password != after.Password ||
            before.FirstName != after.FirstName || before.MiddleName != after.MiddleName ||
            before.LastName != after.LastName || before.EmailAddress != after.EmailAddress ||
            before.Picture != after.Picture || before.Role.Name != after.Role.Name;

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
