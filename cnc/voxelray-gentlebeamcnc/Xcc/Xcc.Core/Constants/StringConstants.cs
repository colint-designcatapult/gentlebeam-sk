using System;

namespace Xcc.Core.Constants
{
    public static class StringConstants
    {
        public const string ConfirmExitHeader = "Exit Confirmation";
        public const string ConfirmExitMessage = "Do you want to exit and close the application?";
        public const string WrongConstructorCalledErrorMessage = "This constructor can only be used in design mode.";
        public const string DetailsMessage = "See details in the log.";

        public const string SavePreferencesErrorMessage = "Failed to save user preferences.";
        
        public const string LoadPreferencesErrorMessage = "Failed to load user preferences.";
        public const string DeserializePreferencesErrorMessage = "Failed to deserialize userpreferences file.";
        public const string UnsavedChangesConfirmationMessage = "You will lose unsaved changes. Continue?";

        public const string SystemReadyUiMessage = "System ready";
        public const string SystemNotReadyUiMessage = "System not ready";

        public static class Common
        {
            public const string ErrorTitle = "Error";
            public const string ConfirmationDialogTitle = "Confirmation";
            public const string SaveErrorTitle = "Save Error";
            public const string DeleteDialogTitle = "Delete";
            public const string RefreshDialogTitle = "Refresh";
            public const string ApplyDialogTitle = "Apply";
            public const string SettingsDialogTitle = "Settings";
            public const string QaDialogTitle = "Quality Assurance";
            public const string FieldsUiText = "Fields";
            public const string FieldUiText = "Field";
            public const string SystemInfoTitle = "System Info";
            public const string SystemInfoFailed = "Failed to get system info.";
            public const string DatabaseConnectionErrorTitle = "Database connection error";

            public const string NoDatabaseConnectionErrorMessage = "No connection to the database.";
            public static string StartupPlanLookupUiError = $"Failed to retrieve pending/loaded plan data.{Environment.NewLine}Please check the connection and try again.";
            public const string RestartExternalOnSaveNotification = "Please restart the software on the treatment console to apply the changes.";
            
            public static class Authorization
            {
                public const string LoginDialogTitle = "Log In";

                public const string NetworkErrorNoConnection = "Failed to connect to the database.";
                public const string NetworkCredentialsError = "Invalid username or password.";
                public static readonly string DbInternalError = $"Database service internal error.{Environment.NewLine}{DetailsMessage}";

                public const string UserDatabaseError = "Database error.";
                public const string AuthorizationError = "Authorization error.";
                public static readonly string SessionExpirationError = $"Session expired.{Environment.NewLine}Please log in to continue.";

                public static readonly string SessionAuthExpirationError = $"Session authorization expired.{Environment.NewLine}Please renew the session by logging in again.";

                public const string UnknownError = "Something went wrong.";

                public const string AuthenticationErrorLogMessage = "Failed to authenticate.";
                public const string NoAuthorizedUserErrorMessage = "No authorized user.";
            }

            /// <summary>
            /// Deprecated. Use from Empyrean.Common.Core.Constants
            /// </summary>
            [Obsolete]
            public static class Validation 
            {
                public const string FieldRequiredError = "Field is required.";

                // DateOfBirth
                public const string DateParseError = "Please enter a valid date.";
                public const string FutureDateError = "Date can't be later than today.";

                // DateOnly
                public const string DateOnlyParseError = "Please enter a valid date.";

                // Numeric, IntRange, DoubleRange, FloatRange
                public const string NumericStartsWithDashError = "Entry cannot start with a dash.";
                public const string NumericEndsWithDashError = "Entry cannot end with a dash.";
                public const string NumericInvalidCharacterError = "Only digits and dashes are allowed.";
                public const string NumericTwoDashesError = "Two dashes in a row are not allowed.";
                public static readonly string NumericMinRangeFormatString = $"{{0}} must be{Environment.NewLine}at least {{1}}.";
                public static readonly string NumericMinMaxRangeFormatString = $"The field {{0}} must be{Environment.NewLine}between {{1}} and {{2}}.";

                public const string StringIsNullOrEmpty = "Please start to type something.";
                public const string NotANumberError = "Please enter a number.";
                public const string ValueRangeRequest = "Please enter a number in the range."; // {Min}-{Max}
                
                // Email Rule
                public const string EmailParseError = "Please enter a valid email address.";

                // Name Rule
                public const string NameEndsWithWhitespaceError = "Name cannot end with a space.";
                public const string NameStartsWithDashError = "Name cannot start with a dash.";
                public const string NameEndsWithDashError = "Name cannot end with a dash.";
                public const string NameStartsWithApostropheError = "Name cannot start with an apostrophe.";
                public const string NameEndsWithApostropheError = "Name cannot end with an apostrophe.";
                public const string NameStartsWithPeriodError = "Name cannot start with a period.";
                public const string NameInvalidCharacterError = "Name contains invalid characters.";
                public const string NameInvalidError = "Please enter a valid name."; //Strings containing any "-" " " "'" twice in a row counts as non-valid
		        public const string NameExistsAlready = "Name already exists.";

