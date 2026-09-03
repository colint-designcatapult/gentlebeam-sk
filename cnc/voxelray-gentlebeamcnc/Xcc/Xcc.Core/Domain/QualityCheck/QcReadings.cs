using System;
using System.Collections.Generic;

namespace Xcc.Core.Domain.QualityCheck
{
    public enum QcSessionStatus : uint
    {
        Idle = 0,
        Armed = 1,
        Starting = 2,
        Accumulating = 3,
        Stopping = 4,
        Complete = 5,
        Error = 6,
    }

    public sealed class QcReadings
    {
        public QcReadings(
            uint channel0Accumulation,
            uint channel1Accumulation,
            uint channel0SampleCount,
            uint channel1SampleCount)
        {
            Channel0Accumulation = channel0Accumulation;
            Channel1Accumulation = channel1Accumulation;
            Channel0SampleCount = channel0SampleCount;
            Channel1SampleCount = channel1SampleCount;
            Accumulations = Array.AsReadOnly(new[] { channel0Accumulation, channel1Accumulation });
        }

        public uint Channel0Accumulation { get; }
        public uint Channel1Accumulation { get; }
        public uint Channel0SampleCount { get; }
        public uint Channel1SampleCount { get; }
        public IReadOnlyList<uint> Accumulations { get; }
    }

    public sealed class QcAcquisitionException : Exception
    {
        public QcAcquisitionException(string message) : base(message)
        {
        }
    }
}
