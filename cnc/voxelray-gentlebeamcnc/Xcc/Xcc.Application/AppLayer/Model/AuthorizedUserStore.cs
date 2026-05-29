using System;
using Prism.Mvvm;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Application.AppLayer.Model
{
    public interface IAuthorizedUserStore
    {
        IUser? AuthorizedUser { get; set; }

        public event EventHandler<IUser?>? AuthorizedUserChanged;
    }

    public class AuthorizedUserStore : BindableBase, IAuthorizedUserStore
    {
        private IUser? _authorizedUser;
        public IUser? AuthorizedUser
        {
            get => _authorizedUser;
            set
            {
                if (SetProperty(ref _authorizedUser, value))
                {
                    AuthorizedUserChanged?.Invoke(this, _authorizedUser);
                }
            }
        }

        public event EventHandler<IUser?>? AuthorizedUserChanged;
    }
}