                // File Path Security
                public const string FilePathErrorTitle = "File Path Error";
                public static readonly string FilePathInvalidContentError = $"File path contains forbidden characters.{Environment.NewLine}Please rename the file or use another file location";
                public const string FilePathNotExistError = "Specified file does not exist";
                public const string FilePathNotOnDiskError = "File must be located on a disk volume";
            }

            public static class Detector
            {
                public const string ErrorTitle = "Detector Error";
                public const string StatusCheckErrorUiMessage = "Detector is not available. Reboot the Detector Panel and restart the Detector Service.";
            }

            /// <summary>
            /// These are just generic parts of other messages:
            /// </summary>
            public static class Generic
            {
                public const string DbFailedPrefix = "Failed to connect to the database.";
                public static readonly string RetryOrContactSupportMessageFooter = $"Please retry the operation and restart the system if needed.{Environment.NewLine}If the problem persists, contact Support.";
                public static readonly string RestartOrContactSupportMessageFooter = $"Please restart the system.{Environment.NewLine}If the problem persists, contact Support.";
            }
        }

        public static class TreatmentConsole
        {
            public const string ResetTimersTitle = "Reset Timers";
            public static readonly string ResetTimersErrorMessage = $"Failed to reset the timers.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public const string ClearErrorsTitle = "Clear Errors";
            public static readonly string ClearErrorsErrorMessage = $"Failed to clear the faults.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public const string StopTitle = "Stop";
            public const string StopErrorMessage = "Stop failed. Press the E-stop button immediately and contact Support.";

            public const string ResumeErrorTitle = "Resume Error";
            public static readonly string ResumeErrorMessage = $"Failed to resume the plan execution.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public const string ClearPlanTitle = "Clear Plan";
            public static readonly string ClearPlanErrorMessage = $"Failed to clear the plan.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public const string ConditioningConfirmationTitle = "Conditioning Confirmation";
            public const string ConditioningConfirmationMessage = "This operation should be performed once a week. Do you want to proceed?";

            public const string FullWarmupEventDialogTitle = "Full Warmup";
            public const string FullWarmupSaveToDbFailedError = "Failed to save full warmup result."; // with FullWarmupEventDialogTitle
            public static readonly string FullWarmupFailedError = $"Failed to perform a full warmup.{Environment.NewLine}Please try the warmup operation again.";  // with FullWarmupEventDialogTitle

            public const string EmissionErrorTitle = "Emission Error";
            public static readonly string TelemetryLostErrorMessage = $"Connection to the treatment equipment lost.{Environment.NewLine}{Common.Generic.RestartOrContactSupportMessageFooter}";
            public static readonly string EmissionFaultErrorMessage = $"Hardware fault occurred.{Environment.NewLine}Please check the faults view and try clearing the error.";

            public const string EmissionInterruptNotificationTitle = "Emission Interrupted";
            public const string EmissionStoppedNotificationMessage = "Emission was stopped.";

            public const string WarmupEventDialogTitle = "Warmup";
            public const string WarmupDbNoActiveHeadError = "Cannot save warmup result to the database. The active head could not be determined."; // with WarmupEventDialogTitle and FullWarmupEventDialogTitle

            public const string WarmupErrorTitle = "Warmup Error";
            public const string WarmupFailureError = "Failed to warmup the system.";

            public const string EmissionTitle = "Emission";
            public const string EmissionInterruptedError = "Emission was interrupted.";

            public const string QualityCheckRequiredErrorTitle = "Quality Check Error";

            public const string TreatmentPlanCompletionConfirmationTitle = "Treatment Delivery Completed";
            public static readonly string TreatmentPlanCompletionConfirmationMessage = $"All operational points were delivered successfully.{Environment.NewLine}The timers will be reset and the plan will be unloaded.";
            public static readonly string TreatmentPlanImperfectCompletionConfirmationMessage = $"Treatment plan execution is finished.{Environment.NewLine}Note that some operational points aren't completed precisely.{Environment.NewLine}The timers will be reset, and the plan will be unloaded.";

            public const string PlanRecoveryDialogTitle = "Plan Recovery";
            public static readonly string PlanRecoveryUpdateFromBoardConfirmation = $"There is matching plan data on the treatment equipment.{Environment.NewLine}Do you want to update the emission data from there?";

            public const string PlanPreparationErrorTitle = "Prepare Error";
            public const string PlanPreparationErrorMessage = "Failed to prepare the system."; // with PlanPreparationEventDialogTitle
            public static readonly string PlanPreparationForQcBoardPingErrorMessage = $"QC board error. {Environment.NewLine}Please check the network settings and connection";
            public const string PlanPreparationForQcErrorMessage = "Failed to prepare the system for QC.";
            public static readonly string PlanPreparationAfterFaultErrorMessage = $"Failed to resume plan preparation after fault.{Environment.NewLine}{Common.Generic.RestartOrContactSupportMessageFooter}";

