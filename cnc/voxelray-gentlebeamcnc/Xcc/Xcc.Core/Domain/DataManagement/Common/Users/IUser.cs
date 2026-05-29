using System;

namespace Xcc.Core.Domain.DataManagement.Common.Users
{
    public interface IUser : IEntry
    {
        DateTime CreationDate { get; set; }
        string Picture { get; set; }
        string FirstName { get; set; }
        string MiddleName { get; set; }
        string LastName { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string EmailAddress { get; set; }
        DateTime LastAccessed { get; set; }
        
        UserRole Role { get; set; }

        string Fullname();
    }
}
