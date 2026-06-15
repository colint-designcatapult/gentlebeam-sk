using System.Threading.Tasks;

namespace Xcc.Core.Infra.DataManagement.Common.DataAccess
{
    public interface IAuthCommands
    {
        Task<string> AuthenticateUserAsync(string username, string password);
    }

}
