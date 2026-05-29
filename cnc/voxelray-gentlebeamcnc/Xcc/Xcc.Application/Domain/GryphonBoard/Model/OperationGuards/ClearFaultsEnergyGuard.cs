using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.Domain.GryphonBoard.Model.OperationGuards
{
    /// <summary>
    /// Guard to prevent from clearing faults with too high voltage on the board,
    /// as it would cause a consequent warmup fault in auto-warmup triggered by ClearFaults,
    /// so we need to wait until the voltage gets below the threshold
    /// </summary>
    public class ClearFaultsEnergyGuard : BindableBase
    {
        #region Constants
        public readonly float SAFE_WARM_FAULT_KV_THRESHOLD = 5.0f;
        #endregion


        private bool _canClearErrors = false;
        public bool CanClearErrors
        {
            get => _canClearErrors;
            protected set => SetProperty(ref _canClearErrors, value);
        }

        public ClearFaultsEnergyGuard()
        {
        }

        public void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry?.IsFaultState() ?? false)
            {
                CanClearErrors = systemTelemetry.KvFeedback < SAFE_WARM_FAULT_KV_THRESHOLD;
            }
            else
            {
                CanClearErrors = false;
            }
        }
    }
}
