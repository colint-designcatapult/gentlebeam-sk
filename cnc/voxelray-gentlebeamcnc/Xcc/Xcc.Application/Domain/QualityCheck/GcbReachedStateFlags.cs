using Prism.Events;
using System.Collections.Generic;
using System.Linq;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.QualityCheck
{
    public class GcbReachedStateFlags
    {
        private GcbStateNew? prevState;
        private object _stateSetLock = new object();
        private HashSet<GcbStateNew?> _stateSet = new();

        public GcbReachedStateFlags(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnTelemetryChanged);
        }

        public void ResetAllFlags()
        {
            lock (_stateSetLock)
            {
                _stateSet.Clear();
            }
        }

        public void ResetFlag(GcbStateNew? state)
        {
            lock (_stateSetLock)
            {
                _stateSet.Remove(state);
            }
        }

        public bool CheckFlag(GcbStateNew? state)
        {
            lock (_stateSetLock)
            {
                return _stateSet.Contains(state);
            }
        }

        public bool CheckFlags(ICollection<GcbStateNew?> states)
        {
            lock (_stateSetLock)
            {
                return states.Any(_stateSet.Contains);
            }
        }

        public GcbStateNew? LastState()
        {
            return prevState;
        }


        private void SetFlag(GcbStateNew? state)
        {
            lock (_stateSetLock)
            {
                _stateSet.Add(state);
            }
        }


        private void OnTelemetryChanged(ISystemTelemetry? telemetry)
        {
            GcbStateNew? currentState = telemetry?.ControlBoardState;
            if (prevState != currentState)
            {
                SetFlag(currentState);
                prevState = currentState;
            }
        }
    }
}
