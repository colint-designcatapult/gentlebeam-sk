using Heracles.Application.Models.Treatment;

using Prism.Commands;
using Prism.Mvvm;

using Xcc.Application.Helpers;

namespace Heracles.Indoor.ViewModels
{
    public class FieldDetailsViewModel(ITreatmentInfoStore treatmentInfoStore) : BindableBase
    {
        #region Read-only properties
        public ITreatmentInfoStore TreatmentInfoStore { get; } = treatmentInfoStore;
        #endregion Read-only properties


        #region Observable task properties
        private ObservableTask? _currentTask;
        public ObservableTask? CurrentTask
        {
            get => _currentTask;
            set => SetProperty(ref _currentTask, value);
        }


        private DelegateCommand? _retryCurrentTaskCommand;
        public DelegateCommand? RetryCurrentTaskCommand
        {
            get => _retryCurrentTaskCommand;
            set => SetProperty(ref _retryCurrentTaskCommand, value);
        }
        #endregion Properties
    }
}
