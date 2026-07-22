using Heracles.Application.Domain.DataManagement.System.Collimators;
using System;
using System.Linq;

namespace Heracles.Application.AppLayer.Collimators;

public enum ApplicatorReadiness
{
    Ready,
    NoApplicator,
    UnknownApplicator,
    IncorrectApplicator,
}

public static class ApplicatorReadinessEvaluator
{
    public static ApplicatorReadiness Evaluate(
        ICollimatorModel collimatorModel,
        ICollimatorConfiguration? plannedApplicator)
    {
        var attachedApplicator = collimatorModel.ActiveCollimator;
        if (attachedApplicator is null)
            return ApplicatorReadiness.NoApplicator;

        var registeredApplicator = collimatorModel.Collimators?
            .FirstOrDefault(applicator => string.Equals(
                applicator.Serial,
                attachedApplicator.Serial,
                StringComparison.Ordinal));
        if (registeredApplicator?.Configuration is null)
            return ApplicatorReadiness.UnknownApplicator;

        return plannedApplicator is not null && !registeredApplicator.Configuration.IsSame(plannedApplicator)
            ? ApplicatorReadiness.IncorrectApplicator
            : ApplicatorReadiness.Ready;
    }
}
