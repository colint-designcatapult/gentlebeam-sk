using System.Linq;

namespace Xcc.Core.Domain.DataManagement.Common.Users
{
    public class UserRole : RoleRecord
    {
        public static class BuiltInNames
        {
            public const string Administrator = "Administrator";
            public const string Rtt = "RTT";
            public const string Service = "Service";
            public const string Physicist = "Physicist";
            public static string[] Names = [Administrator, Rtt, Service, Physicist]; // [M2SG-612] 
        }

        /// <summary>
        /// Default guest role without any permissions
        /// </summary>
        public static UserRole Guest { get; } = new UserRole("Guest");
        public UserPermissions Permissions { get; set; }
        public bool BuiltIn { get; set; }
        
        public UserRole(string name)
        {
            Name = name;
            Permissions = new UserPermissions();
            BuiltIn = IsBuiltIn(Name);
        }

        public UserRole(long id, string name)
        {
            Id = id;
            Name = name;
            Permissions = new UserPermissions();
            BuiltIn = IsBuiltIn(Name);
        }

        public UserRole(UserRole role)
        {
            Id = role.Id;
            Name = role.Name;
            Permissions = new UserPermissions(role.Permissions);
            BuiltIn = IsBuiltIn(Name);
        }

        private bool IsBuiltIn(string name)
        {
            return BuiltInNames.Names.Contains(name);
        }
    }
}
