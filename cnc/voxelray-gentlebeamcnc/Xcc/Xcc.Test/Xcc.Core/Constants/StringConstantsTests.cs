using System.Globalization;
using Xcc.Core.Constants;
using Xcc.Core.ValidationRules;

namespace Xcc.Test.Xcc.Core.Constants
{
    public class StringConstantsTests
    {
        public static bool IsMultiline(string value)
        {
            return value.Contains(Environment.NewLine);
        }
        
        [Test]
        public void StringConstants_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.ConfirmExitHeader), Is.False);
            Assert.That(IsMultiline(StringConstants.ConfirmExitMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.WrongConstructorCalledErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.DetailsMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SavePreferencesErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.LoadPreferencesErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.DeserializePreferencesErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.UnsavedChangesConfirmationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemReadyUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemNotReadyUiMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_Common_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Common.ErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.ConfirmationDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.SaveErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.DeleteDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.RefreshDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.ApplyDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.SettingsDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.FieldsUiText), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.FieldUiText), Is.False);
        }
        
        [Test]
        public void StringConstants_Common_Authorization_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Common.Authorization.LoginDialogTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Authorization.NetworkErrorNoConnection), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Authorization.NetworkCredentialsError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Authorization.UserDatabaseError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Authorization.AuthorizationError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Authorization.UnknownError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Authorization.AuthenticationErrorLogMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Authorization.NoAuthorizedUserErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_Common_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Common.Validation.FieldRequiredError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Validation.DateParseError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.FutureDateError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Validation.DateOnlyParseError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Validation.NumericStartsWithDashError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NumericEndsWithDashError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NumericInvalidCharacterError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NumericTwoDashesError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Validation.StringIsNullOrEmpty), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NotANumberError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.ValueRangeRequest), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Validation.EmailParseError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameEndsWithWhitespaceError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameStartsWithDashError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameEndsWithDashError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameStartsWithApostropheError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameEndsWithApostropheError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameStartsWithPeriodError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameInvalidCharacterError), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NameInvalidError), Is.False);
        }
        
        [Test]
        public void StringConstants_Common_Validation_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.Common.Validation.NumericMinRangeFormatString), Is.True);
            Assert.That(IsMultiline(StringConstants.Common.Validation.NumericMinMaxRangeFormatString), Is.True);
        }
        
        [Test]
        public void StringConstants_Common_Detector_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Common.Detector.ErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Common.Detector.StatusCheckErrorUiMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.ClearErrorsTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.ConditioningConfirmationTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.ConditioningConfirmationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.FullWarmupEventDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.FullWarmupSaveToDbFailedError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.EmissionErrorTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.EmissionInterruptNotificationTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.EmissionStoppedNotificationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.StopTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.StopErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.WarmupEventDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.WarmupDbNoActiveHeadError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.WarmupErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.WarmupFailureError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.EmissionTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.EmissionInterruptedError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheckRequiredErrorTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.TreatmentPlanCompletionConfirmationTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.PlanRecoveryDialogTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.PlanPreparationErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.PlanPreparationErrorMessage), Is.False);

            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.FailedToClearPlan), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.FailedToCreateEmissionPlan), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.LowBatteryDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.LowBatteryWarningStringFormat), Is.False);
        }

        [Test]
        public void StringConstants_TreatmentConsole_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.ClearErrorsErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.FullWarmupFailedError), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.TelemetryLostErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.EmissionFaultErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.PlanPreparationAfterFaultErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.TreatmentPlanCompletionConfirmationMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.PlanRecoveryUpdateFromBoardConfirmation), Is.True);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_Treatment_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.TreatmentPlanNotDefined), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.ApplicatorErrorDialogTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.EmissionRecordingDbErrorUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.LookForIncomingPlanErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.FailedClearPlanErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.FailedAckPlanLoading), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.FailedToRecoverPlanFromBoard), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.FailedToSaveTreatment), Is.False);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_Treatment_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.ApplicatorInterlockError), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.LowConsoleBatteryError), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.IgnoreMissingQcConfirmation), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.QcTestFailedErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.FailedQcErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.MissingQcErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.MissingQcReferenceErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Treatment.TreatmentRetryUiMessage), Is.True);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_SafetyCheck_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.ErrorTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.CompletionConfirmationTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.HistoryListLoadError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.CreatePlanErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_SafetyCheck_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.StartErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.CompletionConfirmationMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.SafetyCheck.SaveDataErrorMessage), Is.True);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_QualityCheck_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.CustomModeConfirmationMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.FullModeConfirmationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.NotificationTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.CompletionNotification), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.DeleteFieldConfirmationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.DiscardChangesConfirmationTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.DiscardChangesConfirmationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.CreateCollectionError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.FieldOperationErrorTitle), Is.False);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_QualityCheck_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.AddFieldErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.RemoveFieldErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.QualityCheck.BoardConnectionCheckFailed), Is.True);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_Imaging_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.CompletionConfirmationTitle), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.NoActiveHeadErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.ActiveHeadImagingFieldMismatchErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.ImagingPlanNotDefined), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.FailedAckImagingPlan), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.FailedCheckPendingImagingPlan), Is.False);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_Imaging_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.CompletionConfirmationMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.Imaging.IgnoreMissingQcConfirmation), Is.True);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_DetectorCalibration_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.CalibrationTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.ApplyDataQuestion), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.FailedToApplyMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.FailedToStartMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.FailedToPreparePlanMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.FailedToAcquireCalibrationDataMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.GainCompleteMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.OffsetCompleteMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.ProgressStringFormat), Is.False);
        }
        
        [Test]
        public void StringConstants_TreatmentConsole_DetectorCalibration_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.Validation.DurationIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.TreatmentConsole.DetectorCalibration.Validation.DurationIsNotSetErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_EMR_Calibration_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Physics.ApproveDialogTitle), Is.False);
        }
        
        [Test]
        public void StringConstants_EMR_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.PlanUnloadFromConsoleError), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.PlanUnloadFromConsoleErrorLogMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_EMR_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.PatientIsNotSelectedErrorMessage), Is.True);
        }
        
        [Test]
        public void StringConstants_EMR_Plan_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Plan.FetchErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.FetchUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.SaveErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.SaveUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.LoadForTreatmentErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.LoadForTreatmentUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.PlanIsNotSetErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.HandleSelectionEventErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.PlanStreamErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.PlanEventReceivedMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.TargetDoesNotMatchUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.TotalDurationExceedLimitUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.UnsavedPlanChangesUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.SaveTreatmentPlanChangesUiMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.DeleteTreatmentFieldsDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.DeleteTreatmentFieldsConfirmationUiMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.ValidationErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.ValidationUiErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.NoMatchingHeadUiErrorMessage), Is.False);

            Assert.That(IsMultiline(StringConstants.EMR.Plan.ResumeTreatmentDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.TreatmentWasNotCompletedUiMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Plan.ApprovePlanDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.LookForLoadedImagingPlanErrorMessage), Is.False);
        }

        [Test]
        public void StringConstants_EMR_Plan_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Plan.UpdateTreatmentFieldErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.EMR.Plan.HandlePlanEventErrorMessage), Is.True);
        }

        [Test]
        public void StringConstants_EMR_Plan_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Plan.Validation.DwellTimeMustBeNonZero), Is.False);
        }

        [Test]
        public void StringConstants_EMR_Images_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Images.FetchImagesErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Images.FetchImagesUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Images.DicomFileTransferTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Images.InvalidDicomFileCrc), Is.False);
        }

        [Test]
        public void StringConstants_EMR_PatientImages_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.PatientImages.LoadDICOMErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.PatientImages.LoadDICOMUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.PatientImages.DICOMFileNotSpecifiedErrorMessage), Is.False);
        }

        [Test]
        public void StringConstants_EMR_PatientProfile_SingleLine()
        {
        }

        [Test]
        public void StringConstants_EMR_PatientProfile_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.PatientProfile.SaveErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.EMR.PatientProfile.SaveUiErrorMessage), Is.True);
        }

        [Test]
        public void StringConstants_EMR_Patients_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Patients.FetchPatientsErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.FetchPatientsUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.SavePatientErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.SavePatientUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.CreatePatientErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.CreatePatientUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.PatientExistDialogTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.PatientExistsUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.SaveNewPatientAuditLogMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.SaveExistingPatientAuditLogMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.SavePatientIsDoneAuditLogMessage), Is.False);
        }

        [Test]
        public void StringConstants_EMR_Patients_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.FirstNameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.FirstNameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.LastNameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.LastNameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.MrnIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.MrnIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.SexIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.SexIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.DateOfBirthIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Patients.Validation.DateOfBirthIsNotSetErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_EMR_Prescription_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.FetchPrescriptionError), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.FetchPrescriptionUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.SavePrescriptionErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.SavePrescriptionUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.CreatePrescriptionErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.CreatePrescriptionUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.UnsavedChangedUiMessage), Is.False);
        }

        [Test]
        public void StringConstants_EMR_Prescription_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.TreatmentTypeIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.TreatmentTypeIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.SiteNameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.SiteNameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.VolumeIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.VolumeIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.DosePerFractionIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.DosePerFractionIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.FractionIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.FractionFormatErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.Prescription.Validation.FractionCompareErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_EMR_ImagingView_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.FetchProtocolsErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.FetchProtocolsUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.SaveProtocolErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.SaveProtocolUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.CreateProtocolErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.CreateProtocolUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.DeleteProtocolErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.DeleteProtocolUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.LoadForImagingErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.LoadForImagingUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.ProtocolIsNotSelectedErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.ActiveHeadIsNotSetErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.ProtocolNameIsNotSpecifiedError), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.CaptureDetectorTagsErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.CaptureDetectorTagsUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.RobotRepositionDialogHeader), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.InvalidRobotPosition), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.RobotMovementFailed), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.FailedToGetRobotPosition), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.FinalRepositionCalculationFailed), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.RobotEnableHandguidingErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.RobotEnableHandguidingUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.ResultIsNotEmptyErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.FailedToRepositionErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.EMR.ImagingView.RepositionMatrixIsNullError), Is.False);
        }

        [Test]
        public void StringConstants_Configuration_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Configuration.ActiveHeadNotFound), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Configuration.ActiveHeadFetchErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Configuration.ActiveHeadFetchErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Configuration.ActivePresetFetchErrorTitle), Is.False);
            Assert.That(IsMultiline(StringConstants.Configuration.ActivePresetFetchErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Configuration.NoPresetForEnergy), Is.False);
        }
        
        [Test]
        public void StringConstants_Calibration_SingleLine()
        {
            Assert.Multiple(() =>
            {
                Assert.That(IsMultiline(StringConstants.Physics.PhysicsDataDialogTitle), Is.False);

                Assert.That(IsMultiline(StringConstants.Physics.ConfigurationFileLoadErrorTitle), Is.False);
                Assert.That(IsMultiline(StringConstants.Physics.CsvImportErrorTitle), Is.False);
                Assert.That(IsMultiline(StringConstants.Physics.CsvImportDataMissingError), Is.False);
                Assert.That(IsMultiline(StringConstants.Physics.CsvFileFormatError), Is.False);
                Assert.That(IsMultiline(StringConstants.Physics.CsvFileReadError), Is.False);

                Assert.That(IsMultiline(StringConstants.Physics.OutputFactorDialogTitle), Is.False);
                Assert.That(IsMultiline(StringConstants.Physics.OutputFactorResetWarning), Is.False);
                Assert.That(IsMultiline(StringConstants.Physics.OutputFactorSubmitError), Is.False);

                Assert.That(IsMultiline(StringConstants.Physics.StoreCsvDialogTitle), Is.False);

                Assert.That(IsMultiline(StringConstants.Physics.PhysicsDataIsModified), Is.False);
            });
        }
        
        [Test]
        public void StringConstants_Calibration_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.Physics.LeaveConfigurationTabConfirmationMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.CoilConfigurationSaveErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.TargetPointsSaveErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.OutputFactorsSaveErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.PhysicsDataIsInvalid), Is.True);

            Assert.That(IsMultiline(StringConstants.Physics.HeaterCurrentSaveErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.MagnetometerConfigurationSaveErrorMessage), Is.True);


            Assert.That(IsMultiline(StringConstants.Physics.FailedToStoreCsvErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.SavedDataRequiredApprovalMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.CsvImportMissingSourceError), Is.True);
            Assert.That(IsMultiline(StringConstants.Physics.CsvWrongSourceTypeError), Is.True);

        }

        [Test]
        public void StringConstants_Calibration_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Physics.Validation.XCoilCurrentRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.Physics.Validation.XCoilCurrentIsZero), Is.False);
            Assert.That(IsMultiline(StringConstants.Physics.Validation.XCoilCurrentIsNotSet), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Physics.Validation.YCoilCurrentRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.Physics.Validation.YCoilCurrentIsZero), Is.False);
            Assert.That(IsMultiline(StringConstants.Physics.Validation.YCoilCurrentIsNotSet), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Physics.Validation.FocusCurrentRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.Physics.Validation.FocusCurrentIsZero), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Physics.Validation.HeaterCurrentRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.Physics.Validation.HeaterCurrentIsNotSet), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Physics.Validation.CorrectionMatrixValueIsNotSet), Is.False);
            
            Assert.That(IsMultiline(StringConstants.Physics.Validation.ReferenceFieldValueIsNotSet), Is.True);
        }

        [Test]
        public void StringConstants_SystemSettings_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.Common.RestartExternalOnSaveNotification), Is.False);
        }

        [Test]
        public void StringConstants_SystemSettings_Network_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.Network.FetchErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.Network.FetchUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.Network.SaveErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.Network.SaveUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.Network.SettingsIsNotSetErrorMessage), Is.False);
        }

        [Test]
        public void StringConstants_SystemSettings_ImagingProtocols_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.FetchErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.FetchUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.SaveErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.SaveUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.CreateErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.CreateUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.DeleteErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.DeleteUiErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.DeleteConfirmationMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.ProtocolIsNotSelectedErrorMessage), Is.False);
        }

        [Test]
        public void StringConstants_SystemSettings_ImagingProtocols_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.ProtocolNameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.ProtocolNameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.DoseIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.DoseIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.SourceToImageDistanceIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.SourceToImageDistanceIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.FieldOfViewIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.FieldOfViewIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.RadiusIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.RadiusIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.NumberOfProjectionsIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.NumberOfProjectionsIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.TimeIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.TimeIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.AngleIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.AngleIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.SliceThicknessIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.SliceThicknessIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.EntranceDoseIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.EntranceDoseIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.SkinDoseIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.ImagingProtocols.Validation.SkinDoseIsNotSetErrorMessage), Is.False);
        }

        [Test]
        public void StringConstants_SystemSettings_UserManagement_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.DeleteUserConfirmationUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.DeleteAuthorizedUserUiMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.UserIsNotSelectedErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_SystemSettings_UserManagement_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.FetchUsersErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.FetchUsersUiErrorMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.SaveUserErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.SaveUserUiErrorMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.CreateUserErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.CreateUserUiErrorMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.DeleteUserErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.DeleteUserUiErrorMessage), Is.True);
        }

        [Test]
        public void StringConstants_SystemSettings_UserManagement_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.FirstNameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.FirstNameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.LastNameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.LastNameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.UsernameIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.UsernameIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.PasswordIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.PasswordIsNotSetErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.PasswordConfirmIsRequired), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.EmailIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserManagement.Validation.EmailIsNotSetErrorMessage), Is.False);
        }

        [Test]
        public void StringConstants_SystemSettings_UserRoles_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.DeleteRoleConfirmationUiMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.DeleteAuthorizedUserRoleUiMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.UserRoleIsNotSelectedErrorMessage), Is.False);
        }
        
        [Test]
        public void StringConstants_SystemSettings_UserRoles_MultiLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.FetchUserRolesErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.FetchUserRolesUiErrorMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.SaveUserRoleErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.SaveUserRoleUiErrorMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.CreateUserRoleErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.CreateUserRoleUiErrorMessage), Is.True);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.DeleteUserRoleErrorMessage), Is.True);
            Assert.That(IsMultiline(StringConstants.SystemSettings.UserRoles.DeleteUserRoleUiErrorMessage), Is.True);
        }

        [Test]
        public void StringConstants_SystemSettings_HeadManagement_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.FetchHeadsErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.FetchHeadsUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.SaveHeadErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.SaveHeadUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.CreateHeadErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.CreateHeadUiErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.HeadIsNotSelectedErrorMessage), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.ConfirmActiveHeadSwitchUiMessage), Is.False);
        }

        [Test]
        public void StringConstants_SystemSettings_HeadManagement_Validation_SingleLine()
        {
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.Validation.SerialIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.Validation.TypeIsRequired), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.Validation.ImagingFieldRequired), Is.False);
            
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.Validation.TypeIsNotSetErrorMessage), Is.False);
            Assert.That(IsMultiline(StringConstants.SystemSettings.HeadManagement.Validation.ImagingFieldIsNotSetErrorMessage), Is.False);
        }

        //[Test]
        //public void StringConstants_Robot_SingleLine()
        //{
        //    Assert.That(IsMultiline(StringConstants.Robot.MotionModeStringFormat), Is.False);
        //    Assert.That(IsMultiline(StringConstants.Robot.NoConnection), Is.False);
        //    Assert.That(IsMultiline(StringConstants.Robot.MinutesUntilBrakeTest), Is.False);
        //}
    }
}