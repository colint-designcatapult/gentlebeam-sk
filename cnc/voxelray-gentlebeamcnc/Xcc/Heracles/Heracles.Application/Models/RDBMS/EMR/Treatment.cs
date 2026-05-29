using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Treatment : ITreatment
    {
        public Treatment()
        {
        }

        public Treatment(
            ITreatment entry,
            IPlan? plan = null,
            ICollection<IActualTreatmentField>? actualTreatmentFields = null)
        {
            entry?.CopyProperties(this);
            if (plan != null)
            {
                Plan = plan;
            }
            if (actualTreatmentFields != null)
            {
                ActualTreatmentFields = actualTreatmentFields;
            }
        }

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public int Fraction { get; set; }

        public DateTime CreationDate { get; set; }

        public long VisitId { get; set; }

        public long PlanId { get; set; }

        public string? PerformedBy { get; set; }

        public double DailyDose { get; set; }

        public double CumulativeDose { get; set; }

        public ICollection<IActualTreatmentField> ActualTreatmentFields { get; set; } = new List<IActualTreatmentField>();

        public IPlan? Plan { get; } = null;

        public double LesionDepth { get; set; }

        public IActualTreatmentField GetField(TreatmentFieldName fieldName)
        {
            var field = ActualTreatmentFields.FirstOrDefault(f => f.Name == fieldName);
            if (field == null)
            {
                // Try to create & add a new field:
                var treatmentField = Plan?.GetField(fieldName);
                if (treatmentField == null) {
                    throw new ArgumentException($"Invalid field name request: name={fieldName}");
                }
                var newField = new ActualTreatmentField(treatmentField) { TreatmentId = Id };
                ActualTreatmentFields.Add(newField);
                return newField;
            }
            else 
            {
                return field;
            }
        }

        public bool IsComplete()
        {
            return ActualTreatmentFields is not null 
                && ActualTreatmentFields.Count == Plan?.TreatmentFields?.Count
                && ActualTreatmentFields.All(x => x.Completed == 1);
        }

        public bool PerformedWithin(TimeSpan timeInterval)
        {
            return CreationDate + timeInterval > DateTime.Now;
        }
    }
}
