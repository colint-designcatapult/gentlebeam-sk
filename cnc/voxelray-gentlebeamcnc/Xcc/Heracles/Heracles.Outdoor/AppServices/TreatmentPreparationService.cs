using Heracles.Application.Common;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Heracles.External.AppServices.Plan;
using Heracles.External.AppServices.System;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.Common;
using Xcc.Application.Domain.System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.External.AppServices
{
    public class TreatmentPlanItem
    {
        public ITreatmentField Planned { get; }
        public IActualTreatmentField Actual { get; }
        public CoilConfigurationInfo ExecutionParameters { get; }
        public OutputFactorInfo OutputFactor { get; }
        public TreatmentPlanItem(
            ITreatmentField planned,
            IActualTreatmentField actual, 
            CoilConfigurationInfo executionParameters,
            OutputFactorInfo outputFactorInfo)
        {
            Planned = planned;
            Actual = actual;
            ExecutionParameters = executionParameters;
            OutputFactor = outputFactorInfo;
        }

        public double Duration => Planned.DwellTime;
        public double RemainingTime => Planned.DwellTime - Actual.ActualDuration;
    }

    public class TreatmentPlan
    {
        // TODO: maybe we just need to hold TreatmentId instead of entire Treatment here
        // On the other hand, we may have a new treatment that we have yet to create & make a new visit
        public ITreatment Treatment { get; }
        public ICollimatorConfiguration CollimatorConfiguration { get; }
        public ICollection<TreatmentPlanItem> Fields { get; }
        // TODO: maybe HeaterCurrent needs to be placed somewhere else, 
        // now we just use it for EmissionPlan building, so this is the closest point for it
        public double HeaterCurrent { get; }

        public TreatmentPlan(
            ITreatment treatment,
            ICollimatorConfiguration collimatorConfiguration,
            ICollection<TreatmentPlanItem> fields,
            double heaterCurrent)
        {
            Treatment = treatment;
            CollimatorConfiguration = collimatorConfiguration;
            Fields = fields;
            HeaterCurrent = heaterCurrent;
        }

        public GcbEmissionPlan GetEmissionPlan()
        {
            var plan = new GcbEmissionPlan();
            var totalPoints = Fields.Count;
            foreach (var field in Fields)
            {
                plan.AddPoint(new GcbOperationalPoint
                {
                    PointIndex = plan.TotalPoints,
                    TotalPointTime = (float)field.Duration,
                    RemainingPointTime = (float)field.RemainingTime,
                    SetpointKv = EnergyConverter.Convert(CollimatorConfiguration.Energy),
                    TargetMA = (float)field.Planned.Current,

                    // TODO: we don't apply magnetometer now, just get calibrated coilX/Y
                    FilamentSetpoint = (float)HeaterCurrent,
                    FocusCoilSetpoint = Convert.ToSingle(field.ExecutionParameters.FocusCurrent),
                    XCoilSetpoint = Convert.ToSingle(field.ExecutionParameters.XDeflectionCurrent),
                    YCoilSetpoint = Convert.ToSingle(field.ExecutionParameters.YDeflectionCurrent),
                    AutoExecution = true
                });
            }
            return plan;
        }
    }

    public class TreatmentPreparationService
    {
        public TreatmentPreparationService(
            PlanLoadingService planLoadingService,
            IDialogService dialogService,
            CollimatorProfileService collimatorProfileService,
            ITreatmentRepository treatmentRepository,
            IPlanModel planModel,
            IActualTreatmentFieldModel actualTreatmentFieldModel,
            ITreatmentModel treatmentModel,
            IAuthorizedUserStore authorizedUserStore)
        {
            PlanLoadingService = planLoadingService;
            DialogService = dialogService;
            CollimatorProfileService = collimatorProfileService;
            TreatmentRepository = treatmentRepository;
            PlanModel = planModel;
            ActualTreatmentFieldModel = actualTreatmentFieldModel;
            TreatmentModel = treatmentModel;
            AuthorizedUserStore = authorizedUserStore;
        }

        public PlanLoadingService PlanLoadingService { get; }
        public IDialogService DialogService { get; }
        public CollimatorProfileService CollimatorProfileService { get; }
        public ITreatmentRepository TreatmentRepository { get; }
        public IPlanModel PlanModel { get; }
        public IActualTreatmentFieldModel ActualTreatmentFieldModel { get; }
        public ITreatmentModel TreatmentModel { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }

        public readonly TimeSpan MinTreatmentInterval = TimeSpan.FromHours(12);

        public async Task<TreatmentPlan> PrepareTreatmentAsync(IPlan? plan)
        {
            var treatmentInfo = await PlanLoadingService.FetchPlanDataAsync(plan);
            PlanModel.SetPlan(treatmentInfo);

            if (plan is not null)
            {
                var lastTreatment = await TreatmentRepository.FetchLatestTreatmentByPlanAsync(treatmentInfo.Plan);
                var lastTreatmentOnAnyPlan =
                    lastTreatment ?? FindLatestTreatmentForPatientAsync(treatmentInfo);
                //// If the last treatment for the same patient was within min interval,
                //// we need the user confirmation for a new treatment
                //if (lastTreatmentOnAnyPlan is not null
                //    && lastTreatmentOnAnyPlan.PerformedWithin(MinTreatmentInterval)) 
                //{

                //}

                var nextTreatment = DefineNextTreatment(treatmentInfo, lastTreatment);
                TreatmentModel.SetTreatment(nextTreatment);
                if (BaseEntry.IsNullOrBlankEntry(nextTreatment) == false) {
                    // TODO: replace with direct set or remove this model completely
                    await ActualTreatmentFieldModel.FetchCollection(nextTreatment.Id);
                }

                // Update PlanModel's treatment fields from actual treatment field's with actual duration:
                var actualFields = nextTreatment.ActualTreatmentFields.ToList();
                foreach (var treatmentField in PlanModel.TreatmentFields)
                {
                    var atf = actualFields.FirstOrDefault(a => a.Name == treatmentField.Name);
                    if (atf == null)
                        continue;

                    treatmentField.Actual = atf.ActualDuration;
                }

                // Load and verify physics data:
                var collimatorProfile = await GetMatchingProfileAsync(
                    treatmentInfo.Simulation.TargetType,
                    treatmentInfo.Prescription.Energy);
                
                // Build a plan:
                return new TreatmentPlan(
                    treatment: nextTreatment,
                    collimatorConfiguration: collimatorProfile.CollimatorConfiguration,
                    fields: plan.TreatmentFields.Select(
                        f => new TreatmentPlanItem(
                            planned: f,
                            actual: nextTreatment.GetField(f.Name),
                            executionParameters: collimatorProfile.GetCoilConfiguration(f.Name).Value,
                            outputFactorInfo: collimatorProfile.GetOutputFactor(f.Name).Value)).ToList(),
                    heaterCurrent: collimatorProfile.HeaterCurrent);
            }
            else
            {
                TreatmentModel.CloseTreatment();
                return null;
            }
        }

        private async Task<ICollimatorCalibrationInfo> GetMatchingProfileAsync(TargetType collimatorType, Energy energy)
        {
            try
            {
                var profile = await CollimatorProfileService.FindCollimatorProfileAsync(collimatorType, energy);

                var preset = profile?.CollimatorConfiguration?.DefaultPreset;

                if (preset is { IsApproved : false })
                {
                    throw new Exception("The applicator preset is not approved");
                }
                // Verify that there are all necessary coil configs and output factors:
                // TODO: we may move this validation to TreatmentPlan,
                // filling it with nulls by default and checking for consistency on demand.
                // TODO: we may probably verify for treatment's present field names only,
                // and not for an entire set of applicator's fields.
                var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(collimatorType);
                var missingCoilConfigs = fieldNameMapping.Values.Where(name => profile.GetCoilConfiguration(name) is null);
                if (missingCoilConfigs.Count() > 0)
                {
                    throw new Exception($"{missingCoilConfigs.Count()} coil configuration(s) missing in the applicator preset");
                }
                var missingOutputFactors = fieldNameMapping.Values.Where(name => profile.GetOutputFactor(name) is null);
                if (missingOutputFactors.Count() > 0)
                {
                    throw new Exception($"{missingCoilConfigs.Count()} output factor(s) missing in the applicator preset");
                }

                return profile;
            }
            catch (Exception ex)
            {
                throw new Exception("Applicator profile error", ex);
            }
        }

        /// <summary>
        /// TODO: this method should perform a search 
        /// up the treatment history tree for the patient,
        /// but it'd be better to have it on DB side.
        /// Other way may be to check for last treatment by Visit history only
        /// </summary>
        /// <param name="treatmentInfoStore"></param>
        /// <returns></returns>
        private ITreatment FindLatestTreatmentForPatientAsync(ITreatmentInfoStore treatmentInfoStore)
        {
            return null;
        }

        private ITreatment DefineNextTreatment(ITreatmentInfoStore treatmentInfo, ITreatment lastTreatment)
        {
            if (lastTreatment != null && !lastTreatment.IsComplete())
            {
                // H10TG-161: Resume Uncompleted Treatment
                // If the last treatment for the same patient was not completed,
                // we need the user confirmation for resuming it.
                if (DialogService.Confirmation(
                        StringConstants.Common.ConfirmationDialogTitle,
                        StringConstants.TreatmentConsole.PlanResumePreviousTreatmentConfirmation
                        ))
                {
                    return lastTreatment;
                }
            }
            var prevFractionNumber = lastTreatment?.Fraction ?? 0;
            var prevCumulativeDose = lastTreatment?.CumulativeDose ?? 0;
            var dailyDose = treatmentInfo.Prescription.DailyDose;
            return new Treatment
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                CreationDate = DateTime.Now,
                PlanId = treatmentInfo.Plan.Id,
                VisitId = BaseEntry.NEW_ENTRY_ID, // visit should be an optional parameter and skipped here
                LesionDepth = treatmentInfo.Simulation.LesionDepth.Value,
                DailyDose = dailyDose,
                PerformedBy = AuthorizedUserStore.AuthorizedUser.EmailAddress,
                Fraction = prevFractionNumber + 1,
                CumulativeDose = prevCumulativeDose + dailyDose
            };
        }
    }
}
