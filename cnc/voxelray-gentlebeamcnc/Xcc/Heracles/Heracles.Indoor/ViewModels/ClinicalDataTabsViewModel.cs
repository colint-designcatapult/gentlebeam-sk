using Prism.Events;
using Prism.Regions;

using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;
using System.Threading.Tasks;
using Heracles.Application.Models.Treatment;
using Heracles.Indoor.Models.UseCases;
using Prism.Commands;

namespace Heracles.Indoor.ViewModels
{
    public class UnloadFromTreatmentEvent : PubSubEvent { }
    public class ViewTreatmentHistoryEvent : PubSubEvent<PatientRecordReadAudit.Request?> { }
    public class ViewPlanningRecordsEvent : PubSubEvent<PatientRecordReadAudit.Request?> { }

    public class ClinicalDataTabsViewModel : RegionViewModelBase
    {
        public ClinicalDataTabsViewModel() : base(null) 
        {
        }

        public ClinicalDataTabsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator,
            ITreatmentInfoStore treatmentInfo, FieldModel fieldModel, PatientRecordReadAudit readAudit) : base(regionManager)
        {
            eventAggregator.GetEvent<UnloadFromTreatmentEvent>().Subscribe(() => SelectedTabIndex = 1, ThreadOption.UIThread);
            TreatmentInfo = treatmentInfo;
            FieldModel = fieldModel;
            ReadAudit = readAudit;
            Events = eventAggregator;
        }

        private ITreatmentInfoStore TreatmentInfo { get; }
        private FieldModel FieldModel { get; }
        private PatientRecordReadAudit ReadAudit { get; }
        private IEventAggregator Events { get; }

        private DelegateCommand<object>? _viewSelectedTabCommand;
        public DelegateCommand<object> ViewSelectedTabCommand => _viewSelectedTabCommand ??= new DelegateCommand<object>(
            index =>
            {
                SelectedTabIndex = (int)index;
                _ = ViewSelectedTabAsync();
            });

        public Task ViewSelectedTabAsync() =>
            ViewSelectedTabAsync(ReadAudit.CreateRequest(TreatmentInfo.Diagnosis?.Id));

        private async Task ViewSelectedTabAsync(PatientRecordReadAudit.Request? request)
        {
            var selectedTab = SelectedTabIndex;
            try
            {
                if (FieldModel.CurrentFieldTask is not null)
                    await FieldModel.CurrentFieldTask.Task;
                if (SelectedTabIndex != selectedTab)
                    return;
                foreach (var field in FieldModel.Fields)
                    if (field.PatientId == request?.PatientId)
                        ReadAudit.Record(request, "diagnosis list", field.Id);
            }
            catch
            {
                // Failed site reads are displayed by the existing task overlay.
                return;
            }
            if (SelectedTabIndex != selectedTab)
                return;
            if (selectedTab == 1)
                Events.GetEvent<ViewTreatmentHistoryEvent>().Publish(request);
            else
            {
                ReadAudit.Record(request, "diagnosis", TreatmentInfo.Diagnosis?.Id ?? 0);
                Events.GetEvent<ViewPlanningRecordsEvent>().Publish(request);
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            ReadAudit.PlanningVisible = SelectedTabIndex == 0;
            base.OnNavigatedTo(navigationContext);

            RegionManager.RequestNavigate(Regions.Main.ClinicalData.PlanRegion, "PlanView");
            RegionManager.RequestNavigate(Regions.Main.ClinicalData.TreatmentsRegion, "TreatmentsView");
            RegionManager.RequestNavigate(Regions.Main.ClinicalData.ImagesRegion, "ImagesView");
            if (ReadAudit.CurrentRequest is { } returnRequest)
                _ = ViewSelectedTabAsync(returnRequest with { PlanningVisible = SelectedTabIndex == 0 });
            else if (navigationContext.Parameters.TryGetValue("UserPatientView", out bool requested) && requested)
                _ = ViewSelectedTabAsync();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            ReadAudit.PlanningVisible = false;
            base.OnNavigatedFrom(navigationContext);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (SetProperty(ref _selectedTabIndex, value) && ReadAudit is not null)
                    ReadAudit.PlanningVisible = value == 0;
            }
        }
    }
}
