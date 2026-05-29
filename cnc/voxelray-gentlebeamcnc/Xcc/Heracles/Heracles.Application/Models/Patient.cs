using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System;
using System.ComponentModel.DataAnnotations;

using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Models;

namespace Heracles.Application.Models
{
    public interface IValidatedPatient : IPatient, IPropagateIsValid
    {
    }

    public class Patient : DirtyFlaggedBindableBase, IValidatedPatient
    {
        private string _firstName;
        [Required]
        public string FirstName
        {
            get => _firstName;
            set
            {
                SetProperty(ref _firstName, value);
                Validate(value);
            }
        }

        private string _lastName;
        [Required]
        public string LastName
        {
            get => _lastName;
            set
            {
                SetProperty(ref _lastName, value);
                Validate(value);
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private string _city;
        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        private string _state;
        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        private string _country;
        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        private string _phone;
        [Phone]
        public string Phone
        {
            get => _phone;
            set
            {
                SetProperty(ref _phone, value == string.Empty ? null : value);
                Validate(_phone);
            }
        }

        private string _email;
        [EmailAddress]
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value == string.Empty ? null : value);
                Validate(_email);
            }
        }

        private DateOnly? _dob;
        [DateOfBirth]
        public DateOnly? DOB
        {
            get => _dob;
            set
            {
                SetProperty(ref _dob, value);
                Validate(value);
            }
        }

        private Sex? _sex;
        [Required]
        public Sex? Sex
        {
            get => _sex;
            set
            {
                SetProperty(ref _sex, value);
                Validate(value);
            }
        }

        private string _provider;
        public string Provider
        {
            get => _provider;
            set => SetProperty(ref _provider, value);
        }

        private string _pathology;
        public string Pathology
        {
            get => _pathology;
            set => SetProperty(ref _pathology, value);
        }

        private string _picture;
        public string Picture
        {
            get => _picture;
            set => SetProperty(ref _picture, value);
        }

        private DateTime? _lastVisit;

        public DateTime? LastVisit
        {
            get => _lastVisit;
            set => SetProperty(ref _lastVisit, value);
        }


        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; }

        private string _patientId;
        public string PatientId
        {
            get => _patientId;
            set => SetProperty(ref _patientId, value);
        }

        public bool IsLocal { get; set; } = true;
        public string ProviderId { get; set; }

        private string _middleName;
        public string MiddleName
        {
            get => _middleName;
            set => SetProperty(ref _middleName, value);
        }

        private string _zip;
        [DashesAndDigits]
        public string Zip
        {
            get => _zip;
            set
            {
                SetProperty(ref _zip, value);
                Validate(value);
            }
        }

        public string Ethnicity { get; set; }
        public string Race { get; set; }


        private PatientIdType _patientIdType = PatientIdType.Passport;
        public PatientIdType PatientIdType
        {
            get => _patientIdType;
            set => SetProperty(ref _patientIdType, value);
        }


        private string _mrn;
        [Required]
        [DashesAndDigits]
        public string MRN
        {
            get => _mrn;
            set
            {
                SetProperty(ref _mrn, value);
                Validate(value);
            }
        }


        private string _notes;
        public string Notes 
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public PatientStatus Status { get; set; } = PatientStatus.Active;


        private IVisit? _visit;
        public IVisit? Visit
        {
            get => _visit;
            set => SetProperty(ref _visit, value);
        }


        public Patient() { }

        public Patient(IPatient? patient)
        {
            if (patient != null)
            {
                Xcc.Core.Common.GenericExtensions.CopyProperties(patient, this);
            }

            IsModified = false;
        }

    }
}
