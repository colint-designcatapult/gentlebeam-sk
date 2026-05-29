using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck.Events;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Core.Enums;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Application.UI.ViewModels
{
    public class BeamQaReportsViewModel : BindableBase
    {
        #region Contructors
        public BeamQaReportsViewModel()
        {
            ShowRawColumns = true;
            ShowDeviationColumns = false;
            if (System.Windows.Application.Current.MainWindow is not null)
            {
                throw new System.Exception("This constructor can be used only in design mode.");
            }
        }

        public BeamQaReportsViewModel(
            IEventAggregator eventAggregator,
            IQcReportListModel qcModel,
            IPopUpService popUpService,
            IDialogService dialogService,
            IDispatcherService dispatcherService,
            ICollimatorModel collimatorModel,
            IQcRepository qcRepository,
            QcReportListService qcListService,
            ILogWriter logWriter)
        {
            EventAggregator = eventAggregator;
            QcModel = qcModel;
            PopUpService = popUpService;
            DialogService = dialogService;
            DispatcherService = dispatcherService;
            CollimatorModel = collimatorModel;
            QcRepository = qcRepository;
            QcListService = qcListService;
            LogWriter = logWriter;
            eventAggregator.GetEvent<OnSetAsReferenceClickedEvent>().Subscribe(OnSetAsReferenceClickedAsync);
            eventAggregator.GetEvent<QualityCheckFinishedEvent>().Subscribe(OnQualityCheckFinished);
            eventAggregator.GetEvent<OnQcSampleApproveClickedEvent>().Subscribe(OnApproveClicked);

            CollimatorModel.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ICollimatorModel.Collimators))
                {
                    GetAvailableTargetTypes();
                }
            };

            GetAvailableTargetTypes();
        }


        #endregion Contructors



        #region Injected Dependencies
        public IEventAggregator EventAggregator { get; }
        public IQcReportListModel QcModel { get; }
        public IPopUpService PopUpService { get; }
        public IDialogService DialogService { get; }
        public IDispatcherService DispatcherService { get; }
        public ICollimatorModel CollimatorModel { get; }
        public IQcRepository QcRepository { get; }
        public QcReportListService QcListService { get; }
        public ILogWriter LogWriter { get; } 
        #endregion Injected Dependencies



        #region Properties        

        private bool _showRawColumns = true;
        public bool ShowRawColumns { get => _showRawColumns; set => SetProperty(ref _showRawColumns, value); }

        private bool _showDeviationColumns = false;
        public bool ShowDeviationColumns { get => _showDeviationColumns; set => SetProperty(ref _showDeviationColumns, value); }

        private IEnumerable<TargetType> _availableTargetTypeValues;
        public IEnumerable<TargetType> AvailableTargetTypeValues
        {
            get => _availableTargetTypeValues;
            private set
            {
                if (SetProperty(ref _availableTargetTypeValues, value))
                    CollimatorType = _availableTargetTypeValues.FirstOrDefault();
            }
        }

        private TargetType _collimatorType;
        public TargetType CollimatorType
        {
            get => _collimatorType;
            set 
            { 
                if (SetProperty(ref _collimatorType, value))
                {
                    OnCollimatorChanged();
                }
            }
        }

        private Energy? _energy;
        public Energy? Energy
        {
            get => _energy;
            set
            {
                if (SetProperty(ref _energy, value))
                {
                    RaisePropertyChanged(nameof(CanReload));
                    FetchQcSamples();
                }
            }
        }

        private SsdType _ssd;
        public SsdType Ssd
        {
            get => _ssd;
            set => SetProperty(ref _ssd, value);
        }





        private DelegateCommand? _reloadCommand;
        public DelegateCommand ReloadCommand => _reloadCommand ??= new DelegateCommand(FetchQcSamples).ObservesCanExecute(() => CanReload);
        public bool CanReload => Energy is not null;



        // TODO: seem no reason to keep the entire sample value as selected,
        // maybe we need to keep its id only
        private IQcSample _selectedQcSample;
        public IQcSample SelectedQcSample
        {
            get => _selectedQcSample;
            set 
            { 
                if (SetProperty(ref _selectedQcSample, value) || value != null)
                {
                    PrepareSelectedSample();
                }            
            }
        }

        #endregion Properties



        #region Observable tasks
        private ObservableTask _currentQcTask;
        public ObservableTask CurrentQcTask
        {
            get => _currentQcTask;
            private set => SetProperty(ref _currentQcTask, value);
        }

        private DelegateCommand? _retryCurrentTaskCommand;
        public DelegateCommand? RetryCurrentTaskCommand
        {
            get => _retryCurrentTaskCommand;
            set => SetProperty(ref _retryCurrentTaskCommand, value);
        }
        #endregion Observable tasks



        #region Private methods 
        private void GetAvailableTargetTypes()
        {
            try
            {
                var collimators = CollimatorModel.Collimators.Where(c => c.Configuration != null && c.IsActive).DistinctBy(x => x.Configuration.Type);

                AvailableTargetTypeValues = collimators.Select(c => c.Configuration.Type)
                    .Where(type => type != TargetType.TargetType_QC_Collimator).Order();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Common.StringConstants.QualityCheck.NoApplicatorConfigurationsError,
                    ex);
            }
        }

        private async void OnSetAsReferenceClickedAsync()
        {            
            if (SelectedQcSample == null)
                return;

            try
            {
                IQcSample updated = await QcListService.SetAsReferenceAsync(SelectedQcSample);

                DispatcherService.Invoke(() =>
                {
                    SelectedQcSample = updated;

                    PopUpService.ShowMessage(
                        StringConstants.Common.QaDialogTitle,
                        StringConstants.Common.RestartExternalOnSaveNotification,
                        ReportType.Info);
                });
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.QualityCheck.SetAsReferenceTitle,
                    StringConstants.QualityCheck.SetAsReferenceErrorMessage,
                    ex);
            }
        }

        private ICollimatorConfiguration GetCollimatorConfiguration()
        {
            return CollimatorModel.Collimators?.FirstOrDefault(c =>
                c.IsActive && 
                c.Configuration.Type == CollimatorType &&
                c.Configuration.Energy == Energy
            )?.Configuration;
        }

        private void OnCollimatorChanged()
        {
            Energy = null;

            Ssd = (CollimatorType == TargetType.TargetType_30mm_SSD_7_Fields) ? SsdType.SsdType30mm : SsdType.SsdType50mm;
        }

        private void OnQualityCheckFinished()
        {
            // Select latest (topmost) qc sample:
            SelectedQcSample = QcModel.Items.FirstOrDefault();
        }

        private void OnApproveClicked()
        {
            if (SelectedQcSample == null)
                return;

            try
            {
                DialogService.ApprovalDialog(new QcSampleApprovalAction(QcRepository, SelectedQcSample));
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync(ex.Message, LogRecordSeverity.Error, LogRecordType.System);
            }

            NotifySelectedSampleChanged();
        }

        private void NotifySelectedSampleChanged()
        {
            EventAggregator.GetEvent<OnQcSampleSelectionChanged>().Publish(
                new OnQcSampleSelectionChangedEventArgs
                {
                    SelectedSample = SelectedQcSample
                });
        }

        private void FetchQcSamples()
        {
            RetryCurrentTaskCommand = new DelegateCommand(() =>
            {
                CurrentQcTask = new ObservableTask(FetchQcSamplesAsync(), StringConstants.QualityCheck.QcDataLoadError);
            });
            RetryCurrentTaskCommand.Execute();
        }

        private async Task FetchQcSamplesAsync()
        {
            try
            {
                if (!Energy.HasValue)
                {
                    QcModel.Clear();
                    SelectedQcSample = null;
                    return;
                }

                var fetchedItems = await QcListService.FetchSampleReportListAsync(GetCollimatorConfiguration());
                SelectedQcSample = fetchedItems.FirstOrDefault();
            }
            catch(Exception ex)
            {
                _ = LogWriter.LogAsync($"{StringConstants.QualityCheck.QcDataLoadError} {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }

        private void PrepareSelectedSample()
        {
            RetryCurrentTaskCommand = new DelegateCommand(() =>
            {
                CurrentQcTask = new ObservableTask(
                    PrepareSelectedSampleAsync(),
                    StringConstants.QualityCheck.UpdateDeviationValuesErrorMessage);
            });
            RetryCurrentTaskCommand.Execute();
        }

        private async Task PrepareSelectedSampleAsync()
        {
            try
            {
                if (SelectedQcSample != null)
                {
                    var sample = await QcListService.FetchQcSampleDataAsync(SelectedQcSample);

                    var referencedSample = QcModel.ReferencedSample;
                    if (referencedSample != SelectedQcSample)
                    {
                        sample.ApplyReference(referencedSample);
                    }
                    else
                    {
                        sample.ApplyReference(null);
                    }
                }
                EventAggregator.GetEvent<OnQcSampleSelectionChanged>().Publish(new OnQcSampleSelectionChangedEventArgs { SelectedSample = SelectedQcSample });
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync(
                    $"{StringConstants.QualityCheck.UpdateDeviationValuesErrorMessage} {ex.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }
        #endregion Private methods
    }
}