            public const string FailedToClearPlan = "Failed to clear the plan.";
            public const string FailedToCreateEmissionPlan = "Failed to create the emission plan.";
            public const string LowBatteryDialogTitle = "Low Battery";
            public const string LowBatteryWarningStringFormat = "Battery charge is low: {0:F0} %";
            public const string FailedToGetGcbVersion = "Failed to get main control board version";

            public static class Treatment
            {
                public const string TreatmentPlanNotDefined = "A treatment plan is not defined.";
                public const string ApplicatorErrorDialogTitle = "Applicator Error";
                public static string ApplicatorInterlockError = $"The applicator is not fully inserted.{Environment.NewLine}Treatment cannot be started.{Environment.NewLine}Please ensure that the applicator is fully inserted and try again.";
                public static string LowConsoleBatteryError = $"The battery charge is too low.{Environment.NewLine}Treatment cannot be started.{Environment.NewLine}Please charge the battery and try again.";

                public static readonly string IgnoreMissingQcConfirmation = $"No successful quality checks found within 24 hours.{Environment.NewLine}Please perform a quality check.{Environment.NewLine}Do you want to proceed with the treatment anyway?";
                public static readonly string QcTestFailedErrorMessage = $"No successful quality checks found within 24 hours.{Environment.NewLine}Please perform a quality check.";

                public static readonly string FailedQcErrorMessage = $"Last quality check wasn't successful.{Environment.NewLine}Please verify it or perform a new quality check.";
                public static readonly string MissingQcErrorMessage = $"No quality check records found within 24 hours.{Environment.NewLine}Please perform a quality check.";
                public static readonly string MissingQcReferenceErrorMessage = $"Referenced quality check record is missing.{Environment.NewLine}Please specify a referenced quality check.";

                public const string EmissionRecordingDbErrorUiMessage = "Failed to save the emission data to the database.";
                public const string LookForIncomingPlanErrorMessage = "Failed to look for an incoming plan.";
                public const string FailedClearPlanErrorMessage = "Failed to clear the plan.";
                public const string FailedAckPlanLoading = "Failed to confirm the treatment plan loading.";
                public const string FailedToRecoverPlanFromBoard = "Failed to recover the plan from the board.";
                public const string FailedToSaveTreatment = "Failed to save the treatment.";
                public static readonly string TreatmentRetryUiMessage = $"{FailedToSaveTreatment}{Environment.NewLine}Do you want to try again?";
            }

            public static class SafetyCheck
            {
                public const string ErrorTitle = "Safety Check Error";
                public static readonly string StartErrorMessage = $"Failed to perform the safety check.{Environment.NewLine}Please check the Log for details.";

                public const string CompletionConfirmationTitle = "Safety Check";
                public static readonly string CompletionConfirmationMessage = $"The safety check completed successfully.{Environment.NewLine}The timers will be reset and the plan will be unloaded.";

                public const string HistoryListLoadError = "Failed to load the safety check list.";

                public const string CreatePlanErrorMessage = "Failed to create a new safety check emission plan.";
                public static readonly string SaveDataErrorMessage = $"Failed to save safety check data.{Environment.NewLine}Retry the operation and restart the system if needed. If the problem persists, please contact Support.";
            }

            public static class QualityCheck
            {
                public const string CustomModeConfirmationMessage = "Do you want to switch to custom field selection mode?";
                public const string FullModeConfirmationMessage = "Do you want to discard the changes and generate the full set of fields?";

                public const string NotificationTitle = "QC Execution";
                public const string CompletionNotification = "The QC emission plan completed successfully.";

                public const string DeleteFieldConfirmationMessage = "Do you want to delete the selected fields?";

                public const string DiscardChangesConfirmationTitle = "Unsaved Data Will Be Lost";
                public const string DiscardChangesConfirmationMessage = "Do you want to proceed and discard the changes?";

                public const string CreateCollectionError = "Failed to create a new collection of fields.";

                public const string FieldOperationErrorTitle = "Field Operation Error";
                public static readonly string AddFieldErrorMessage = $"Failed to add a new field.{Environment.NewLine}Retry the operation and restart the system if needed. If the problem persists, please contact Support.";
                public static readonly string RemoveFieldErrorMessage = $"Failed to remove the field.{Environment.NewLine}Retry the operation and restart the system if needed. If the problem persists, please contact Support.";

                public static readonly string BoardConnectionCheckFailed = $"Failed to connect to the QC board.{Environment.NewLine}Retry the operation and restart the system if needed.{Environment.NewLine}If the problem persists, please contact Support."; // with TreatmentConsole.PlanPreparationErrorTitle
            }

            public static class Imaging
            {
                public const string CompletionConfirmationTitle = "Image Acquisition Completed";
                public static readonly string CompletionConfirmationMessage = $"Image acquisition completed successfully.{Environment.NewLine}The timers will be reset and the plan will be unloaded.";

                public static readonly string IgnoreMissingQcConfirmation = $"No successful quality checks within 24 hours.{Environment.NewLine}Please perform a quality check.{Environment.NewLine}Do you want to proceed with the image acquisition anyway?";

                public const string NoActiveHeadErrorMessage = "Cannot prepare the imaging plan. No active head data.";
                public const string ActiveHeadImagingFieldMismatchErrorMessage = "Cannot prepare the imaging plan. Active head imaging field mismatch.";

