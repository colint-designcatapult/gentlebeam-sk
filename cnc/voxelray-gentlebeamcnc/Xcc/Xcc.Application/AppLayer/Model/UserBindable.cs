using System.ComponentModel.DataAnnotations;

using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Application.AppLayer.Model;

public sealed class UserBindable : DirtyFlaggedBindableBase
{
    public UserBindable()
    {
        AcceptChanges();
    }

    public UserBindable(IUser user)
    {
        Id = user.Id;
        Picture = user.Picture;
        FirstName = user.FirstName;
        MiddleName = user.MiddleName;
        LastName = user.LastName;
        Username = user.Username;
        Password = user.Password;
        EmailAddress = user.EmailAddress;
        Role = user.Role;

        PasswordConfirm = user.Password;

        AcceptChanges();
    }


    #region IUser
    public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

    private string _picture = string.Empty;
    public string Picture
    {
        get => _picture;
        set => SetPropertyWithDirtyFlag(ref _picture, value);
    }

    private string? _firstName;
    [Required(ErrorMessage = StringConstants.SystemSettings.UserManagement.Validation.FirstNameIsRequired)]
    public string? FirstName
    {
        get => _firstName;
        set
        {
            SetPropertyWithDirtyFlag(ref _firstName, value);
            Validate(value);
        }
    }

    private string _middleName = string.Empty;
    public string MiddleName
    {
        get => _middleName;
        set => SetPropertyWithDirtyFlag(ref _middleName, value);
    }

    private string? _lastName;
    [Required(ErrorMessage = StringConstants.SystemSettings.UserManagement.Validation.LastNameIsRequired)]
    public string? LastName
    {
        get => _lastName;
        set
        {
            SetPropertyWithDirtyFlag(ref _lastName, value);
            Validate(value);
        }
    }

    private string? _username = string.Empty;
    [Required(ErrorMessage = StringConstants.SystemSettings.UserManagement.Validation.UsernameIsRequired)]
    public string? Username
    {
        get => _username;
        set
        {
            SetPropertyWithDirtyFlag(ref _username, value);
            Validate(value);
        }
    }

    private string? _password;
    [Required(ErrorMessage = StringConstants.SystemSettings.UserManagement.Validation.PasswordIsRequired)]
    public string? Password
    {
        get => _password;
        set
        {
            SetPropertyWithDirtyFlag(ref _password, value);
            Validate(value);
            Validate(PasswordConfirm, nameof(PasswordConfirm));
        }
    }

    private string? _emailAddress;
    [Required(ErrorMessage = StringConstants.SystemSettings.UserManagement.Validation.EmailIsRequired)]
    public string? EmailAddress
    {
        get => _emailAddress;
        set
        {
            SetPropertyWithDirtyFlag(ref _emailAddress, value);
            Validate(value);
        }
    }


    private UserRole _role = UserRole.Guest;
    public UserRole Role
    {
        get => _role;
        set
        {
            SetPropertyWithDirtyFlag(ref _role, value);
            Validate(value);
        }
    }
    #endregion IUser


    private string? _passwordConfirm;
    [Required(ErrorMessage = StringConstants.SystemSettings.UserManagement.Validation.PasswordConfirmIsRequired)]
    [Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
    public string? PasswordConfirm
    {
        get => _passwordConfirm;
        set
        {
            SetPropertyWithDirtyFlag(ref _passwordConfirm, value);
            Validate(value);
            Validate(Password, nameof(Password));
        }
    }


    public IUser ToUser()
    {
        return new User
        {
            Id = this.Id,
            Picture = this.Picture,
            FirstName = this.FirstName ?? throw new(StringConstants.SystemSettings.UserManagement.Validation.FirstNameIsNotSetErrorMessage),
            MiddleName = this.MiddleName,
            LastName = this.LastName ?? throw new(StringConstants.SystemSettings.UserManagement.Validation.LastNameIsNotSetErrorMessage),
            Username = this.Username ?? throw new(StringConstants.SystemSettings.UserManagement.Validation.UsernameIsNotSetErrorMessage),
            Password = this.Password ?? throw new(StringConstants.SystemSettings.UserManagement.Validation.PasswordIsNotSetErrorMessage),
            EmailAddress = this.EmailAddress ?? throw new(StringConstants.SystemSettings.UserManagement.Validation.EmailIsNotSetErrorMessage),

            Role = this.Role
        };
    }
}