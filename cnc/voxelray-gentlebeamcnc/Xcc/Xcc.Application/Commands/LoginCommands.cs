namespace Xcc.Application.Commands
{
    public class LoginCommands
    {
        public static bool CanLogin(string username, string password)
        {
            return !string.IsNullOrEmpty(username);
        }
    }
}