                public const string ImagingPlanNotDefined = "An imaging plan is not defined.";
                public const string FailedAckImagingPlan = "Failed to confirm the imaging plan loading.";
                public const string FailedCheckPendingImagingPlan = "Failed to check for a pending imaging plan.";
            }

            public static class DetectorCalibration
            {
                public const string CalibrationTitle = "Calibration";
                public const string ApplyDataQuestion = "Do you want to apply the acquired calibration data?";
                public const string FailedToApplyMessage = "Failed to apply the calibration data.";
                public const string FailedToStartMessage = "Failed to start a calibration procedure.";
                public const string FailedToPreparePlanMessage = "Failed to prepare a calibration plan.";
                public const string FailedToAcquireCalibrationDataMessage = "Failed to acquire calibration data.";
                public const string GainCompleteMessage = "Gain calibration data acquired. Apply?";
                public const string OffsetCompleteMessage = "Offset calibration complete.";
                public const string ProgressStringFormat = "Calibration in progress: {0:F0} %";

                public static class Validation
                {
                    public const string DurationIsRequired = "Please enter a duration.";
                    public const string DurationIsNotSetErrorMessage = "Duration is not set.";
                }
            }
        }

        public static class EMR
        {

            public static class Physics 
            {
                public const string ApproveDialogTitle = "Approve Configuration";
            }

            public const string PlanUnloadFromConsoleError = "Cannot unload the plan.";
            public const string PlanUnloadFromConsoleErrorLogMessage = "Unload plan error.";
            public static readonly string PatientIsNotSelectedErrorMessage = $"Patient is not selected.{Environment.NewLine}Please retry opening the patient record.";


            public static class Plan
            {
                public const string FetchErrorMessage = "Failed to get treatment fields from the database.";
                public const string FetchUiErrorMessage = $"{FetchErrorMessage} {DetailsMessage}";

                public const string SaveErrorMessage = "Failed to save treatment fields to the database.";
                public const string SaveUiErrorMessage = $"{SaveErrorMessage} {DetailsMessage}";

                public const string LoadForTreatmentErrorMessage = "Failed to load plan for treatment.";
                public const string LoadForTreatmentUiErrorMessage = $"{LoadForTreatmentErrorMessage} {DetailsMessage}";

                public static readonly string UpdateTreatmentFieldErrorMessage = $"Failed to update treatment field with id {0}.{Environment.NewLine}Field not found in the list. Please reload the treatment plan.";
                public const string PlanIsNotSetErrorMessage = "Plan is not set.";
                public const string HandleSelectionEventErrorMessage = "Failed to handle a selection changed event.";
                public const string PlanStreamErrorMessage = "Plan stream error occurred.";
                public static readonly string HandlePlanEventErrorMessage = $"Failed to handle database plan status event.{Environment.NewLine}{Common.Generic.RestartOrContactSupportMessageFooter}";

                public const string PlanEventReceivedMessage = "Plan treatment event received. Plan id={0}, TreatmentLoadingState: {1}.";

                public const string TargetDoesNotMatchUiMessage = "The connected target {0} does not match to the plan target {1}.";
                public const string TotalDurationExceedLimitUiMessage = "Total plan duration should not exceed {0} seconds.";
                public const string UnsavedPlanChangesUiMessage = "You will lose unsaved changes in the plan form. Continue?";
                public const string SaveTreatmentPlanChangesUiMessage = "Please save treatment plan changes.";

                public const string DeleteTreatmentFieldsDialogTitle = "Delete Treatment Fields";
                public const string DeleteTreatmentFieldsConfirmationUiMessage = "Are you sure you want to delete the selected treatment fields?";

                public static readonly string ValidationErrorMessage = "Failed to validate the treatment field.";
                public static readonly string ValidationUiErrorMessage = 
                    $"This plan's fields contain invalid values. " +
                    $"They were calculated using different emission power, dose rate, or output factors.";
                public static readonly string NoMatchingHeadUiErrorMessage = 
                    $"There is no head configuration for this plan's target type. " +
                    $"Please provide the head and its configuration to be able to edit the plan.";

                public const string ResumeTreatmentDialogTitle = "Resume Treatment";
                public const string TreatmentWasNotCompletedUiMessage = "The last treatment wasn't completed. Do you want to resume it?";

                public const string ApprovePlanDialogTitle = "Approve Plan";
                public const string LookForLoadedImagingPlanErrorMessage = "Failed to look for a loaded imaging plan.";

                public static class Validation
                {
                    // Treatment Field
                    public const string DwellTimeMustBeNonZero = "Duration cannot be 0.";
                }
            }


            public static class Images
            {
                public const string FetchImagesErrorMessage = "Failed to fetch patient images from the database.";
                public const string FetchImagesUiErrorMessage = $"{FetchImagesErrorMessage} {DetailsMessage}";

                public const string DicomFileTransferTitle = "DICOM file transfer";
                public const string InvalidDicomFileCrc = "Checksum of a received DICOM file does not match the provided value.";
            }

