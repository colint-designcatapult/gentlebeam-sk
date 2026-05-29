using Heracles.Core.Enums;

namespace Heracles.Application.Domain.DataManagement.System.Collimators
{
    public struct ApplicatorParameters(TargetType type, Energy energy)
    {
        public readonly TargetType Type => type;
        public readonly Energy Energy => energy;

        public static ApplicatorParameters? FromValues(TargetType? type, Energy? energy)
        {
            return (type is not null && energy is not null)
                ? new ApplicatorParameters(type.Value, energy.Value)
                : null;
        }
    }
}
