using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Model.Indicators
{
    public interface IGcbIndicators
    {
        WarmUpProgress WarmUpProgress { get; }
        HVSetupProgress HVSetupProgress { get; }
        LaunchingProgress LaunchingProgress { get; }
        BeamOnProgress BeamOnProgress { get; }
    }

    public class GcbIndicators : IGcbIndicators
    {
        public GcbIndicators(
            INotifyWarmupEvent warmupEventSource,
            IMainBoardModel mainBoardModel,
            IEventAggregator eventAggregator,
            ILogWriter logWriter) 
        {
            WarmUpProgress = new WarmUpProgress(warmupEventSource, logWriter);
            HVSetupProgress = new HVSetupProgress(mainBoardModel);
            LaunchingProgress = new LaunchingProgress(mainBoardModel);
            BeamOnProgress = new BeamOnProgress(mainBoardModel);
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnSystemTelemetryChanged);
        }

        private void OnSystemTelemetryChanged(ISystemTelemetry? telemetry)
        {
            WarmUpProgress.OnSystemTelemetryChanged(telemetry);
            HVSetupProgress.OnSystemTelemetryChanged(telemetry);
            LaunchingProgress.OnSystemTelemetryChanged(telemetry);
            BeamOnProgress.OnSystemTelemetryChanged(telemetry);
        }

        public WarmUpProgress WarmUpProgress { get; }
        public HVSetupProgress HVSetupProgress { get; } 
        public LaunchingProgress LaunchingProgress { get; }
        public BeamOnProgress BeamOnProgress { get; }
    }
}
