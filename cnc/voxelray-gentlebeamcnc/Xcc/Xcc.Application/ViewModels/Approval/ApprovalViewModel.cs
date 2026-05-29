using Grpc.Core;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Constants;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Xcc.Application.ViewModels.Approval
{
    public class ApprovalViewModel(
        ILogWriter logWriter) : DialogViewModelBase, IDialogAware
    {
        private IApprovalAction? _approvalAction;
        public const string ApprovalActionParameterName = "ApprovalAction";

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
            Title = StringConstants.QualityCheck.ApproveQcSampleDialogTitle;
            if (parameters.TryGetValue(ApprovalActionParameterName, out IApprovalAction action) && action is not null)
            {
                _approvalAction = action;
            }
            else
            {
                throw new NullReferenceException("No approval action specified");
            }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ApproveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ApproveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _revealPassword;
        public bool RevealPassword
        {
            get => _revealPassword;
            set => SetProperty(ref _revealPassword, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get { return _errorMessage; } set { SetProperty(ref _errorMessage, value); } }


        #region Commands
        private DelegateCommand? _approveCommand;
        public DelegateCommand? ApproveCommand => _approveCommand ??= new DelegateCommand(
            async () =>
            {

                ErrorMessage = string.Empty;
                try
                {
                    if (_approvalAction != null)
                    {
                        await _approvalAction.ApproveAsync(Username, Password);
                    }
                    CloseDialog();
                }
                catch (DataServiceException dataServiceException)
                {
                    string details = string.Empty;
                    if (dataServiceException.InnerException is RpcException rpc)
                    {
                        ErrorMessage = rpc.StatusCode switch
                        {
                            StatusCode.DeadlineExceeded => StringConstants.Common.Authorization.NetworkErrorNoConnection,
                            StatusCode.Unavailable => StringConstants.Common.Authorization.NetworkErrorNoConnection,
                            StatusCode.Unauthenticated => StringConstants.Common.Authorization.NetworkCredentialsError, // invalid username/password
                            StatusCode.Internal => StringConstants.Common.Authorization.DbInternalError,
                            _ => StringConstants.Common.Authorization.UnknownError, //if you got here, this case should be processed separately.
                        };
                        details = rpc.Message;
                    }
                    else
                    {
                        ErrorMessage = StringConstants.Common.Authorization.UnknownError;
                        details = dataServiceException.Message;
                    }

                    _ = logWriter.LogAsync(
                        $"{StringConstants.Common.Authorization.AuthenticationErrorLogMessage}: {details}",
                        Core.Enums.LogRecordSeverity.Error,
                        Core.Enums.LogRecordType.System);

                    return;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message; // StringConstants.Common.Authorization.UnknownError; //if you got here, this case should be processed separately.

                    _ = logWriter.LogAsync(
                        $"{StringConstants.Common.Authorization.AuthenticationErrorLogMessage}: {ex.Message}",
                        Core.Enums.LogRecordSeverity.Error,
                        Core.Enums.LogRecordType.System);

                    return;
                }
            },
            canExecuteMethod: () => 
                !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password));


        private DelegateCommand? _cancelCommand;
        public DelegateCommand? CancelCommand => _cancelCommand ??= new DelegateCommand(
            () =>
            {
                CancelDialog();
            });
        #endregion Commands
    }
}