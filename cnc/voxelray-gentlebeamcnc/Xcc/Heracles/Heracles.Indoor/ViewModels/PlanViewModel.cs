using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient;
using Heracles.Application.Common;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Constants;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Views;

using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xcc.Application.AppLayer.Model;
using Xcc.Application.UI;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels;

public class PlanViewModel : TreatmentViewModelBase
{
    #region Constructors
    public PlanViewModel(
        IAuthorizedUserStore authorizedUserStore,
        IDialogService dialogService,
        IPopUpService popUpService,
        IDisruptiveActionGuardService disruptiveActionGuardService,
        IEventAggregator eventAggregator,
        LoadForTreatmentEventSource loadForTreatmentEventSource,
        PlanEventSource planEventSource,
        ILogWriter logWriter,
        IPatientRepository patientRepository,
        ICollimatorModel collimatorModel,
        IPlanModel planModel,
        IRegionManager regionManager,
        ITreatmentDoseCalculation treatmentDoseCalculation,
        ITreatmentHistoryModel treatmentHistoryModel,
        ITreatmentInfoStore treatmentInfoStore,
        IPhotoService photoService) :
        base(
            regionManager,
            logWriter,
            eventAggregator,
            dialogService,
            disruptiveActionGuardService,
            treatmentInfoStore,
            collimatorModel,
            planModel)
    {
        // Assignments
        AuthorizedUserStore = authorizedUserStore;
        PopUpService = popUpService;
        PatientRepository = patientRepository;
        TreatmentDoseCalculation = treatmentDoseCalculation;
        TreatmentHistoryModel = treatmentHistoryModel;
        PhotoService = photoService;

        //Event subscriptions
        PlanModel.IsModifiedChanged += (s, e) => VerifyCommand.RaiseCanExecuteChanged();
        PlanModel.IsValidChanged += (s, e) => VerifyCommand.RaiseCanExecuteChanged();
        PlanModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PlanModel.Plan))
            {
                VerifyCommand.RaiseCanExecuteChanged();
            }
        };
        eventAggregator.GetEvent<PhotoSavedEvent>().Subscribe(photoDescription => 
        {
            RefreshPhotosAsync();
        });
        TreatmentHistoryModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TreatmentHistoryModel.Treatments))
            {
                VerifyCommand.RaiseCanExecuteChanged();
            }
        };

        loadForTreatmentEventSource.LoadForTreatmentEvent += (_, e) => OnPlanEvent(e.Plan);
        planEventSource.PlanChangedEvent += (_, e) => OnPlanEvent(e);
    }

    #endregion Constructors



    #region Injected Dependencies
    public IPatientRepository PatientRepository { get; }
    public ITreatmentHistoryModel TreatmentHistoryModel { get; }
    public ITreatmentDoseCalculation TreatmentDoseCalculation { get; }
    public IAuthorizedUserStore AuthorizedUserStore { get; }
    public IPopUpService PopUpService { get; }
    public IPhotoService PhotoService { get; }

    #endregion Injected Dependencies

    #region Properties
    private Task CurrentTask { get; set; }

    //TODO:
    //public ObservableCollection<ISeries> SeriesList { get; set; }
    public ObservableCollection<string> SeriesList { get; set; }

    #endregion Properties



    #region Commands
    private DelegateCommand<IPhoto>? _selectPhotoCommand;
    public DelegateCommand<IPhoto> SelectPhotoCommand => _selectPhotoCommand ??= new DelegateCommand<IPhoto>(
        (photo) =>
        {
            if (photo is not null)
            {
                try
                {
                    var parameters = new DialogParameters { { "photo", photo } };
                    DialogService.ShowDialog("PhotoViewerModalView", parameters, result =>
                    {
                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        });
    
    private async void RefreshPhotosAsync()
    {
        if (TreatmentInfoStore.Diagnosis is null)
        {
            TreatmentInfoStore.Photos = new ObservableCollection<IPhoto>();
            return;
        }
        
        try
        {
            var photoResult = await PhotoService.GetPhotosAsync(TreatmentInfoStore.Diagnosis.Id);
            TreatmentInfoStore.Photos = photoResult.photos;
        }
        catch (Exception ex)
        {
            // Log but don't crash - photos are optional
        }
    }

    private DelegateCommand? _verifyCommand;
    public DelegateCommand VerifyCommand => _verifyCommand ??= new DelegateCommand(
        () =>
        {
            try
            {
                // Check if plan has any fields with dwell time of 300+ seconds,
                // it is permitted now by the hardware to load such high values
                // TODO: duplication with LoadForTreatment
                if (PlanModel.Plan.Status != PlanStatus.APPROVED
                    && PlanModel.TreatmentFields.Any(tf => tf.DwellTime >= ClinicalDataConstants.DwellTimeLimit))
                {
                    ShowDialog(
                        StringConstants.EMR.PlanDwellTimeLimitExceededErrorTitle,
                        StringConstants.EMR.PlanDwellTimeLimitExceededErrorMessage);

                    return;
                }

                PlanModel.ShowVerifyDialog();
            }
            catch (Exception ex)
            {
                _= LogWriter.LogAsync($"Failed to verify: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
            }
        },
        canExecuteMethod: CanVerify);

    private bool CanVerify()
    {
        //System.Diagnostics.Debug.WriteLine($"CanVerify: \nPlanModel.IsValid = {PlanModel.IsValid}\nPlanModel.IsModified = {PlanModel.IsModified}\nTreatmentHistoryModel.Treatments?.Count = {TreatmentHistoryModel.Treatments?.Count}\n");

        return TreatmentInfoStore?.Diagnosis is not null &&
               TreatmentInfoStore?.Diagnosis.Archived == false &&
               !PlanModel.IsModified &&
               !BaseEntry.IsNullOrBlankEntry(PlanModel.Plan) &&
               (PlanModel.IsValid || PlanModel.Plan.Status == PlanStatus.APPROVED);
        // We can't change the status of an approved plan with existing treatments
        //(PlanModel.Plan.Status != PlanStatus.APPROVED || (TreatmentHistoryModel.Treatments?.Count == 0));
    }

    #endregion Commands



    #region Private methods

    private async Task ChangeStatusAsync(string username, string password, PlanStatus planStatus)
    {
        try
        {
            var plan = await PlanModel.ChangeStatusAsync(username, password, planStatus);
            // As status change affects prescription and simulation, we send this event to react on it,
            // and SimulationViewModel is supposed to refetch all the data from scratch to get a consistent DB state
            EventAggregator.GetEvent<PlanStatusChangedEvent>().Publish(plan);
        }
        catch (Exception ex)
        {
            ShowDialog(StringConstants.EMR.PlanVerificationErrorTitle, StringConstants.EMR.PlanVerificationError);
            await LogWriter.LogAsync(
                $"{StringConstants.EMR.PlanVerificationError}. Old plan status: {PlanModel.Plan.Status}. Desired plan status {planStatus}. {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
        }
    }

    private void OnPlanEvent(IPlan newPlanState)
    {
        try
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _= LogWriter.LogAsync(
                    $"OnDatabasePlanChanged: Id = {newPlanState.Id}, TreatmentLoadingState = {newPlanState.TreatmentLoadingState.ToString()}, CollimatorType = {newPlanState.CollimatorType.ToString()}",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                // Plan was in treatment and now it probably gets back,
                // need to notify treatments and navigate to them.
                var currentPlan = PlanModel.Plan;
                if (currentPlan is not null && newPlanState is not null
                    && currentPlan.Id == newPlanState.Id && currentPlan.TreatmentLoadingState == TreatmentLoadingState.Loaded)
                {
                    EventAggregator.GetEvent<UnloadFromTreatmentEvent>().Publish();
                    // We also may need to update Visit to last treatment's one
                    CurrentTask = UpdatePatientVisitAsync();
                }
                PlanModel.OnDatabasePlanChanged(newPlanState);

                RaisePropertyChanged(nameof(CanVerify));
            });
        }
        catch (Exception ex)
        {
            _ = LogWriter.LogAsync($"Failed to handle DB plan status event: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
        }
    }

    private async Task UpdatePatientVisitAsync()
    {
        TreatmentInfoStore.Patient.Visit = await PatientRepository.FetchLastVisitAsync(TreatmentInfoStore.Patient.Id);
    }

    #endregion Private methods



    #region TreatmentViewModelBase
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
    }
    #endregion TreatmentViewModelBase
}
