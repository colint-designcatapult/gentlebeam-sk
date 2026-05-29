
using Heracles.Application.Models;
using Heracles.Application.Models.Treatment;

using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;

using Xcc.Application.Common;
using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Constants;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels
{
    public class ClinicalDataViewModel : RegionViewModelBase
    {
        public ClinicalDataViewModel() : base(null) 
        {
        }

        public ClinicalDataViewModel(
            ITreatmentInfoStore treatmentInfoStore,
            IRegionManager regionManager, 
            IDialogService dialogService,
            ILogWriter logWriter,
            IPlanModel planModel) : base(regionManager, null, dialogService)
        {
            TreatmentInfoStore = treatmentInfoStore;
            LogWriter = logWriter;
            PlanModel = planModel;
        }

        #region Read-only properties
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public ILogWriter LogWriter { get; }
        public IPlanModel PlanModel { get; }
        #endregion Read-only properties


        #region Commands
        private DelegateCommand? _cancelLoadPlanCommand;
        public DelegateCommand CancelLoadPlanCommand
        {
            get => _cancelLoadPlanCommand ??= new DelegateCommand(
                async () => 
                {
                    try
                    {
                        await PlanModel.UnloadFromTreatmentAsync();
                    }
                    catch (Exception ex)
                    {
                        DialogService.ReportError(
                            StringConstants.Common.ErrorTitle, 
                            StringConstants.EMR.PlanUnloadFromConsoleError);
                        
                        await LogWriter.LogAsync(
                            $"{StringConstants.EMR.PlanUnloadFromConsoleErrorLogMessage}: {ex.Message}", 
                            Xcc.Core.Enums.LogRecordSeverity.Error, 
                            Xcc.Core.Enums.LogRecordType.System);
                    }
                });
        }
        #endregion Commands


        #region RegionViewModelBase
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            RegionManager.RequestNavigate(Regions.Main.ClinicalDataRegion, "ClinicalDataTabsView");
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            TreatmentInfoStore.Plan = null;
            TreatmentInfoStore.Prescription = null;
            TreatmentInfoStore.Simulation = null;
            TreatmentInfoStore.Diagnosis = null;
            TreatmentInfoStore.Patient = null;

            base.OnNavigatedFrom(navigationContext);
        }
        #endregion RegionViewModelBase
    }
}
