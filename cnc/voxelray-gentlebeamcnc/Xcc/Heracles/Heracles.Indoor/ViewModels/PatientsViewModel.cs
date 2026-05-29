using Grpc.Core;

using Heracles.Application.Common;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models.UseCases;

using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels
{
    public class PatientProfileForm : DirtyFlaggedBindableBase, IPatientProfileForm
    {
        #region Properties

        private IValidatedPatient? _formData;
        public IValidatedPatient? FormData
        {
            get => _formData;
            set 
            {
                SetPropertyWithDirtyFlag(ref _formData, value);
                IsValid = _formData?.IsValid ?? false;
            }
        }

        private bool _isCalendarVisible = false;
        public bool IsCalendarVisible
        {
            get { return _isCalendarVisible; }
            set { SetProperty(ref _isCalendarVisible, value); }
        }

        public DateTime? _dob;
        public DateTime? DOB
        {
            get => _dob;
            set
            {
                SetProperty(ref _dob, value);
            }
        }

        public IAuthorizedUserStore UserStore { get; }

        #endregion Properties

        public PatientProfileForm()
        {
            FormData = null;
            UserStore = null!;
        }

        public PatientProfileForm(IAuthorizedUserStore userStore)
        {
            FormData = null;
            UserStore = userStore;
        }


        public void ShowForm(IPatient? patient)
        {
            IsCalendarVisible = false;

            SetFormData(patient);
        }

        public void HideForm()
        {
            IsCalendarVisible = false;

            FormData = null;
        }

        #region private methods
        private void SetFormData(IPatient? patient)
        {
            var bindablePatient = new Patient(patient);
            if (String.IsNullOrEmpty(bindablePatient.ProviderId))
            {
                bindablePatient.ProviderId = UserStore.AuthorizedUser?.EmailAddress ?? string.Empty;
            }

            // Subscrube to DOB change event to close calendar after such change:
            bindablePatient.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(bindablePatient.DOB))
                {
                    IsCalendarVisible = false;
                }
            };
            FormData = bindablePatient;
        }
        #endregion private methods
    }

    public class PatientsViewModel : RegionViewModelBase, IPatientSelection
    {
        #region Properties
        private string _searchPhrase = string.Empty;
        public string SearchPhrase
        {
            get => _searchPhrase;
            set
            {
                if(SetProperty(ref _searchPhrase, value))
                    PatientsViewSource.View?.Refresh();
            }
        }

        private CollectionViewSource _patientsViewSource = new();
        public CollectionViewSource PatientsViewSource 
        { 
            get => _patientsViewSource; 
            set => SetProperty(ref _patientsViewSource, value); 
        }

        private SortDescription _sortDescription;
        public SortDescription SortDescription
        {
            get => _sortDescription;
            set
            {
                var tmp = _sortDescription;
                if (SetProperty(ref _sortDescription, value))
                {
                    PatientsViewSource.SortDescriptions.Remove(tmp);
                    PatientsViewSource.SortDescriptions.Add(value);
                }
            }
        }

        private IPatient? _selectedPatient;
        public IPatient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    ValidateCanExecCommands();
                }
            }
        }

        private IPatientProfileForm? _patientProfileForm = null;
        public IPatientProfileForm? PatientProfileForm
        {
            get => _patientProfileForm;
            set
            {
                SetProperty(ref _patientProfileForm, value);
                if (value is not null)
                {
                    value.IsModifiedChanged += (s, e) => ValidateCanExecCommands();
                    value.IsValidChanged += (s, e) => ValidateCanExecCommands();
                }
            }
        }

        private PatientEditing? PatientEditing { get; set; } = null;
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public ITreatmentInfoStoreController TreatmentInfoStoreController { get; }
        public IEmrPlanCommands PlanCommands { get; }
        public ILogRepository LogWriter { get; }
        public IActionAuditService ActionAuditService { get; }
        public IPatientListModel PatientModel { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public DateTime DisplayDateEnd => DateTime.Now;
        public DateTime DisplayDateStart => DateTime.Now.AddYears(-100);
        #endregion Properties


        #region Observable tasks
        private ObservableTask? _fetchPatientListTask;
        public ObservableTask? FetchPatientListTask
        {
            get => _fetchPatientListTask;
            set => SetProperty(ref _fetchPatientListTask, value);
        }

        private DelegateCommand? _retryFetchPatientListFetchCommand;
        public DelegateCommand? RetryFetchPatientListCommand
        {
            get => _retryFetchPatientListFetchCommand;
            set => SetProperty(ref _retryFetchPatientListFetchCommand, value);
        }

        private DelegateCommand? _cancelFetchPatientListCommand;
        public DelegateCommand? CancelFetchPatientListCommand => _cancelFetchPatientListCommand ??= new(() => FetchPatientListTask = null);

        private ObservableTask? _savePatientTask;
        public ObservableTask? SavePatientTask
        {
            get => _savePatientTask;
            set => SetProperty(ref _savePatientTask, value);
        }

        private DelegateCommand? _retrySavePatientSaveCommand;
        public DelegateCommand? RetrySavePatientCommand
        {
            get => _retrySavePatientSaveCommand;
            set => SetProperty(ref _retrySavePatientSaveCommand, value);
        }

        private void SavePatient()
        {
            RetrySavePatientCommand = new DelegateCommand(() =>
            {
                SavePatientTask = new ObservableTask(
                    SavePatientAsync(), 
                    StringConstants.EMR.PatientSaveErrorMessage);
            });
            RetrySavePatientCommand.Execute();
        }
        #endregion Observable tasks


        #region Commands
        private DelegateCommand? _editPatientCommand;
        public DelegateCommand EditPatientCommand => _editPatientCommand ??= new DelegateCommand(
            () =>
            {
                PatientEditing = new PatientEditing(PatientProfileForm, this, PatientModel, patientToEdit: SelectedPatient);
                ValidateCanExecCommands();
            },
            () => PatientEditing is null && SelectedPatient is not null);

        private DelegateCommand? _newPatientCommand;
        public DelegateCommand NewPatientCommand => _newPatientCommand ??= new DelegateCommand(
            () =>
            {
                PatientEditing = new PatientEditing(PatientProfileForm, this, PatientModel, patientToEdit: null);
                ValidateCanExecCommands();
            },
            () => PatientEditing is null);

        private DelegateCommand? _savePatientCommand;
        public DelegateCommand SavePatientCommand => _savePatientCommand ??= new DelegateCommand(
            SavePatient,
            () => 
                PatientProfileForm.IsModified && 
                PatientProfileForm.IsValid);

        private DelegateCommand<IPatient>? _goToTreatmentCommand;
        public DelegateCommand<IPatient> GoToTreatmentCommand => _goToTreatmentCommand ??= new DelegateCommand<IPatient>(
            patient =>
            {
                TreatmentInfoStore.Patient = patient;
                GoToTreatment();
            },
            patient => true);

        private DelegateCommand? _cancelEditPatientCommand;
        public DelegateCommand CancelEditPatientCommand => _cancelEditPatientCommand ??= new DelegateCommand(
            () =>
            {
                PatientEditing?.Cancel();
                PatientEditing = null;
                ValidateCanExecCommands();
            },
            () => true);

        private DelegateCommand? _removeSearchPhraseCommand;
        public DelegateCommand RemoveSearchPhraseCommand => _removeSearchPhraseCommand ??= new DelegateCommand(
            () =>
            {
                SearchPhrase = string.Empty;
            },
            () => true);

        #endregion Commands


        #region Constructors
        public PatientsViewModel(
            IRegionManager regionManager, 
            IEventAggregator eventAggregator,
            ITreatmentInfoStore treatmentInfoStore,
            ITreatmentInfoStoreController treatmentInfoStoreController,
            IEmrPlanCommands planCommands,
            ILogRepository logWriter,
            IActionAuditService actionAuditService,
            IPatientListModel patientListModel,
            IDialogService dialogService,
            IAuthorizedUserStore authorizedUserStore)
            : base(regionManager, eventAggregator, dialogService)
        {
            TreatmentInfoStore = treatmentInfoStore;
            TreatmentInfoStoreController = treatmentInfoStoreController;
            PlanCommands = planCommands;
            LogWriter = logWriter;
            ActionAuditService = actionAuditService;
            PatientModel = patientListModel;
            AuthorizedUserStore = authorizedUserStore;
            PatientModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PatientModel.Patients))
                {
                    PatientsViewSource.Source = PatientModel.Patients;
                }
            };

            FetchPatientList();

            PatientsViewSource.Filter += (s, e) =>
            {
                IPatient? patient = e.Item as IPatient;

                if (patient == null)
                    e.Accepted = false;
                else
                    e.Accepted = Contains(patient, SearchPhrase);
            };

            SortDescription = new SortDescription(nameof(IPatient.LastName), ListSortDirection.Ascending);

            PatientProfileForm = new PatientProfileForm(authorizedUserStore);
        }

        public PatientsViewModel() : base(null) 
        {
            PatientProfileForm = new PatientProfileForm()
            {
                FormData = new Patient()
            };
        }
        #endregion Constructors

        #region Private methods
        private async Task FetchPatientListAsync()
        {
            try
            {
                // TODO: refactor this
                // As we may call retry the same task failed,
                // and this failure may be at the plan recovery step,
                // we don't need to fetch any patient data again
                var patientList = PatientModel.Patients;
                if (patientList is null || patientList.Count == 0)
                {
                    await PatientModel.QueryPatientsAsync();
                }
            }
            catch (FetchPatientsException ex)
            {
                //DialogService.ReportError(StringConstants.EMR.PatientListErrorTitle, StringConstants.EMR.PatientListFetchError);

                _ = LogWriter.LogAsync($"{ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
            catch (Exception ex)
            {
                //DialogService.ReportError(StringConstants.EMR.PatientListErrorTitle, StringConstants.EMR.PatientListFetchError);

                _ = LogWriter.LogAsync($"{StringConstants.EMR.PatientListFetchError}. {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }           
        }

        private async Task TaskToFetchPatientsAndTryLockAsync()
        {
            await FetchPatientListAsync();
            // Only after we fetched the patients completely, 
            // we try to find a loaded/pending plan and navigate to it,
            // updating task's error title correspondingly:
            if (FetchPatientListTask is not null)
                FetchPatientListTask.SetErrorMessage(Xcc.Core.Constants.StringConstants.EMR.Patients.PatientUnderTreatmentLookupError);
            await TryLockOnLoadedPlan();
        }

        private void FetchPatientList()
        {
            RetryFetchPatientListCommand = new DelegateCommand(() =>
            {
                FetchPatientListTask = new ObservableTask(
                    TaskToFetchPatientsAndTryLockAsync(),
                    StringConstants.EMR.PatientListFetchError);
            });
            RetryFetchPatientListCommand.Execute();
        }

        private async Task TryLockOnLoadedPlan()
        {
            var existingPlan = await TreatmentInfoStoreController.TryPrepareLoadedPlanAsync();
            if (existingPlan != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(GoToTreatment);
            }
        }

        private void ValidateCanExecCommands()
        {
            SavePatientCommand.RaiseCanExecuteChanged();
            EditPatientCommand.RaiseCanExecuteChanged();
            NewPatientCommand.RaiseCanExecuteChanged();
        }

        private bool Contains(IPatient patient, string searchPhrase)
        {
            if (string.IsNullOrWhiteSpace(searchPhrase))
                return true;

            char whitespace = ' ';

            var searchPhraseTerms = searchPhrase.Split(whitespace, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string patientAsString =
                patient.MRN + Environment.NewLine +
                patient.FirstName + Environment.NewLine +
                patient.MiddleName + Environment.NewLine +
                patient.LastName + Environment.NewLine +
                patient.DOB + Environment.NewLine +
                patient.Sex;

            var firstNameLastName = patient.FirstName.Trim() + whitespace + patient.LastName.Trim();
            var lastNameFirstName = patient.LastName.Trim() + whitespace + patient.FirstName.Trim();

           string possibleFullNamePhrase = string.Join(whitespace, searchPhraseTerms);

            if(firstNameLastName.Contains(possibleFullNamePhrase, StringComparison.OrdinalIgnoreCase)) 
                return true;

            if(lastNameFirstName.Contains(possibleFullNamePhrase, StringComparison.OrdinalIgnoreCase)) 
                return true;

            return patientAsString.Contains(searchPhrase, StringComparison.OrdinalIgnoreCase);
        }

        private async Task SavePatientAsync()
        {
            try
            {
                AuditReportOnStartSavingPatientAsync(PatientProfileForm.FormData);

                IPatient savedPatient = await PatientEditing.SavePatientAsync();
                if (savedPatient != null)
                {
                    PatientEditing = null;
                    ValidateCanExecCommands();

                    AuditReportOnSavePatientCompleteAsync(savedPatient);
                }
            }
            catch (DataServiceException dEx)
            {
                if (dEx.InnerException is not null)
                {
                    if (dEx.InnerException is RpcException { StatusCode: StatusCode.AlreadyExists } rpcException)
                    {
                        _ = LogWriter.LogAsync(
                            $"{StringConstants.EMR.PatientSaveErrorMessage} {rpcException.Message}", 
                            LogRecordSeverity.Error, 
                            LogRecordType.System);

                        DialogService.ReportError(
                            StringConstants.EMR.PatientAlreadyExistsErrorTitle,
                            StringConstants.EMR.PatientAlreadyExistsErrorMessage);
                        return;
                    }
                }

                _ = LogWriter.LogAsync(
                    $"{StringConstants.EMR.PatientSaveErrorMessage} {dEx.Message}. {dEx.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
            catch (Exception ex)
            {
                string message = $"{StringConstants.EMR.PatientSaveErrorMessage} {ex.Message}";
                _ = LogWriter.LogAsync(
                    message,
                    LogRecordSeverity.Error, 
                    LogRecordType.System);

                return;
            }
        }

        private void AuditReportOnStartSavingPatientAsync(IPatient patientProfile)
        {
            var user = AuthorizedUserStore.AuthorizedUser;

            string action = 
                (BaseEntry.IsBlankEntry(patientProfile)) 
                ? StringConstants.EMR.SaveNewPatientAuditLogMessage
                : $"{StringConstants.EMR.SaveExistingPatientAuditLogMessage} id={patientProfile.Id}";
            
            ActionAuditService.RegisterAction(action);
        }

        private void AuditReportOnSavePatientCompleteAsync(IPatient patientProfile)
        {
            var user = AuthorizedUserStore.AuthorizedUser;

            string action = $"{StringConstants.EMR.SavePatientIsDoneAuditLogMessage} id={patientProfile.Id}";

            ActionAuditService.RegisterAction(action);
        }

        private void GoToTreatment()
        {
            SelectedPatient = TreatmentInfoStore.Patient;
            RegionManager?.RequestNavigate(Regions.MainRegion, "ClinicalDataView");
        }


        #endregion Private methods
    }
}
