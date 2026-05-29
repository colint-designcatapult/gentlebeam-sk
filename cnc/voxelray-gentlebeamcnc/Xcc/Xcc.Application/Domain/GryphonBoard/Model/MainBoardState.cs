using System;
using System.Linq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Xcc.Application.Domain.GryphonBoard.Model
{
    public class MainBoardState : IMainBoardStateManagement
    {
        public GcbEmissionPlan CurrentPlan { get; private set; } = null!;
        public ISystemTelemetry? SystemTelemetry { get; private set; } = null!;
        public GcbStateNew? State => SystemTelemetry?.ControlBoardState;
        public bool IsPlanStaged { get; protected set; } = false;
        public GcbSession? Session { get; private set; } = null!;
        public IGCBDataStore GcbDataStore { get; }
        public ILogWriter LogWriter { get; }

        public MainBoardState(IGCBDataStore gcbDataStore, ILogWriter logWriter)
        {
            GcbDataStore = gcbDataStore;
            LogWriter = logWriter;
        }

        public bool CanBeamOn()
        {
            throw new NotImplementedException();
        }

        public bool CanClearFaults()
        {
            throw new NotImplementedException();
        }

        public bool CanClearPlan()
        {
            throw new NotImplementedException();
        }

        public bool CanLoadPlan()
        {
            throw new NotImplementedException();
        }

        public bool CanPrepare()
        {
            throw new NotImplementedException();
        }

        public bool CanResetTimers()
        {
            throw new NotImplementedException();
        }

        public bool CanStartWarmUp()
        {
            throw new NotImplementedException();
        }

        public bool CanStop()
        {
            throw new NotImplementedException();
        }

        #region private methods
        public void SetSession(GcbSession session)
        {
            Session = session;
        }

        public void SetCurrentPlan(GcbEmissionPlan plan)
        {
            CurrentPlan = plan;
        }

        public void OnSystemTelemetryChanged(ISystemTelemetry systemTelemetry)
        {
            var previousState = State;
            SystemTelemetry = systemTelemetry;

            if (systemTelemetry != null)
            {
                GcbDataStore.Interlocks = GcbInterlocks.Create(systemTelemetry.InterlockFlags);
                GcbDataStore.Faults = new GcbFaults(systemTelemetry.FaultFlags);
                if (previousState != systemTelemetry?.ControlBoardState && systemTelemetry?.FaultFlags != 0)
                {
                    var faultList = GcbDataStore.Faults.GetFaults();
                    var faults = (faultList == null) ? "No data" : string.Join('\n', faultList.Select(x => x.ToString()));
                    _ = LogWriter.LogAsync($"GCB went into a fault state: {systemTelemetry?.ControlBoardState}.\nReason: {faults}", LogRecordSeverity.Info, LogRecordType.System);
                }
            }
            else
            {
                GcbDataStore.Interlocks = null;
                GcbDataStore.Faults = null;
            }
            GcbDataStore.SystemTelemetry = SystemTelemetry;

        }

        public GcbOperationalPoint CurrentPoint()
        {
            throw new NotImplementedException();
        }
        #endregion private methods
    }
}
