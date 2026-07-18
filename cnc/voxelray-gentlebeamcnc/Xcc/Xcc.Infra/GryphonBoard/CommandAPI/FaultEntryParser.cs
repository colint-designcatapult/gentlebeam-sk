using System;
using System.Collections.Generic;
using Empyrean.Common.Infra.Networking.Udp;
using System.Linq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public static class FaultEntryParser
    {
        public static FaultEntry Parse(byte[] udpPacketData)
        {
            UdpPacket faultPacket = new(udpPacketData);

            int faultId = faultPacket[0]; // primary fault which caused termination. Value is 1 + bit number of the fault in the fault table
            int faultIdSupportingDetail = faultPacket[1];
            int faultEntryState = faultPacket[2]; // state in which fault occured
            int faultTimeValue = faultPacket[3]; // main board runtime at which fault was logged
            float expectedParameter = faultPacket[4];
            int expectedParameterSupportingDetail = faultPacket[5];
            float parameterTolerance = faultPacket[6];
            float measuredParameter = faultPacket[7];
            int measuredParameterSupportingDetail = faultPacket[8];

            if (faultId == (int)SystemFault.InterlockFault)
            {
                // Get expected & measured values as int and interpret them as interlock masks:
                int interlocksExpected = expectedParameterSupportingDetail;
                int interlocksActual = measuredParameterSupportingDetail;
                int mismatchMask = ~(interlocksExpected ^ interlocksActual); // 0's are mismatches
                
                RawInterlockMask expectedInterlockValues = new(interlocksExpected);
                RawInterlockMask actualInterlockValues = new(interlocksActual);
                RawInterlockMask interlockMismatches = new(mismatchMask);

                // Build a list of expectation-actual mismatches:
                var failedInterlocksWithValues =
                    interlockMismatches
                    .GetOpenInterlocks() // we're interested in False values only, as they're mismatches
                    .Select(x => new InterlockState(x, expectedInterlockValues.CheckInterlock(x), actualInterlockValues.CheckInterlock(x)))
                    .ToList();

                return new InterlockFaultEntry(failedInterlocksWithValues)
                {
                    FaultId = faultId,
                    FaultType = (SystemFault)faultId,
                    FaultIdSupportingDetails = (GCBFaultDetails)faultIdSupportingDetail,
                    FaultEntryState = faultEntryState,
                    FaultTimeValue = faultTimeValue,
                    ExpectedParameter = expectedParameter,
                    ExpectedParameterSupportingDetails = expectedParameterSupportingDetail,
                    ParameterTolerance = parameterTolerance,
                    MeasuredParameter = measuredParameter,
                    MeasuredParameterSupportingDetails = measuredParameterSupportingDetail,
                };
            }
            else 
            {
                return new FaultEntry
                {
                    FaultId = faultId,
                    FaultType = (SystemFault)faultId,
                    FaultIdSupportingDetails = (GCBFaultDetails)faultIdSupportingDetail,
                    FaultEntryState = faultEntryState,
                    FaultTimeValue = faultTimeValue,
                    ExpectedParameter = expectedParameter,
                    ExpectedParameterSupportingDetails = expectedParameterSupportingDetail,
                    ParameterTolerance = parameterTolerance,
                    MeasuredParameter = measuredParameter,
                    MeasuredParameterSupportingDetails = measuredParameterSupportingDetail,
                };
            }
        }

        private sealed class RawInterlockMask
        {
            private readonly int _flags;

            internal RawInterlockMask(int flags)
            {
                _flags = flags;
            }

            internal bool CheckInterlock(GcbInterlockFlags interlock) =>
                (_flags & (int)interlock) != 0;

            internal IEnumerable<GcbInterlockFlags> GetOpenInterlocks() =>
                Enum.GetValues<GcbInterlockFlags>().Where(interlock => !CheckInterlock(interlock));
        }
    }
}
