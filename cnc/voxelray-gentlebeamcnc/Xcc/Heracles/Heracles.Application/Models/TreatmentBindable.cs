using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Heracles.Application.Models
{
    public class TreatmentBindable : BindableBase, ITreatmentBindable
    {
        public long Id { get; set; }
        public int Fraction { get; set; }
        public DateTime CreationDate { get ; set ; }
        public double LesionDepth { get; set; }
        public Energy Energy { get; set; }
        public double DailyDose { get; set; }
        public double CumulativeDose { get ; set ; }
        public string? PerformedBy { get ; set ; }

        public bool IsComplete { get; } = false;

        private ObservableCollection<IActualTreatmentField> _actualTreatmentFields = new();
        public ObservableCollection<IActualTreatmentField> ActualTreatmentFields
        {
            get => _actualTreatmentFields;
            private set => SetProperty(ref _actualTreatmentFields, value);
        }

        public TreatmentBindable(
            ITreatment treatment, 
            Energy energy, 
            Xcc.Core.Domain.DataManagement.Common.Users.IUser? user = null)
        {
            Id = treatment.Id;
            Fraction = treatment.Fraction;
            CreationDate = treatment.CreationDate;
            LesionDepth = treatment.LesionDepth;
            Energy = energy;
            DailyDose = treatment.DailyDose;
            CumulativeDose = treatment.CumulativeDose;
            IsComplete = (treatment.ActualTreatmentFields is not null) && treatment.IsComplete();

            if (user != null)
            {
                PerformedBy = $"{user.FirstName} {user.LastName}";
            }
            if (treatment.ActualTreatmentFields is not null)
            {
                ActualTreatmentFields = new ObservableCollection<IActualTreatmentField>(
                    treatment.ActualTreatmentFields);
            }
        }
    }
}
