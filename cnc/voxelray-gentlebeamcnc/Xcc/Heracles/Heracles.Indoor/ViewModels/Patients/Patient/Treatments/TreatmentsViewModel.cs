using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Common;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Models;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Constants;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models.UseCases;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Xcc.Shared.Services;

using XccStringConstants = Xcc.Core.Constants.StringConstants;

namespace Heracles.Indoor.ViewModels.Patients.Patient.Treatments
{
    public class TreatmentsViewModel : TreatmentViewModelBase
    {
        #region Constructors
        public TreatmentsViewModel(
            IRegionManager regionManager,
            IDisruptiveActionGuardService disruptiveActionGuardService,
            IEventAggregator eventAggregator,
            IAuthorizedUserStore authorizedUserStore,
            ICollimatorModel collimatorModel,
            IDialogService dialogService,
            IPopUpService popUpService,
            ILogRepository logWriter,
            IPlanLoading planLoading,
            IPlanModel planModel,
            ITreatmentHistoryModel treatmentHistoryModel,
            ITreatmentInfoStore treatmentInfoStore,
            IPlanRepository planRepository): 
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
            //Property assignments
            AuthorizedUserStore = authorizedUserStore;
            PopUpService = popUpService;
            PlanLoading = planLoading;
            TreatmentHistoryModel = treatmentHistoryModel;
            PlanRepository = planRepository;

            // Event subscriptions
            AuthorizedUserStore.AuthorizedUserChanged += (_, _) => RaisePropertyChanged(nameof(CanLoadForTreatment));

            PlanModel.IsModifiedChanged += (_, _) => RaisePropertyChanged(nameof(CanLoadForTreatment));

            PlanModel.IsValidChanged += (_, _) => RaisePropertyChanged(nameof(CanLoadForTreatment));

            TreatmentInfoStore.PlanChanged += (_, _) =>
            {
                // We need to load the history only when plan was set/reset:
                // TODO: we need to query only if it is really needed (like unloading from treatment),
                // and not on any event of plan update
                FetchTreatmentHistory(); 
                
                RaisePropertyChanged(nameof(CanLoadForTreatment));
            };

            CollimatorModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CollimatorModel.ActiveCollimator))
                {
                    RaisePropertyChanged(nameof(CanLoadForTreatment));
                }
            };

            //TreatmentInfoStore.DiagnosisChanged += (_, _) =>
            //{
            //    FetchTreatmentHistory();
            //    RaisePropertyChanged(nameof(CanLoadForTreatment));
            //};
        }

        #endregion Constructors



        #region Injected Dependencies
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public IPopUpService PopUpService { get; }
        public IPlanLoading PlanLoading { get; }
        public ITreatmentHistoryModel TreatmentHistoryModel { get; }
        public IPlanRepository PlanRepository { get; }

        #endregion Injected Dependencies



        #region Properties
        private Task? CurrentTask { get; set; }
        #endregion Properties



        #region Commands
        private DelegateCommand? _clearTreatmentSelectionCommand;
        public DelegateCommand ClearTreatmentSelectionCommand => _clearTreatmentSelectionCommand ??= new DelegateCommand(
            () =>
            {
                TreatmentHistoryModel.SelectedTreatment = null;
            });
        

        private DelegateCommand<object>? _showActualFieldsCommand;
        public DelegateCommand<object> ShowActualFieldsCommand => _showActualFieldsCommand ??= new DelegateCommand<object>(
           item =>
           {
               TreatmentHistoryModel.SelectedTreatment = item as ITreatmentBindable;
           });
        #endregion Commands



        #region Private methods
        private void FetchTreatmentHistory()
        {
            IPlan prevPlan = TreatmentHistoryModel.Plan;
            IPlan newPlan = TreatmentInfoStore.Plan;
            if (!BaseEntry.IsNullOrBlankEntry(newPlan)) 
            {
                bool isSamePlan = prevPlan is not null && newPlan is not null && prevPlan.Id == newPlan.Id;
                CurrentTask = FetchTreatmentsDataAsync(isSamePlan);
            }
            else
            {
                CurrentTask = SetHistoryContextAsync();
            }
        }

        private async Task SetHistoryContextAsync()
        {
            // get current values to prevent races 
            var diagnosis = TreatmentInfoStore.Diagnosis;
            var prescription = TreatmentInfoStore.Prescription;
            var plan = TreatmentInfoStore.Plan;

            var prevTask = CurrentTask;

            if (prevTask != null && prevTask.IsCompleted == false)
            {
                try
                {
                    await prevTask;
                }
                catch (Exception)
                {
                    // We ignore previous task crash
                }
            }

            // null or blank plan, nothing to fetch, just update the context
            TreatmentHistoryModel.SetContext(diagnosis, prescription, plan);
        }

        private async Task FetchTreatmentsDataAsync(bool isSamePlan)
        {
            try
            {
                if (isSamePlan)
                {
                    await TreatmentHistoryModel.UpdateTreatmentsAsync();
                }
                else
                {
                    TreatmentHistoryModel.SetContext(TreatmentInfoStore.Diagnosis, PlanModel.Prescription, PlanModel.Plan);
                    await TreatmentHistoryModel.FetchTreatmentsAsync();
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log($"{nameof(TreatmentsViewModel)}.{nameof(FetchTreatmentsDataAsync)}: Exception {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
        }
        #endregion Private methods



        #region 'Load For Treatment' members
        private DelegateCommand? _loadForTreatmentCommand;
        public DelegateCommand LoadForTreatmentCommand => _loadForTreatmentCommand ??= new DelegateCommand(
            async () =>
            {
                await LoadForTreatment();
            }
        ).ObservesCanExecute(() => CanLoadForTreatment);


        private bool CanLoadForTreatment
        {
            get
            {
                var activeConfiguration = CollimatorModel.ActiveCollimator?.Configuration;

                bool planIsValid =
                    TreatmentInfoStore.Plan is not null &&
                    activeConfiguration is not null &&
                    TreatmentInfoStore.Plan.Status.Equals(PlanStatus.APPROVED) &&
                    TreatmentInfoStore.Plan.TreatmentLoadingState.Equals(TreatmentLoadingState.Unloaded) &&
                    TreatmentInfoStore.Plan.CollimatorType == activeConfiguration.Type &&
                    !PlanModel.IsModified;

                bool diagnosisIsActive =
                    TreatmentInfoStore.Diagnosis is not null &&
                    TreatmentInfoStore.Diagnosis.Archived == false;

                bool userHasPermission =
                    AuthorizedUserStore.AuthorizedUser is not null &&
                    AuthorizedUserStore.AuthorizedUser.Role.Permissions.Treatment;

                return planIsValid && diagnosisIsActive && userHasPermission;
            }
        }

        private async Task LoadForTreatment()
        {
            if (!PlanModel.IsValid)
            {
                DialogService?.ReportError(
                    StringConstants.EMR.PlanLoadErrorDialogTitle,
                    StringConstants.EMR.PlanValidationErrorMessage);
                return;
            }
            else if(!ApplicatorCompatibilityStatus.IsCompatible)
            {
                ReportTargetMismatchError();
            }
            else
            {
                try
                {
                    var preset = CollimatorModel.ActiveCollimator?.Configuration?.DefaultPreset;
                    if (preset?.IsApproved != true)
                    {
                        DialogService?.ReportError(
                            StringConstants.EMR.PlanNoApprovedPresetErrorTitle,
                            StringConstants.EMR.PlanNoApprovedPresetErrorMessage);
                        return;
                    }

                    // Check if plan has any fields with dwell time of 300+ seconds,
                    // it is permitted now by the hardware to load such high values
                    // TODO: duplication with ApprovePlan
                    if (PlanModel.TreatmentFields.Any(tf => tf.DwellTime >= ClinicalDataConstants.DwellTimeLimit))
                    {
                        ShowDialog(
                            StringConstants.EMR.PlanDwellTimeLimitExceededErrorTitle,
                            StringConstants.EMR.PlanDwellTimeLimitExceededErrorMessage);
                        return;
                    }

                    // Check if we have any treatment fractions left:
                    int existingFractions = TreatmentHistoryModel.Treatments?.Count ?? 0;
                    if (existingFractions >= PlanModel.Prescription.NumberOfFxs)
                    {
                        ShowDialog(
                            StringConstants.EMR.PlanLoadNoFractionsLeftErrorTitle,
                            StringConstants.EMR.PlanLoadNoFractionsLeftErrorMessage
                            );
                        return;
                    }

                    // Check for existing pending plan:
                    var pendingPlan = await PlanRepository.FindPendingPlanAsync();
                    if (pendingPlan != null)
                    {
                        ShowDialog(
                            StringConstants.EMR.PlanLoadForTreatmentDialogTitle,
                            $"{StringConstants.EMR.PendingPlanAlreadyExistsInfo}: id={pendingPlan.Id}",
                            ReportType.Info);
                        return;
                    }

                    // Check if the latest fraction was done in less than 12 hours
                    if (existingFractions > 0 && TreatmentHistoryModel.Treatments.Last().CreationDate.AddHours(12) > DateTime.Now)
                    {
                        if (!DialogService.Confirmation(
                            StringConstants.EMR.PlanTreatmentConfirmationTitle,
                            StringConstants.EMR.PlanTreatmentInLessThan12hrsConfirmation))
                        {
                            return;
                        }
                    }

                    if (TreatmentHistoryModel?.Treatments?.LastOrDefault()?.IsComplete == false)
                    {
                        var result = PopUpService.YesNoCancelDialog(
                            XccStringConstants.EMR.Plan.ResumeTreatmentDialogTitle,
                            XccStringConstants.EMR.Plan.TreatmentWasNotCompletedUiMessage);

                        if (result is DialogBoxResult.Yes)
                        {
                            await PlanLoading.LoadForTreatmentAsync(PlanModel.Plan.Id, isPartial: true);
                        }
                        else if (result is DialogBoxResult.No)
                        {
                            await PlanLoading.LoadForTreatmentAsync(PlanModel.Plan.Id, isPartial: false);
                        }
                    }
                    else
                        await PlanLoading.LoadForTreatmentAsync(PlanModel.Plan.Id, isPartial: false);


                    //await PlanModel.FetchLatestPlanAsync(); // todo: temp, while unable to receive plan events
                }
                catch (Exception ex)
                {
                    await ShowAndLogErrorAsync(
                        StringConstants.Common.ErrorTitle,
                        StringConstants.EMR.PlanLoadForTreatmentError,
                        ex);
                }
            }
        }

        private void ReportTargetMismatchError()
        {
            TargetType? planTargetType = TreatmentInfoStore.Plan?.CollimatorType;
            Energy? planEnergy = PlanModel.Prescription?.Energy;

            var planTargetTypeName = planTargetType?.GetAttribute<DisplayAttribute>()?.Name ?? "None";
            var planEnergyName = planEnergy?.GetAttribute<DisplayAttribute>()?.Name ?? "?";

            if (planTargetType is null || planEnergy is null)
            {
                DialogService.ReportError(
                    StringConstants.EMR.PlanLoadErrorDialogTitle,
                    string.Format(StringConstants.ClinicalData.ApplicatorRequirementsUiErrorMessage,
                        Environment.NewLine,
                        planTargetTypeName,
                        planEnergyName));

            }
            else if (BaseEntry.IsNullOrBlankEntry(CollimatorModel.ActiveCollimator))
            {
                DialogService.ReportError(
                    StringConstants.EMR.PlanLoadErrorDialogTitle,
                    StringConstants.EMR.PlanNoActiveApplicatorError);
            }
            else
            {
                DialogService.ReportError(
                    StringConstants.EMR.PlanLoadErrorDialogTitle,
                    string.Format(StringConstants.ClinicalData.ApplicatorMismatchUiMessage,
                        CollimatorModel.ActiveCollimator.Serial, 
                        planTargetTypeName, 
                        planEnergyName));
            }
        }
        #endregion 'Load For Treatment' members
    }
}
