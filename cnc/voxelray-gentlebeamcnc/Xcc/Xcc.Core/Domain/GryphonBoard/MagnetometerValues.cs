using Xcc.Core.Helpers;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct MagnetometerValues
    {
        public Vector3 Back { get; } = new();
        public Vector3 Front { get; } = new();

        public MagnetometerValues(ISystemTelemetry telemetry)
        {
            for (int dim = 0; dim < 3; dim++)
            {
                Back[dim, 0] = telemetry.Mag1[dim];
                Front[dim, 0] = telemetry.Mag2[dim];
            }
        }
    }
}
