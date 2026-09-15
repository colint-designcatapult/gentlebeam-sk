using Heracles.Application.Common;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.Models.UseCases;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using Xcc.Application.Common;
using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels
{
    public class CameraViewModel : RegionViewModelBase, INavigationAware
    {
        #region Properties

        public IDialogService DialogService { get; }
        public IHeraclesMainSettings Settings { get; }
        public ILogRepository LogWriter { get; }
        public IEmrPhotoCommands PhotoCommands { get; }
        public IPatientListModel PatientListModel { get; }
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public IEventAggregator EventAggregator { get; }
        private readonly PatientRecordReadAudit _readAudit;

        private string _cameraUriSource;
        public string CameraUriSource
        {
            get => _cameraUriSource;
            set => SetProperty(ref _cameraUriSource, value);
        }

        private string _pathToDatabase;
        public string PathToDatabase
        {
            get => _pathToDatabase;
            set => SetProperty(ref _pathToDatabase, value);
        }

        private bool _isSavingImage;
        public bool IsSavingImage
        {
            get => _isSavingImage;
            set => SetProperty(ref _isSavingImage, value);
        }
        #endregion Properties


        #region Commands

        private DelegateCommand<string> _screenshotCommand;
        public DelegateCommand<string> ScreenshotCommand => _screenshotCommand ??= new DelegateCommand<string>
            ((pathToScreenshot) =>
            {
                if (pathToScreenshot == null)
                {
                    DialogService.ReportError(StringConstants.Common.SaveErrorTitle, StringConstants.CameraView.PhotoSaveErrorMessage);
                    return;
                }

                //pathToScreenshot is a path to the captured frame. If it null - some error occured and frame was not created.
                Task.Run(async () =>
                {
                    const string modality = "";
                    var seriesId = await SaveNewSeries(modality);
                    if (seriesId == 0)
                        return;

                    int studyId = 1;
                    await SaveNewImage(studyId, pathToScreenshot, PhotoType.Identification); // todo: what type of Photo should be selected here?
                    studyId++;
                });
                
                ExitCommand.Execute();
            });

        private DelegateCommand _saveImageCommand;
        public DelegateCommand SaveImageCommand => _saveImageCommand ??= new DelegateCommand(async () =>
        {
            IsSavingImage = true;
            try
            {
                await SaveImageAsync();
            }
            finally
            {
                IsSavingImage = false;
            }
        });

        #endregion Commands

        protected override void OnExit()
        {
            // The journal synchronously restores the clinical view through this explicit user request.
            _readAudit.InRequest(_readAudit.CreateRequest(TreatmentInfoStore.Diagnosis?.Id), base.OnExit);
        }


        #region Constructors
        public CameraViewModel(
            IRegionManager regionManager,
            IDialogService dialogService,
            IHeraclesMainSettings settings,
            ILogRepository logWriter, 
            IEmrPhotoCommands photoCommands,
            IPatientListModel patientListModel,
            ITreatmentInfoStore treatmentInfoStore,
            IEventAggregator eventAggregator,
            PatientRecordReadAudit readAudit)
            :base(regionManager)
        {
            DialogService = dialogService;
            Settings = settings;
            LogWriter = logWriter;
            PhotoCommands = photoCommands;
            PatientListModel = patientListModel;
            TreatmentInfoStore = treatmentInfoStore;
            EventAggregator = eventAggregator;
            _readAudit = readAudit;

            CameraUriSource = settings.CameraUriSource;

            PathToDatabase = settings.StorageRoot;
        }

        public CameraViewModel() : base(null) {}
        #endregion Constructors


        #region Private methods

        private async Task SaveImageAsync()
        {
            try
            {
                // Capture image from go2rtc API
                byte[] imageBytes = await CaptureFromGoRtcAsync(Settings.ImageUrlTreatmentHead);
                
                // Generate filename with timestamp (matching old pattern: 2026-09-02_14_30_45.123)
                string timestamp = DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HH'_'mm'_'ss'.'fff");
                string filename = $"image_{timestamp}.jpeg";
                
                // Create directory path: {StorageRoot}/Simulations/{DiagnosisId}/
                string diagnosisPath = Path.Combine(
                    PathToDatabase, 
                    "Simulations", 
                    TreatmentInfoStore.Diagnosis.Id.ToString());
                
                // Ensure we have a valid directory path
                diagnosisPath = Path.GetFullPath(diagnosisPath);
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(diagnosisPath))
                {
                    Directory.CreateDirectory(diagnosisPath);
                }
                
                // Save bytes to disk
                string filePath = Path.Combine(diagnosisPath, filename);
                filePath = Path.GetFullPath(filePath);
                
                await File.WriteAllBytesAsync(filePath, imageBytes);
                
                // Verify file was created
                if (!File.Exists(filePath))
                {
                    throw new Exception("Failed to save image file to disk - file does not exist after write");
                }
                
                // Save to database using existing method
                await SaveNewImage(
                    (int)TreatmentInfoStore.Diagnosis.Id, 
                    filePath, 
                    PhotoType.Identification);

                // Show success feedback using Report with Info type
                DialogService.Report("Success", "Image saved successfully", Xcc.Core.Enums.ReportType.Info);
            }
            catch (Exception ex)
            {
                // Show error feedback
                DialogService.Report("Error", "Unable to save image", Xcc.Core.Enums.ReportType.Error);
                LogWriter.Log($"Failed to save image: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
            }
        }
        
        private async Task<byte[]> CaptureFromGoRtcAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                throw new ArgumentException("Image URL is not configured");

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                var response = await httpClient.GetAsync(imageUrl);
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to capture image: {response.StatusCode}");

                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                return imageBytes;
            }
        }

        #endregion Private methods


        #region INavigationAware
        #endregion INavigationAware

        protected async Task<int> SaveNewSeries(string modality)
        {
            //string name = $"series-{DateTime.Now}";

            //double lesionDepth = SimulationModel?.Simulation?.LesionDepth ?? 0.0d;

            //long visitId = 0L;

            //if (SimulationModel.Simulation != null)
            //{
            //    visitId = SimulationModel.Simulation.VisitId;
            //}
            //else if (PatientInTreatment.Patient.Visits.Count > 0)
            //{
            //    visitId = PatientInTreatment.Patient.Visits.FirstOrDefault().Id;
            //}
            //else
            //{
            //    await LogService.LogAsync("Failed to save a new Series: unknown visit_id", LogRecordSeverity.Error, LogRecordType.Error);
            //// TODO: show error
            //    return 0;
            //}

            //// todo: initialize Series parameters with correct data
            //ISeries series = new Series()
            //{
            //    VisitId = visitId,
            //    LesionDepth = lesionDepth,
            //    DiagnosisId = SimulationModel.Diagnosis.Id,
            //    Name = name,
            //    Modality = modality
            //};

            //try
            //{
            //    var resultSeries = await SeriesCommands.CreateAsync(series);
            //    if (resultSeries != null)
            //    {
            //        LogService.Log($"New Series: [{resultSeries.Name}]", LogRecordSeverity.Info, LogRecordType.Database);
            //    }

            //    return (int)resultSeries.Id;
            //} 
            //catch (Exception ex) 
            //{
            //        LogService.Log($"Failed to save a new Series: {ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            //// TODO: show error
            //}

            return -1;
        }

        protected async Task SaveNewImage(int diagnosisId, string location, PhotoType photoType)
        {
            const string debugLogFile = "cameradebug.log";
            const int ChunkSize = 256 * 1024; // 256KB chunks for streaming
            
            try
            {
                // Create a new visit or get a recent one (on the same day)
                var lastVisit = await PatientListModel.GetSameDayVisitAsync(TreatmentInfoStore.Patient, VisitType.Simulation);
                
                TreatmentInfoStore.Patient.Visit = lastVisit;

                // todo: initialize Image parameters with correct data
                // Use SiteLocation enum from diagnosis for the Location
                string locationString = TreatmentInfoStore.Diagnosis?.SiteLocation?.ToString() ?? string.Empty;
                
                IPhotoDescription image = new PhotoDescription
                {
                    CreationDate = DateTime.Now,
                    DiagnosisId = diagnosisId,
                    Type = photoType,
                    Description = "image description",
                    Path = location,           // File system path where image is stored
                    Location = locationString, // Anatomical location (enum name as string, e.g., "Breast")
                    TemplateType = TemplateType.Simulation,
                    VisitId = lastVisit.Id                
                };
                
                var result = await PhotoCommands.CreateAsync(image);
                
                if (result == null)
                {
                    throw new Exception("Failed to create photo in database - result is null");
                }
                    
                LogWriter.Log($"New Image: [{result.Location}]", LogRecordSeverity.Info, LogRecordType.System);

                // CRITICAL: Load image bytes from disk and send them to server
                byte[] imageBytes = File.ReadAllBytes(location);

                // Create Photo object with the image data
                var photo = new Photo(result)
                {
                    Data = imageBytes
                };

                // Send the actual image data to server
                await PhotoCommands.SendPhotoAsync(photo, ChunkSize, CancellationToken.None);

                // Publish event to notify other ViewModels (like PlanViewModel) that a photo was saved
                EventAggregator.GetEvent<PhotoSavedEvent>().Publish(result);
            }
            catch (Exception ex)
            {
                LogWriter.Log($"{StringConstants.CameraView.NewImageSaveErrorMessage}: {ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
                throw; // Rethrow so caller knows about the failure
            }
        }
    }
}
