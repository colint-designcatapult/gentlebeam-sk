using System;
using System.Collections.Generic;
using System.Linq;

namespace Xcc.Core.Domain.GryphonBoard
{
    public class InterlockState
    {
        public GcbInterlockFlags Interlock { get; }
        public bool Expected { get; }
        public bool Actual { get; }
        public InterlockState(GcbInterlockFlags interlock, bool expected, bool actual)
        {
            Interlock = interlock;
            Expected = expected;
            Actual = actual;
        }
        public override string ToString()
        {
            return $"Interlock: {Interlock} Expected: {Expected} Actual: {Actual}";
        }
    }

    public class InterlockFaultEntry : FaultEntry
    {
        public ICollection<InterlockState> FailedInterlocks { get; }

        public InterlockFaultEntry(ICollection<InterlockState> failedInterlocks) 
        { 
            FailedInterlocks = failedInterlocks; 
        }

        public override string ToString()
        {
            return base.ToString()
                + string.Join(Environment.NewLine, FailedInterlocks.Select(x => x.ToString()));
        }
    }
}
