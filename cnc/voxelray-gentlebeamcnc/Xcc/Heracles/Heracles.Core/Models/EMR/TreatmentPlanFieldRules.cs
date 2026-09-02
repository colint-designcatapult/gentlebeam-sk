using System.Collections.Generic;
using Heracles.Core.Enums;

namespace Heracles.Core.Models.EMR;

public static class TreatmentPlanFieldRules
{
    public const TreatmentFieldName RequiredFieldName = TreatmentFieldName.PlusC;
    public const string InvalidShapeMessage = "Treatment plans must contain exactly one treatment field named PlusC.";

    public static bool IsValid(IEnumerable<ITreatmentField>? fields)
    {
        if (fields is null)
        {
            return false;
        }

        using var enumerator = fields.GetEnumerator();
        if (!enumerator.MoveNext() ||
            enumerator.Current is null ||
            enumerator.Current.Name != RequiredFieldName)
        {
            return false;
        }

        return !enumerator.MoveNext();
    }

    public static void EnsureValid(IEnumerable<ITreatmentField>? fields)
    {
        if (!IsValid(fields))
        {
            throw new InvalidOperationException(InvalidShapeMessage);
        }
    }
}