            public static class PatientImages
            {
                public const string LoadDICOMErrorMessage = "Failed to load DICOM file.";
                public const string LoadDICOMUiErrorMessage = $"{LoadDICOMErrorMessage} {DetailsMessage}";

                public const string DICOMFileNotSpecifiedErrorMessage = "DICOM file name is not specified.";
            }

            public static class PatientProfile
            {
                public static readonly string SaveErrorMessage = $"Failed to save patient picture to the database.{Environment.NewLine}Retry the operation and restart the system if needed. If the problem persist, contact Support.";
                public static readonly string SaveUiErrorMessage = $"{SaveErrorMessage}{Environment.NewLine}{DetailsMessage}";
            }

            public static class Patients
            {
                public const string FetchPatientsErrorMessage = "Failed to get the patient list.";
                public const string FetchPatientsUiErrorMessage = $"{FetchPatientsErrorMessage} {DetailsMessage}";

                public const string SavePatientErrorMessage = "Failed to save the patient to the database.";
                public const string SavePatientUiErrorMessage = $"{SavePatientErrorMessage} {DetailsMessage}";

                public const string CreatePatientErrorMessage = "Failed to create the patient.";
                public const string CreatePatientUiErrorMessage = $"{CreatePatientErrorMessage} {DetailsMessage}";

                public const string PatientExistDialogTitle = "The patient already exists";
                public const string PatientExistsUiErrorMessage = "There is a patient with the same first name, last name, sex, and date of birth in the database.";

                public const string SaveNewPatientAuditLogMessage = "Create a new patient.";
                public const string SaveExistingPatientAuditLogMessage = "Update data for the patient.";
                public const string SavePatientIsDoneAuditLogMessage = "Saved data for the patient.";
                public const string PatientUnderTreatmentLookupError = "Patient under treatment lookup failed.";

                public static class Validation
                {
                    public const string FirstNameIsRequired = "Please enter the first name.";
                    public const string FirstNameIsNotSetErrorMessage = "The first name is not set.";

                    public const string LastNameIsRequired = "Please enter the last name.";
                    public const string LastNameIsNotSetErrorMessage = "The last name is not set.";

                    public const string MrnIsRequired = "Please enter the medical record number (MRN).";
                    public const string MrnIsNotSetErrorMessage = "The medical record number (MRN) is not set.";

                    public const string SexIsRequired = "Please select the sex.";
                    public const string SexIsNotSetErrorMessage = "The sex is not set.";

                    public const string DateOfBirthIsRequired = "Please enter the date of birth.";
                    public const string DateOfBirthIsNotSetErrorMessage = "The date of birth is not set.";
                }
            }

            public static class Prescription
            {
                public const string FetchPrescriptionError = "Failed to get the prescription.";
                public const string FetchPrescriptionUiErrorMessage = $"{FetchPrescriptionError} {DetailsMessage}";

                public const string SavePrescriptionErrorMessage = "Failed to save the prescription to the database.";
                public const string SavePrescriptionUiErrorMessage = $"{SavePrescriptionErrorMessage} {DetailsMessage}";

                public const string CreatePrescriptionErrorMessage = "Failed to create the prescription.";
                public const string CreatePrescriptionUiErrorMessage = $"{CreatePrescriptionErrorMessage} {DetailsMessage}";

                public const string UnsavedChangedUiMessage = $"You will lose unsaved changes in the prescription form. Continue?";

                public static class Validation
                {
                    public const string TreatmentTypeIsRequired = "Please select the treatment type.";
                    public const string TreatmentTypeIsNotSetErrorMessage = "The treatment type is not set.";

                    public const string SiteNameIsRequired = "Please enter the site name.";
                    public const string SiteNameIsNotSetErrorMessage = "The site name is not set.";

                    public const string VolumeIsRequired = "Please select the volume.";
                    public const string VolumeIsNotSetErrorMessage = "The volume is not set.";

                    public const string DosePerFractionIsRequired = "Please enter the dose per fraction.";
                    public const string DosePerFractionIsNotSetErrorMessage = "The dose per fraction is not set.";

                    public const string FractionIsRequired = "Please enter the fraction.";
                    public const string FractionFormatErrorMessage = $"Please enter two positive numbers separated by /.";
                    public const string FractionCompareErrorMessage = $"Fractions per week ({{0}}) must be less than or equal to the number of fractions ({{1}}).";
                }
            }

            public static class ImagingView
            {
                public const string FetchProtocolsErrorMessage = "Failed to get the imaging protocols.";
                public const string FetchProtocolsUiErrorMessage = $"{FetchProtocolsErrorMessage} {DetailsMessage}";

                public const string SaveProtocolErrorMessage = "Failed to save the imaging protocol.";
                public const string SaveProtocolUiErrorMessage = $"{SaveProtocolErrorMessage} {DetailsMessage}";

                public const string CreateProtocolErrorMessage = "Failed to create the imaging protocol.";
                public const string CreateProtocolUiErrorMessage = $"{CreateProtocolErrorMessage} {DetailsMessage}";

