using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Treatment;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Threading.Tasks;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels;

public class PatientProfileViewModel : BindableBase
{
    #region Constructors
    public PatientProfileViewModel(
        ITreatmentInfoStore treatmentInfoStore, 
        ILogWriter logWriter, 
        IPatientListModel patientListModel,
        IDialogService dialogService)
    {
        TreatmentInfoStore = treatmentInfoStore;
        LogWriter = logWriter;
        PatientListModel = patientListModel;
        DialogService = dialogService;
        ProfilePicture = treatmentInfoStore.Patient?.Picture;
        treatmentInfoStore.PatientChanged += (_, patient) => ProfilePicture = patient?.Picture;
    }
    #endregion Constructors


    #region Injected Dependencies
    public ITreatmentInfoStore TreatmentInfoStore { get; }
    private ILogWriter LogWriter { get; }
    private IPatientListModel PatientListModel { get; }
    public IDialogService DialogService { get; }
    #endregion Injected Dependencies


    #region Properties
    private string? _profilePicture;
    public string? ProfilePicture
    {
        get => _profilePicture;
        set => SetProperty(ref _profilePicture, value);
    }

    private DelegateCommand? _loadPictureCommand;
    public DelegateCommand LoadPictureCommand => _loadPictureCommand ??= new DelegateCommand(
        () =>
        {
            try
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Multiselect = false,
                    Filter = "Image files(*.jpg;*.jpeg;*.png;*.bmp;)|*.jpg;*.jpeg;*.png;*.bmp"
                };

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePathValidation.CheckTraversalSecurity(openFileDialog.FileName);
                    SavePatientPicture(openFileDialog.FileName);
                }
            }
            catch (FilePathValidationException ex)
            {
                DialogService.ReportError(StringConstants.Common.Validation.FilePathErrorTitle, ex.Message);
            }
            catch (Exception ex)
            {
                DialogService.ReportError(StringConstants.Common.ErrorTitle, ex.Message);
            }
        });
    #endregion Properties


    #region Observable task properties
    private ObservableTask? _currentTask;
    public ObservableTask? CurrentTask
    {
        get => _currentTask;
        set => SetProperty(ref _currentTask, value);
    }

    private DelegateCommand? _retryCurrentTaskCommand;
    public DelegateCommand? RetryCurrentTaskCommand
    {
        get => _retryCurrentTaskCommand;
        set => SetProperty(ref _retryCurrentTaskCommand, value);
    }

    private DelegateCommand? _cancelCurrentTaskCommand;
    public DelegateCommand? CancelCurrentTaskCommand
    {
        get => _cancelCurrentTaskCommand;
        set => SetProperty(ref _cancelCurrentTaskCommand, value);
    }
    #endregion Observable task properties


    #region Private methods
    private void SavePatientPicture(string profilePicture)
    {
        CurrentTask = new ObservableTask(SavePatientPictureAsync(profilePicture), StringConstants.EMR.PatientProfile.SaveUiErrorMessage);

        RetryCurrentTaskCommand = new DelegateCommand(() =>
        {
            CurrentTask = new ObservableTask(SavePatientPictureAsync(profilePicture), StringConstants.EMR.PatientProfile.SaveUiErrorMessage);
        });

        CancelCurrentTaskCommand = new DelegateCommand(() => CurrentTask = null);
    }

    private async Task SavePatientPictureAsync(string profilePicture)
    {
        try
        {
            if (TreatmentInfoStore.Patient is null)
                throw new ArgumentNullException(StringConstants.EMR.PatientIsNotSelectedErrorMessage);

            var patientToSave = new Patient(TreatmentInfoStore.Patient)
            {
                Picture = profilePicture
            };

            await PatientListModel.SavePatientAsync(patientToSave);

            TreatmentInfoStore.Patient.Picture = profilePicture;
            ProfilePicture = profilePicture;
        }
        catch (Exception exception)
        {
            _ = LogWriter.LogAsync(
                $"{StringConstants.EMR.PatientProfile.SaveErrorMessage} {exception.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }
    #endregion Private methods
}