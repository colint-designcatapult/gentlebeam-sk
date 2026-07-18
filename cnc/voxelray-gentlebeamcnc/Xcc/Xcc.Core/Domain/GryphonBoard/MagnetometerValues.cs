using System;
using Xcc.Core.Helpers;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct MagnetometerValues
    {
        public Vector3 Back { get; } = new();
        public Vector3 Front { get; } = new();

        public MagnetometerValues(ISystemTelemetry telemetry)
        {
            var back = telemetry.Mag1
                ?? throw new InvalidOperationException("Back magnetometer telemetry is unavailable.");
            var front = telemetry.Mag2
                ?? throw new InvalidOperationException("Front magnetometer telemetry is unavailable.");

            Back[0, 0] = back.X;
            Back[1, 0] = back.Y;
            Back[2, 0] = back.Z;
            Front[0, 0] = front.X;
            Front[1, 0] = front.Y;
            Front[2, 0] = front.Z;
        }
    }
}
