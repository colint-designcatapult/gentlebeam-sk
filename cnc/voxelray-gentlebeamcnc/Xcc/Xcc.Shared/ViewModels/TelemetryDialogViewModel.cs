using Prism.Events;

using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Shared.ViewModels
{
    public class TelemetryDialogViewModel : DialogViewModelBase
    {
        public TelemetryDialogViewModel() { }

        public TelemetryDialogViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(SystemTelemetryChanged, ThreadOption.UIThread);
        }

        #region Properties
        private string? _systemTelemetry;
        public string? SystemTelemetry { get => _systemTelemetry; set => SetProperty(ref _systemTelemetry, value); }
        #endregion Properties

        private void SystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            SystemTelemetry = systemTelemetry?.ToString();
        }
    }
}
