using System;
using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public interface IMainBoardState
    {
        GcbStateNew? State { get; }
        GcbOperationalPoint? CurrentEmission { get; }
        GcbSession? Session { get; }
        bool IsPlanStaged { get; }
        ISystemTelemetry? SystemTelemetry { get; }
        // Methods for board state checks
        #region Board command predicates
        bool CanPrepare();
        bool CanStartWarmUp();
        bool CanLoadPlan();
        bool CanBeamOn();
        bool CanStop();
        bool CanClearFaults();
        bool CanClearPlan();
        bool CanResetTimers();
        #endregion Board command predicates
    }

    public interface ISystemTelemetryChanged
    {
        void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry);
    }

    public interface IMainBoardStateManagement : IMainBoardState, ISystemTelemetryChanged
    {
        void SetSession(GcbSession session);
        void SetCurrentEmission(GcbOperationalPoint emission);
    }

    public interface IMainBoardAPI
    {
        void CancelCurrentTask();

        // State changing commands:
        #region Active commands 
        Task Initialize();
        Task Stop();
        Task ClearFaults();
        Task ClearPlan();

        #region Board command sequences
        /// <summary>
        /// Sequence of warmup and emission-load calls.
        /// </summary>
        Task<bool> PrepareEmission(GcbOperationalPoint emission, bool tryKeepPreviousEmission);

        Task<bool> SafeWarmup(WarmupParameters warmupParameters);
        Task BeamOn();

        Task ResumeEmission();
        #endregion Board command sequences
        #endregion Active commands 


        // Commands changing onboard data, but not its operational state:
        #region Board setup commands
        Task ResetTimers();
        #endregion Board setup commands

        // Commands that just query the board's state
        #region Board state queries
        Task<VersionInfo> GetVersionInfo();
        Task<FaultSnapshot> GetFaults();
        Task<GcbOperationalPoint> QueryEmissionFromGCB();
        Task UpdateCurrentEmissionFromGCB();

        #endregion Board state queries

        event EventHandler<GcbActionCompletionEventArgs> GcbActionCompletionEvent;
    }

    public interface IMainBoardModel : IMainBoardStateManagement, IMainBoardAPI
    {
    }

    public class GcbActionCompletionEventArgs : EventArgs
    {
        public GcbActionType ActionType { get; set; }
    }

    public enum GcbActionType
    {
        NewSession,
        StagePlan,
        ReleasePlan,
        StartBeamOn,
        BeamOnCompleted,
        ClearPlan,
        ClearErrors,
        Stop
    }
}
