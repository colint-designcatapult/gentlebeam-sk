using System;
using Xcc.Core.Common;

namespace Xcc.Core.Domain.DataManagement.Common.Users
{
    public class User : IUser
    {
        public User()
        {
        }

        public User(IUser entry)
        {
            entry?.CopyProperties(this);
        }

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public DateTime CreationDate { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Guest;

        public string EmailAddress { get; set; } = string.Empty;

        public DateTime LastAccessed { get; set; }

        public string Picture { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Fullname()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
