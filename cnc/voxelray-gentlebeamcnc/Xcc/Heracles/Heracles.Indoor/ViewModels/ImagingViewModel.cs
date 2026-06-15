using Heracles.Application.DeepColor;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.Indoor.AppLayer.DeepColor;

using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels;

public enum ImagingViewType
{
    Acquisition,
    Viewer
}

[RegionMemberLifetime(KeepAlive = true)]
public class ImagingViewModel(
    IRegionManager regionManager, 
    IDialogService dialogService,
    ILogRepository logWriter, 
    IHeraclesMainSettings heraclesMainSettings,
    ITreatmentInfoStore treatmentInfoStore, 
    ISeriesModel seriesModel,
    IPatientListModel patientListModel, 
    IEventAggregator eventAggregator, 
    IPopUpService popUpService,
    IpcModel ipcModel,
    AcquisitionService acquisitionService,
    IAcquisitionResultStore acquisitionResultStore) : BindableBase, INavigationAware
{
    #region Injected Dependencies
    private IHeraclesMainSettings HeraclesMainSettings { get; } = heraclesMainSettings;
    public IRegionManager RegionManager { get; } = regionManager;
    public IDialogService DialogService { get; } = dialogService;
    private ILogRepository LogWriter { get; } = logWriter;
    private ITreatmentInfoStore TreatmentInfoStore { get; } = treatmentInfoStore;
    private ISeriesModel SeriesModel { get; } = seriesModel;
    private IPatientListModel PatientListModel { get; } = patientListModel;
    public IEventAggregator EventAggregator { get; } = eventAggregator;
    private IAcquisitionResultStore AcquisitionResultStore { get; } = acquisitionResultStore;
    private IPopUpService PopUpService { get; } = popUpService;
    private IpcModel IpcModel { get; } = ipcModel;
    public AcquisitionService AcquisitionService { get; } = acquisitionService;
    #endregion Injected Dependencies



    #region Private Methods
    private async Task<ISeries> SaveSeriesAsync()
    {
        ISeries _series = null;
        IVisit lastVisit = null;

        double lesionDepth = 0.0;

        var storedLesionDepth = AcquisitionResultStore.LesionInfo.LesionDepth;
        if (storedLesionDepth.HasValue)
            lesionDepth = storedLesionDepth.Value;
        else
            await LogWriter.LogAsync($"LesionDepth = {lesionDepth}", LogRecordSeverity.Warn, LogRecordType.User);

        try
        {
            // Create a new visit or get a recent one (on the same day)
            lastVisit = await PatientListModel.GetSameDayVisitAsync(TreatmentInfoStore.Patient, VisitType.Simulation);
            TreatmentInfoStore.Patient.Visit = lastVisit;
        }
        catch (Exception ex)
        {
            PopUpService.LogAndShowError(
                Application.Common.StringConstants.Common.ErrorTitle,
                Application.Common.StringConstants.PhotoAcousticView.VisitCreateError,
                ex);
            return null;
        }

        try
        {
            _series = new Series
            {
                DiagnosisId = TreatmentInfoStore.Diagnosis.Id,
                Type = ImageType.Photoacoustic,
                VisitId = lastVisit.Id,
                Name = "Slices: " + 0,
                Location = "TODO: path to dcm",
                LesionDepth = lesionDepth,
                //Description = TreatmentInfoStore.Diagnosis.SiteName,
                Description = "Slices: " + 0,
                NumberOfInstances = 0,
            };

            _series = await SeriesModel.CreateSeriesAsync(_series);

            var PID = TreatmentInfoStore.Patient.Id;
            var SID = lastVisit.Id;
            var SerID = _series.Id;

            var path = Locations.SeriesLocation(PID, SID, SerID);

            var updatedSeries = new Series(_series)
            {
                Location = path,
            };

            _series = await SeriesModel.UpdateLastSeriesAsync(updatedSeries);

        }
        catch (Exception ex)
        {
            PopUpService.LogAndShowError(
                Application.Common.StringConstants.Common.ErrorTitle,
                Application.Common.StringConstants.PhotoAcousticView.SeriesCreateError,
                ex);

            return null;
        }

        return _series;
    }

    void UpdateSeries(ISeries series)
    {
        Task.Run(async () =>
        {
            try
            {
                series = await SeriesModel.UpdateLastSeriesAsync(series);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.PhotoAcousticView.SeriesCreateError,
                    ex);
            }
        });
    }

    //protected override void OnExit()
    //{
    //    if (AcquisitionResultStore.LesionInfo.LesionDepth.HasValue)
    //    {
    //        if (_series != null)
    //        {
    //            _series.LesionDepth = AcquisitionResultStore.LesionInfo.LesionDepth.Value;

    //            UpdateSeries(_series);
    //        }
    //    };

    //    EventAggregator.GetEvent<AcquisitionCompletedEvent>().Publish();

    //    base.OnExit();
    //}
    #endregion Private Methods



    #region Observble task
    private ObservableTask? _currentTask;
    public ObservableTask? CurrentTask { get => _currentTask; set => SetProperty(ref _currentTask, value); }

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
    #endregion Observble task



    #region 'Exit' button logic
    private DelegateCommand? _exitCommand;
    public DelegateCommand? ExitCommand
    {
        get => _exitCommand;
        set => SetProperty(ref _exitCommand, value);
    }

    private void SaveAcquisitionsAndExit()
    {
        CurrentTask = new ObservableTask(SaveAcquisitionsAndExitAsync(), "Failed to get and save acquisitions. See details in the log.");

        CancelCurrentTaskCommand = new DelegateCommand(() =>
        {
            CurrentTask = null;
            Exit();
        });
    }

    private async Task SaveAcquisitionsAndExitAsync()
    {
        try
        {
            var acquisitions = await IpcModel.GetAcquisitionList();

            var storedAcquisitions = await AcquisitionService.FetchAcquisitionsAsync(TreatmentInfoStore.Diagnosis.Id);

            var storedIds = storedAcquisitions.Select(d => d.Id).ToHashSet();

            ////string message = acquisitions.Length == 0
            ////    ? "No acquisitions"
            ////    : string.Join(", ", acquisitions.Select(x => $"[Name {x.Name}, Id {x.Id}]"));
            ////DialogService?.Report("List of acquisitions", message, ReportType.Info);

            // If we create a new simulation, we need to attach it to a new visit.
            // However, we don't create a visit if there's already one for the same date
            TreatmentInfoStore.Patient.Visit = await PatientListModel.GetSameDayVisitAsync(TreatmentInfoStore.Patient, visitType: VisitType.Simulation);

            var itemsToStore = acquisitions.Where(x => !storedIds.Contains(x.Id));
            foreach (var acq in itemsToStore)
            {
                await AcquisitionService.CreateAcquisitionAsync(acq, TreatmentInfoStore.Simulation.VisitId, TreatmentInfoStore.Diagnosis.Id);
            }

            Exit();
        }
        catch (Exception e)
        {
            _ = LogWriter.LogAsync("Failed to get and save acquisitions" + e.Message, LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    private void Exit()
    {
        if (RegionName is not null)
        {
            RegionManager.Regions[RegionName].NavigationService.Journal.GoBack();
        }
    }
    #endregion 'Exit' button logic



    #region DeepColorGui communication
    public bool FirstDeepColorRun { get; set; } = true;

    public void PrepareAcquisition()
    {
        CurrentTask = new ObservableTask(PrepareAcquisitionAsync(), "Failed to prepare acquisition. See details in the log.");

        CancelCurrentTaskCommand = new DelegateCommand(() =>
        {
            CurrentTask = null;
            Exit();
        });
    }

    public async Task PrepareAcquisitionAsync()
    {
        try
        {
            if (FirstDeepColorRun)
                await Task.Delay(10000);

            await IpcModel.TestConnectionAsync(10);
            await IpcModel.PrepareAcquisition();
        }
        catch (Exception e)
        {
            _ = LogWriter.LogAsync("Failed to prepare acquisition" + e.Message, LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }


    public void LoadAcquisition(int acquisitionId)
    {
        CurrentTask = new ObservableTask(LoadAcquisitionAsync(acquisitionId), "Failed to load acquisition. See details in the log.");

        CancelCurrentTaskCommand = new DelegateCommand(() =>
        {
            CurrentTask = null;
            Exit();
        });
    }

    public async Task LoadAcquisitionAsync(int acquisitionId)
    {
        try
        {
            if (FirstDeepColorRun)
                await Task.Delay(10000);

            await IpcModel.TestConnectionAsync(10);
            await IpcModel.LoadAcquisition(acquisitionId);
        }
        catch (Exception e)
        {
            _ = LogWriter.LogAsync("Failed to prepare acquisition" + e.Message, LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    public void LoadDeepColorApp(Decorator hostPanel)
    {
        if (string.IsNullOrWhiteSpace(HeraclesMainSettings.PathToDeepColorApp))
        {
            DialogService?.ReportError("Acquisition error",
                "Failed to start acquisition: path to DeepColor application is not set.");
            Exit();
            return;
        }

        if (!File.Exists(HeraclesMainSettings.PathToDeepColorApp))
        {
            DialogService?.ReportError("Acquisition error", "Failed to start acquisition: specified executable name doesn't exist.");
            Exit();
            return;
        }

        if (Path.GetExtension(HeraclesMainSettings.PathToDeepColorApp)?.ToLower() != ".exe")
        {
            DialogService?.ReportError("Acquisition error", "Failed to start acquisition: specified executable doesn't exist.");
            Exit();
            return;
        }

        try
        {
            if (hostPanel.Child is null)
            {
                var host = new WindowHost(HeraclesMainSettings.PathToDeepColorApp, "--main_view=skincure --noSplashScreen --hideSideBar");

                host.ProcessExitedOrTerminated += (_, _) => { hostPanel.Child = null; };
                hostPanel.Child = host;
            }
        }
        catch (Exception e)
        {
            DialogService?.ReportError("Acquisition error", "Failed to start acquisition process. See details in the log.");

            _ = LogWriter.LogAsync("Failed to start acquisition process: " + e.Message, LogRecordSeverity.Error, LogRecordType.System);
            Exit();
        }

        FirstDeepColorRun = false;
    }
    #endregion DeepColorGui communication



    #region INavigationAware
    private string? RegionName { get; set; }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        RegionName = navigationContext.NavigationService.Region.Name;

        if (navigationContext.Parameters.TryGetValue("Type", out ImagingViewType type))
        {
            switch (type)
            {
                case ImagingViewType.Acquisition:
                    ExitCommand = new DelegateCommand(SaveAcquisitionsAndExit);

                    PrepareAcquisition();
                    break;
                case ImagingViewType.Viewer:
                    ExitCommand = new DelegateCommand(Exit);
                    
                    if (navigationContext.Parameters.TryGetValue("AcquisitionId", out int acquisitionId))
                    {
                        LoadAcquisition(acquisitionId);
                    }
                    else
                    {
                        string message = $"AcquisitionId parameter is missing for imaging application running in the {ImagingViewType.Viewer} mode.";

                        DialogService?.ReportError("Imaging error", message);
                        _ = LogWriter.LogAsync(message, LogRecordSeverity.Error, LogRecordType.System);
                    }


                    break;
                default:
                    throw new NotSupportedException($"Unsupported imaging application mode {type}");
            }
        }
        else
        {
            string message = $"Imaging application mode is not specified. Expected types {ImagingViewType.Acquisition}, {ImagingViewType.Viewer}.";

            DialogService?.ReportError("Imaging error", message);
            _ = LogWriter.LogAsync(message, LogRecordSeverity.Error, LogRecordType.System);

            Exit();
        }
    }

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    #endregion INavigationAware
}