                public const string DeleteProtocolErrorMessage = "Failed to delete the imaging protocol.";
                public const string DeleteProtocolUiErrorMessage = $"{DeleteProtocolErrorMessage} {DetailsMessage}";

                public const string LoadForImagingErrorMessage = "Failed to load the plan for imaging.";
                public const string LoadForImagingUiErrorMessage = $"{LoadForImagingErrorMessage} {DetailsMessage}";

                public const string ProtocolIsNotSelectedErrorMessage = "The protocol is not selected.";
                public const string ActiveHeadIsNotSetErrorMessage = "The active head is not set.";
                public const string ProtocolNameIsNotSpecifiedError = "The adjusted protocol name is not specified.";
                
                public const string CaptureDetectorTagsErrorMessage = "Failed to capture detector tags.";
                public const string CaptureDetectorTagsUiErrorMessage = $"{CaptureDetectorTagsErrorMessage} {DetailsMessage}";

                public const string RobotRepositionDialogHeader = "Robotic Arm Reposition";
                public const string InvalidRobotPosition = "Invalid robotic arm position.";
                public const string RobotMovementFailed = "Robotic arm movement failed.";
                public const string FailedToGetRobotPosition = "Failed to get the robotic arm's position.";
                public const string FinalRepositionCalculationFailed = "Final reposition calculation failed.";

                public const string RobotEnableHandguidingErrorMessage = "Failed to enable robotic arm hand guiding.";
                public const string RobotEnableHandguidingUiErrorMessage = $"{RobotEnableHandguidingErrorMessage} {DetailsMessage}";
                
                public const string ResultIsNotEmptyErrorMessage = "Result is not empty.";

                public const string FailedToRepositionErrorMessage = "Failed to reposition.";
                public const string RepositionMatrixIsNullError = "Reposition matrix is null.";
            }
        }

        public static class Configuration
        {
            public const string ActiveHeadNotFound = "Active head not found.";

            public const string ActiveHeadFetchErrorTitle = "Get Active Head";
            public const string ActiveHeadFetchErrorMessage = "Failed to get the active head.";

            public const string ActivePresetFetchErrorTitle = "Get Active Preset Configuration";
            public const string ActivePresetFetchErrorMessage = "Failed to get the active preset configuration.";

            public static string NoPresetForEnergy = "No preset for {0}kV energy level.";
        }

        public static class Physics
        {
            public const string PhysicsDataDialogTitle = "Physics Data";

            public static class Validation
            {
                public const string XCoilCurrentRequired = "X-coil deflection current is required.";
                public const string XCoilCurrentIsZero = "X-coil deflection current can't be 0.";
                public const string XCoilCurrentIsNotSet = "X-coil deflection current is not set.";

                public const string YCoilCurrentRequired = "Y-coil deflection current is required.";
                public const string YCoilCurrentIsZero = "Y-coil deflection current can't be 0.";
                public const string YCoilCurrentIsNotSet = "X-coil deflection current is not set.";

                public const string FocusCurrentRequired = "Focus current is required.";
                public const string FocusCurrentIsZero = "Focus current can't be 0.";
                public const string FocusCurrentIsNotSet = "Focus current is not set.";

                public const string HeaterCurrentRequired = "Heater current is required.";
                public const string HeaterCurrentIsNotSet = "Heater current is not set.";

                public const string FactorIsNotSet = "Factor is not set.";
                public const string DurationRequired = "Duration is required.";

                public const string CorrectionMatrixValueIsNotSet = "Correction matrix value is not set.";
                public static readonly string ReferenceFieldValueIsNotSet = $"Reference field value is not set.{Environment.NewLine}Please set the reference field value.";
            }
            
            public const string FetchApplicatorDataUiErrorMessage = "Failed to load applicator data";
            public const string CsvImportUiErrorMessage = "Failed to import configuration from CSV file";
            public const string ConfigurationFileLoadErrorTitle = "Failed to Load the Configuration File";
            public const string CsvImportErrorTitle = "Physics Data Import Error";
            public static readonly string CsvImportMissingSourceError = $"There is no active source.{Environment.NewLine}Please specify an active source to be able to import its configuration";
            public const string CsvImportDataMissingError = "Head data is missing in the file.";
            public static readonly string CsvFileReadError = $"Could not read data from the file.";
            public static readonly string CsvWrongSourceTypeError = $"Source type in the file doesn't match the actual source type.{Environment.NewLine}Please select another file.";
            public const string CsvFileFormatError = "The configuration file contains inconsistent or invalid data. See the log for details.";

            public const string ConfigurationResetError = "Failed to reset configuration.";

            public const string OutputFactorDialogTitle = "Output Factor Configuration";
            public const string OutputFactorResetWarning = "Do you want to reset the configuration? All output factor values will be cleared.";
            public const string OutputFactorSubmitError = "You must configure all collimator fields.";
            public static readonly string OutputFactorsSaveErrorMessage = $"Failed to save output factors.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
            public static readonly string ConfigurationSaveErrorMessage = $"Failed to save the configuration data.{Environment.NewLine}{Common.NoDatabaseConnectionErrorMessage}{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public static readonly string LeaveConfigurationTabConfirmationMessage = $"There are unsaved changes in the tab.{Environment.NewLine}Are you sure you want to exit?";
            public const string ReloadConfigurationConfirmationTitle = "Reload Physics Data";
            public static readonly string ReloadConfigurationConfirmationMessage = $"There are unsaved changes in the tab.{Environment.NewLine}Do you want to reload physics data?";

