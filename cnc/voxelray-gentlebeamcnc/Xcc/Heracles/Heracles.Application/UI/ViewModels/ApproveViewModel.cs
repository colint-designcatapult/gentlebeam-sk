using Grpc.Core;
using Heracles.Core.Enums;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;
using StringConstants = Xcc.Core.Constants.StringConstants;

namespace Heracles.Application.UI.ViewModels
{
    public class ApproveViewModel : DialogViewModelBase
    {
        

        public ApproveViewModel() 
        {
            if (System.Windows.Application.Current.MainWindow is not null)
            {
                throw new System.Exception("This constructor can be used only in design mode.");
            }
        }

        public ApproveViewModel(ILogRepository logWriter, IAuthCommands authCommands)
        {
            Title = Common.StringConstants.Authentication.VerifyDialogTitle;
            LogWriter = logWriter;
            AuthCommands = authCommands;
        }

        #region Read-only properties
        public ILogRepository LogWriter { get; }
        public IAuthCommands AuthCommands { get; }
        #endregion  Read-only properties


        #region Properties
        public override string Title { set; get; } = Common.StringConstants.Authentication.VerifyDialogTitle;

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(_username) == false)
                {
                    SignInAndSaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(_password) == false)
                {
                    SignInAndSaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } set { SetProperty(ref _errorMessage, value); } }


        private IList<PlanStatus> _availableStatuses;
        public IList<PlanStatus> AvailableStatuses { 
            get => _availableStatuses; 
            set => SetProperty(ref _availableStatuses, value); 
        }

        private PlanStatus? _status;
        public PlanStatus? Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {                    
                    SignInAndSaveCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion Properties


        #region Commands
        private DelegateCommand? _signInAndSaveCommand;
        public DelegateCommand SignInAndSaveCommand
        {
            get => _signInAndSaveCommand ??= new DelegateCommand(
                async () =>
                {
                    try
                    {
                        await AuthCommands.AuthenticateUserAsync(Username, Password);
                    }
                    catch (RpcException rpc)
                    {
                        ErrorMessage = rpc.StatusCode switch
                        {
                            StatusCode.DeadlineExceeded => StringConstants.Common.Authorization.NetworkErrorNoConnection,
                            StatusCode.Unavailable => StringConstants.Common.Authorization.NetworkErrorNoConnection,
                            StatusCode.Unauthenticated => StringConstants.Common.Authorization.NetworkCredentialsError, // invalid username/password
                            StatusCode.Internal => StringConstants.Common.Authorization.DbInternalError,
                            _ => StringConstants.Common.Authorization.UnknownError, //if you got here, this case should be processed separately.
                        };

                        _ = LogWriter.LogAsync(
                            $"{StringConstants.Common.Authorization.AuthenticationErrorLogMessage}: {rpc.Message}", 
                            Xcc.Core.Enums.LogRecordSeverity.Error, 
                            Xcc.Core.Enums.LogRecordType.System);

                        return;
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = StringConstants.Common.Authorization.UnknownError; //if you got here, this case should be processed separately.
                        
                        _ = LogWriter.LogAsync(
                            $"{StringConstants.Common.Authorization.AuthenticationErrorLogMessage}: {ex.Message}", 
                            Xcc.Core.Enums.LogRecordSeverity.Error, 
                            Xcc.Core.Enums.LogRecordType.System);

                        return;
                    }

                    DialogParameters parameters = new()
                    {
                        { "Status", Status.Value },
                        { "Username", Username },
                        { "Password", Password },
                    };

                    CloseDialog(parameters);
                },
                canExecuteMethod: () => Status.HasValue);
        }


        private DelegateCommand? _cancelCommand;

        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(
            () =>
            {
                CancelDialog();
            });

        protected override void SetDialogParameters(IDialogParameters parameters)
        {
            base.SetDialogParameters(parameters);
            if (parameters.TryGetValue("CurrentStatus", out PlanStatus currentStatus))
                AvailableStatuses = Enum.GetValues<PlanStatus>().Where(status => status != currentStatus).ToList();
        }
        #endregion Commands
    }
}