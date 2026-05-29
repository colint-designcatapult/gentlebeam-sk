using System;

namespace Xcc.Core.Models
{
    public interface IWarmUpSettings
    {
        /// <summary>
        /// [mA]
        /// </summary>
        float WarmupSetpoint { get; set;  }
        /// <summary>
        /// [mA]
        /// </summary>
        float FilamentSetpoint { get; }
        /// <summary>
        /// [mA]
        /// </summary>
        float ConditioningSetpoint { get; }

        float ConditioningIntervalMinutes { get; }
    }
}