            public static readonly string CoilConfigurationSaveErrorMessage = $"Failed to save coil configuration values.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
            public static readonly string TargetPointsSaveErrorMessage = $"Failed to save target points configuration.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public static readonly string HeaterCurrentSaveErrorMessage = $"Failed to save heater current configuration.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";

            public static readonly string MagnetometerConfigurationSaveErrorMessage = $"Failed to save magnetometer configuration.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
            public static readonly string FailedToStoreCsvErrorMessage = $"Failed to store the configuration to a CSV file.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
            public const string StoreCsvDialogTitle = "Storing Configuration";

            public static readonly string SavedDataRequiredApprovalMessage = $"Physics data saved successfully.{Environment.NewLine}Note: this configuration will become applicable after approval by an authorized user.";

            public static readonly string PhysicsDataIsInvalid = $"Physics data contains a missing field or invalid values.{Environment.NewLine}Please ensure that the values are all present and valid.";
            public static string PhysicsDataIsModified = "Physics data contains unsaved changes.";
        }

        public static class QualityCheck
        {
            public const string QcTitle = "Quality Check";

            public const string QcDataLoadError = "Failed to load quality check data.";
            public const string UpdateDeviationValuesErrorMessage = "Failed to update deviation values for the selected QC sample.";

            public const string SetAsReferenceTitle = "Set as Reference";
            public const string SetAsReferenceErrorMessage = "Failed to set the QC sample as a reference.";

            public const string ApproveQcSampleDialogTitle = "Approve";
        }

        public static class SystemSettings
        {

            public static class Network
            {
                public const string FetchErrorMessage = "Failed to get network settings. Please retry the operation.";
                public const string FetchUiErrorMessage = $"{FetchErrorMessage} {DetailsMessage}";

                public const string SaveErrorMessage = "Failed to save network settings. Please retry the operation.";
                public const string SaveUiErrorMessage = $"{SaveErrorMessage} {DetailsMessage}";
                public static string MacAddressRetrievalUiErrorMessage = $"Failed to retrieve the MAC address.{Environment.NewLine}Please check the Robotic Arm Controller endpoint and try again.";
                public const string SettingsIsNotSetErrorMessage = "Network settings are not set.";
            }



            public static class ImagingProtocols
            {
                public const string FetchErrorMessage = "Failed to get imaging protocols from the database.";
                public const string FetchUiErrorMessage = $"{FetchErrorMessage} {DetailsMessage}";

                public const string SaveErrorMessage = "Failed to save the imaging protocol to the database.";
                public const string SaveUiErrorMessage = $"{SaveErrorMessage} {DetailsMessage}";

                public const string CreateErrorMessage = "Failed to create the imaging protocol.";
                public const string CreateUiErrorMessage = $"{CreateErrorMessage} {DetailsMessage}";

                public const string DeleteErrorMessage = "Failed to delete the imaging protocol.";
                public const string DeleteUiErrorMessage = $"{DeleteErrorMessage} {DetailsMessage}";
                public const string DeleteConfirmationMessage = "Are you sure you want to delete the protocol?";

                public const string ProtocolIsNotSelectedErrorMessage = "Imaging protocol is not selected.";

                public static class Validation
                {
                    public const string ProtocolNameIsRequired = "Please enter the protocol name.";
                    public const string ProtocolNameIsNotSetErrorMessage = "Protocol name is not set.";

                    public const string DoseIsRequired = "Please enter the dose.";
                    public const string DoseIsNotSetErrorMessage = "Dose is not set.";

                    public const string SourceToImageDistanceIsRequired = "Please enter the source-to-image distance.";
                    public const string SourceToImageDistanceIsNotSetErrorMessage = "The source-to-image distance is not set.";

                    public const string FieldOfViewIsRequired = "Please enter the field of view.";
                    public const string FieldOfViewIsNotSetErrorMessage = "The field of view is not set.";

                    public const string RadiusIsRequired = "Please enter the radius.";
                    public const string RadiusIsNotSetErrorMessage = "The radius is not set.";

                    public const string NumberOfProjectionsIsRequired = "Please enter the number of projections.";
                    public const string NumberOfProjectionsIsNotSetErrorMessage = "The number of projections is not set.";

                    public const string TimeIsRequired = "Please enter the time.";
                    public const string TimeIsNotSetErrorMessage = "The time is not set.";

                    public const string AngleIsRequired = "Please enter the angle.";
                    public const string AngleIsNotSetErrorMessage = "The angle is not set.";

                    public const string SliceThicknessIsRequired = "Please enter the slice thickness.";
                    public const string SliceThicknessIsNotSetErrorMessage = "The slice thickness is not set.";

