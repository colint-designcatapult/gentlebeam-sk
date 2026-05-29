using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public class GcbFaults
    {
        private readonly int _faultFlags;
        public GCBFaultDetails FaultDetails { get; } = GCBFaultDetails.Reserved;

        public GcbFaults(int faultFlags)
        {
            _faultFlags = faultFlags;
        }

        public GcbFaults(GCBFaultBit faultBit, GCBFaultDetails details)
        {
            _faultFlags = 1 << (int)faultBit;
            FaultDetails = details;
        }

        public bool CheckFault(GCBFaultBit faultBit)
        {
            return (_faultFlags & 1 << (int)faultBit) != 0;
        }

        public IEnumerable<GCBFaultBit> GetFaults()
        {
            return Enum.GetValues<GCBFaultBit>().Where(CheckFault);
        }
    }
}
