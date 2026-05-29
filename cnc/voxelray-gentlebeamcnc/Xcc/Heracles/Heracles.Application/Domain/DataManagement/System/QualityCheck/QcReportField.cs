using Heracles.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heracles.Application.Domain.DataManagement.System.QualityCheck
{
    public class QcReportField(QcField field, double current, int displayName = 1)
    {
        private readonly QcField _field = field;

        public int DisplayName => displayName;
        public TreatmentFieldName FieldName => _field.FieldName;
        public ICollection<double?> Values => _field.Values;
        public ICollection<double?> Deviations { get; private set; } = null;
        public double Current => current;

        public QcReportField ApplyReference(QcReportField referenceField)
        {
            return new(field, current, displayName) { Deviations = GetDeviations(referenceField) };
        }

        public QcReportField NoReference()
        {
            return new(field, current, displayName);
        }


        public ICollection<double?> GetDeviations(QcReportField referenceField)
        {
            if (Values.Count != referenceField.Values.Count)
            {
                throw new ArgumentException("QcField.GetDeviations error: sample sizes do not match");
            }
            return [.. Values.Zip(referenceField.Values, CalculateDeviation)];
        }

        public static double? CalculateDeviation(double? value, double? reference)
        {
            return (value != null && reference != null && reference != 0.0d)
                ? 100.0 * (value - reference) / reference
                : null;
        }

        public bool IsDeviationAcceptable(double threshold)
        {
            return Deviations != null 
                && Values.Count == Deviations.Count
                && Deviations.All(v => v != null && v.Value < threshold);
        }
    }
}
