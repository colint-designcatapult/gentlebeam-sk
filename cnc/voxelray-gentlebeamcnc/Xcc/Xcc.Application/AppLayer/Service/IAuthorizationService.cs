using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Application.AppLayer.Service
{
    public class AuthorizationServiceException : Exception
    {
        public AuthorizationServiceException(string message, string details, Exception innerException) 
            : base(message, innerException)
        {
            Details = details;
        }

        public string Details { get; }
    }

    public interface IAuthorizationService
    {
        /// <summary>
        /// Authenticates and authorizes the user in the system by their credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <exception cref="AuthorizationServiceException">
        /// Thrown when cannot authenticate or authorize the user
        /// </exception>
        /// <returns>Authorized user instance</returns>
        public Task<IUser> LoginAsync(string username, string password);
    }
}