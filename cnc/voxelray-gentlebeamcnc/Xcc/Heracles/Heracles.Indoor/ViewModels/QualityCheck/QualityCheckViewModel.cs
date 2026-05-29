using Heracles.Application.AppLayer.QualityAssurance.QualityCheck.Events;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Core.Models.RDBMS;

using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels.QualityCheck
{
    public class QualityCheckViewModel : BindableBase
    {
        #region Constructors
        public QualityCheckViewModel(IEventAggregator eventAggregator, IDialogService dialogService, ILogRepository logWriter)
        {
            EventAggregator = eventAggregator;
            LogWriter = logWriter;
            DialogService = dialogService;
            eventAggregator.GetEvent<OnQcSampleSelectionChanged>().Subscribe(OnQcSampleSelectionChanged);
        }
        #endregion Constructors


        #region Injected Dependencies
        public IEventAggregator EventAggregator { get; }
        public IDialogService DialogService { get; }
        public ILogRepository LogWriter { get; }
        #endregion Injected Dependencies


        #region Properties

        private IQcSampleHeader _selectedQcSample;
        public IQcSampleHeader SelectedQcSample
        {
            get => _selectedQcSample;
            set => SetProperty(ref _selectedQcSample, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy; set
            {
                isBusy = value;
                SetAsReferenceCommand.RaiseCanExecuteChanged();
                ApproveCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion Properties


        #region Commands
        private DelegateCommand? _approveCommand;
        public DelegateCommand ApproveCommand => _approveCommand ??= new DelegateCommand(
            () =>
            {
                IsBusy = true;
                EventAggregator.GetEvent<OnQcSampleApproveClickedEvent>().Publish();
            },
            canExecuteMethod: () => SelectedQcSample?.IsApproved == false && !IsBusy);

        private DelegateCommand? _setAsReferenceCommand;
        public DelegateCommand SetAsReferenceCommand => _setAsReferenceCommand ??= new DelegateCommand(
            () =>
            {
                IsBusy = true;
                EventAggregator.GetEvent<OnSetAsReferenceClickedEvent>().Publish();
            },
            canExecuteMethod: () => SelectedQcSample?.Referenced == false && !IsBusy);
        #endregion Commands


        #region Private methods
        private void OnQcSampleSelectionChanged(OnQcSampleSelectionChangedEventArgs args)
        {
            SelectedQcSample = args.SelectedSample;
            IsBusy = false; 
        }
        #endregion Private methods
    }
}
