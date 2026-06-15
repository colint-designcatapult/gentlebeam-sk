using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class ActualTreatmentField : BindableBase, IActualTreatmentField
    {
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public DateTime CreationDate { get; set; }

        public long TreatmentId { get; set; }

        private double _actualDuration = 0;
        public double ActualDuration { get => _actualDuration; set => SetProperty(ref _actualDuration, value); }

        private double _energy = 0;        
        public double ActualEnergy { get => _energy; set => SetProperty(ref _energy, value); }

        private double _actualDose = 0;
        public double ActualDose { get => _actualDose; set => SetProperty(ref _actualDose, value); }

        private double _actualCurrent = 0;

        public double ActualCurrent { get => _actualCurrent; set => SetProperty(ref _actualCurrent, value); }

        public int Completed { get; set; } = 0;

        public TreatmentFieldName Name { get; set; }
        public int DisplayValue { get; set; }

        public int ResumePartial { get; set; }

        public virtual ICollection<IEmissionTreatmentField> EmissionTreatmentFields { get; set; } = new List<IEmissionTreatmentField>();

        public ActualTreatmentField(ITreatmentField treatmentField)
        {
            Name = treatmentField.Name;
            ActualCurrent = treatmentField.Current;
        }

        public ActualTreatmentField(IActualTreatmentField entry)
        {
            entry?.CopyProperties(this);
        }

        public ActualTreatmentField()
        {
        }
    }
}