                    public const string EntranceDoseIsRequired = "Please enter the entrance dose.";
                    public const string EntranceDoseIsNotSetErrorMessage = "The entrance dose is not set.";

                    public const string SkinDoseIsRequired = "Please enter the skin dose.";
                    public const string SkinDoseIsNotSetErrorMessage = "The skin dose is not set.";
                }
            }


            public static class UserManagement
            {
                public static readonly string FetchUsersErrorMessage = $"Failed to get users from the database.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string FetchUsersUiErrorMessage = $"{FetchUsersErrorMessage} {DetailsMessage}";

                public static readonly string SaveUserErrorMessage = $"Failed to save the user to the database.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string SaveUserUiErrorMessage = $"{SaveUserErrorMessage} {DetailsMessage}";

                public static readonly string CreateUserErrorMessage = $"Failed to create the user.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string CreateUserUiErrorMessage = $"{CreateUserErrorMessage} {DetailsMessage}";
                
                public static readonly string DeleteUserErrorMessage = $"Failed to remove the user.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string DeleteUserUiErrorMessage = $"{DeleteUserErrorMessage} {DetailsMessage}";
                public const string DeleteUserConfirmationUiMessage = "Are you sure you want to delete the user?";
                public const string DeleteAuthorizedUserUiMessage = "Cannot delete a user that is currently logged in.";

                public const string UserIsNotSelectedErrorMessage = "User is not selected.";

                public static class Validation
                {
                    public const string FirstNameIsRequired = "Please enter the first name.";
                    public const string FirstNameIsNotSetErrorMessage = "The first name is not set.";

                    public const string LastNameIsRequired = "Please enter the last name.";
                    public const string LastNameIsNotSetErrorMessage = "The last name is not set.";

                    public const string UsernameIsRequired = "Please enter the username.";
                    public const string UsernameIsNotSetErrorMessage = "The username is not set.";

                    public const string PasswordIsRequired = "Please enter the password.";
                    public const string PasswordIsNotSetErrorMessage = "The password is not set.";

                    public const string PasswordConfirmIsRequired = "Please confirm the password.";

                    public const string EmailIsRequired = "Please enter the email adderess.";
                    public const string EmailIsNotSetErrorMessage = "The email address is not set.";
                }
            }


            public static class UserRoles
            {
                public static readonly string FetchUserRolesErrorMessage = $"Failed to get user roles from the database.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string FetchUserRolesUiErrorMessage = $"{FetchUserRolesErrorMessage} {DetailsMessage}";

                public static readonly string SaveUserRoleErrorMessage = $"Failed to save the user role to the database.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string SaveUserRoleUiErrorMessage = $"{SaveUserRoleErrorMessage} {DetailsMessage}";

                public static readonly string CreateUserRoleErrorMessage = $"Failed to create the user role.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string CreateUserRoleUiErrorMessage = $"{CreateUserRoleErrorMessage} {DetailsMessage}";

                public static readonly string DeleteUserRoleErrorMessage = $"Failed to delete the user role.{Environment.NewLine}{Common.Generic.RetryOrContactSupportMessageFooter}";
                public static readonly string DeleteUserRoleUiErrorMessage = $"{DeleteUserRoleErrorMessage} {DetailsMessage}";
                public const string DeleteRoleConfirmationUiMessage = "Are you sure you want to delete the role?";
                public const string DeleteAuthorizedUserRoleUiMessage = "Cannot delete the role of the user that is currently logged in.";

                public const string UserRoleIsNotSelectedErrorMessage = "User role is not selected.";
            }


            public static class HeadManagement
            {
                public const string FetchHeadsErrorMessage = "Failed to get heads from the database.";
                public const string FetchHeadsUiErrorMessage = $"{FetchHeadsErrorMessage} {DetailsMessage}";

                public const string SaveHeadErrorMessage = "Failed to save the head to the database.";
                public const string SaveHeadUiErrorMessage = $"{SaveHeadErrorMessage} {DetailsMessage}";

                public const string CreateHeadErrorMessage = "Failed to create the head.";
                public const string CreateHeadUiErrorMessage = $"{CreateHeadErrorMessage} {DetailsMessage}";

                public const string HeadIsNotSelectedErrorMessage = "The head is not specified.";
                
                public const string ConfirmActiveHeadSwitchUiMessage = "There's another active head. Do you want to switch it?";

                public static class Validation
                {
                    public const string SerialIsRequired = "Please enter the serial number.";
                    public const string TypeIsRequired = "Please select the type.";
                    public const string ImagingFieldRequired = "Please select the imaging field.";

                    public const string TypeIsNotSetErrorMessage = "The head target type is not set.";
                    public const string ImagingFieldIsNotSetErrorMessage = "The head imaging field is not set.";
                }
            }
        }

        public static class Camera
        {
            public const string CameraUnavailableUiErrorMessage = "Camera is not available";
            public static readonly string NoUriUiErrorMessage = $"Video URI is not specified. {Environment.NewLine}Please check the camera settings.";
            public static readonly string NoConnectionUiErrorMessage = $"Failed to open live video feed. {Environment.NewLine}Please check the camera settings and network connection.";
        }
    }
}
