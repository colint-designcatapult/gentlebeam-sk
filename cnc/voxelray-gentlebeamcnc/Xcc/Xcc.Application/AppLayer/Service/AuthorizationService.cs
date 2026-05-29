using Grpc.Core;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Application.AppLayer.Service
{
    public class AuthorizationService(
        IAuthCommands authCommands,
        IBearerTokenUserSessionManager userSessionManager,
        IUserRepository userRepository) : IAuthorizationService
    {
        public async Task<IUser> LoginAsync(string username, string password)
        {
            await AuthenticateUserAsync(username, password);
            return await FetchAuthorizedUserAsync(username, password);
        }

        private async Task AuthenticateUserAsync(string username, string password)
        {
            try
            {
                var bearerToken = await authCommands.AuthenticateUserAsync(username, password);
                if (userSessionManager.UserSession.Username != username)
                {
                    userSessionManager.StartUserSession(username, bearerToken);
                }
                else
                {
                    // TODO: should we reset user permissions here,
                    // just in case if some of them were removed, to prevent from getting them?
                    userSessionManager.UnlockUserSession(username, bearerToken);
                }
            }
            catch (RpcException rpcException)
            {
                var errorMessage = rpcException.StatusCode switch
                {
                    StatusCode.DeadlineExceeded => StringConstants.Common.Authorization.NetworkErrorNoConnection, //no connection
                    StatusCode.Unavailable => StringConstants.Common.Authorization.NetworkErrorNoConnection, //no connection
                    StatusCode.Unauthenticated => StringConstants.Common.Authorization.NetworkCredentialsError, // invalid username/password
                    StatusCode.Internal => StringConstants.Common.Authorization.DbInternalError,
                    _ => StringConstants.Common.Authorization.UnknownError, //if you got here, this case should be processed separately.
                };
                throw new AuthorizationServiceException(errorMessage, rpcException.Message, rpcException);
            }
            catch (Exception ex)
            {
                var errorMessage = StringConstants.Common.Authorization.UnknownError;
                throw new AuthorizationServiceException(errorMessage, ex.Message, ex);
            }
        }

        private async Task<IUser> FetchAuthorizedUserAsync(string username, string password)
        {
            try
            {
                var user = await GetUserAsync(username);
                return await AuthorizeUserAsync(user);
            }
            catch (SecurityException ex)
            {
                var errorMessage = StringConstants.Common.Authorization.AuthorizationError;
                throw new AuthorizationServiceException(errorMessage, ex.Message, ex);
            }
            catch (Exception ex)
            {
                var errorMessage = StringConstants.Common.Authorization.UserDatabaseError;
                throw new AuthorizationServiceException(errorMessage, ex.Message, ex);
            }
        }

        public async Task<IUser> GetUserAsync(string username)
        {
            var users = await userRepository.FetchUsersAsync();

            var user = users.FirstOrDefault(u => u.Username.Equals(username));
            if (user == null)
            {
                throw new Exception($"User {username} is not found");
            }

            return user;
        }

        public async Task<IUser> AuthorizeUserAsync(IUser user)
        {
            // Now we fetch roles on every authorization attempt
            // to be sure that we get actual DB state of permissions
            var roles = await userRepository.FetchAllUserRolesAsync();

            var storedRole = roles.FirstOrDefault(role => role.Name.Equals(user.Role.Name));

            if (storedRole != null)
            {
                return new User(user) { Role = storedRole };
            }
            else
            {
                throw new SecurityException($"Access denied. Unknown user role name: {user.Role?.Name}");
            }
        }
    }
}
