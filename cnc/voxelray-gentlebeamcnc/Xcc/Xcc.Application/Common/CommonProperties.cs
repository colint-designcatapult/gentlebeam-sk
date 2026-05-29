using Prism.Events;
using Prism.Mvvm;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.Common
{
    public class CommonProperties : BindableBase
    {
        public CommonProperties(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnSystemTelemetryChanged, ThreadOption.UIThread);
        }

        private bool _isInSafeState = true;

        /// <summary>
        /// In any GCB state which is NOT in 'Emission cycle' (HVSetup, Ready, Launching, Emission)
        /// </summary>
        public bool IsInSafeState
        {
            get => _isInSafeState;
            set => SetProperty(ref _isInSafeState, value);
        }

        #region Private methods
        private void OnSystemTelemetryChanged(ISystemTelemetry? telemetry)
        {
            if (telemetry is null)
                return;

            IsInSafeState =
                telemetry.ControlBoardState != Xcc.Core.Enums.GcbStateNew.HVSetup
             && telemetry.ControlBoardState != Xcc.Core.Enums.GcbStateNew.Ready
             && telemetry.ControlBoardState != Xcc.Core.Enums.GcbStateNew.Launching
             && telemetry.ControlBoardState != Xcc.Core.Enums.GcbStateNew.Emission;
        }
        #endregion
    }
}
