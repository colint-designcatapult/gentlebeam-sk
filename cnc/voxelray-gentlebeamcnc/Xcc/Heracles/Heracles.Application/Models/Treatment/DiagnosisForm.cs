using Heracles.Application.Common;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System;
using System.ComponentModel.DataAnnotations;

using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.Treatment
{
    public class RequiredIfNotNullAttribute : ValidationAttribute
    {
        private readonly string _dependentPropertyName;

        public RequiredIfNotNullAttribute(string dependentPropertyName)
        {
            _dependentPropertyName = dependentPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var containerType = validationContext.ObjectInstance.GetType();
            var field = containerType.GetProperty(_dependentPropertyName);
            if (field != null)
            {
                var dependentValue = field.GetValue(validationContext.ObjectInstance) as bool?;

                if (dependentValue != null && dependentValue.Value && value == null)
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required");
                }
            }

            return ValidationResult.Success;
        }
    }

    public interface IDiagnosisForm : IDiagnosis, IEmrForm
    {
        bool IsSubcellRequired { get; set; }

        public event EventHandler<Pathology?> PathologyChanged;
    }

    public class DiagnosisForm : EmrForm, IDiagnosisForm
    {
        private long _id = BaseEntry.NEW_ENTRY_ID;
        private string _siteName = String.Empty;
        private string _referring = String.Empty;
        private Pathology? _pathology = null;
        private SiteLocation? _siteLocation = null;
        private IcdCode? _icdCode;
        private Celltype? _subcellOne;
        private Celltype? _subcellTwo;
        private Description? _description;
        private bool _archived;

        public long Id { get => _id; set => SetPropertyWithDirtyFlag(ref _id, value); }

        public bool IsSubcellRequired { get; set; }

        [Required(ErrorMessage = StringConstants.EMR.Validation.FieldNameRequired)]
        public string SiteName
        {
            get => _siteName;
            set
            {
                SetPropertyWithDirtyFlag(ref _siteName, value);
                Validate(value);
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.PathologyRequired)]
        public Pathology? Pathology
        {
            get => _pathology;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _pathology, value))
                {
                    SubcellOne = null;
                    SubcellTwo = null;
                    IcdCode = Core.Constants.IcdCodes.GetCode(SiteLocation, value);

                    PathologyChanged?.Invoke(this, value);                    
                }

                Validate(value);
            }
        }
        public event EventHandler<Pathology?> PathologyChanged;


        [Required(ErrorMessage = StringConstants.EMR.Validation.SiteLocationRequired)]
        public SiteLocation? SiteLocation
        {
            get => _siteLocation;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _siteLocation, value))
                {
                    IcdCode = Core.Constants.IcdCodes.GetCode(value, Pathology);
                }
                Validate(value);
            }
        }

        [RequiredIfNotNull(nameof(IsSubcellRequired), ErrorMessage = StringConstants.EMR.Validation.SubcellRequired)]
        public Celltype? SubcellOne
        {
            get => _subcellOne;
            set
            {
                SetPropertyWithDirtyFlag(ref _subcellOne, value);
                Validate(value);
            }
        }

        [RequiredIfNotNull(nameof(IsSubcellRequired), ErrorMessage = StringConstants.EMR.Validation.SubcellRequired)]
        public Celltype? SubcellTwo
        {
            get => _subcellTwo;
            set
            {
                SetPropertyWithDirtyFlag(ref _subcellTwo, value);
                Validate(value);
            }
        }

        [Required(ErrorMessage = StringConstants.EMR.Validation.FieldDescriptionRequired)]
        public Description? Description
        {
            get => _description;
            set
            {
                SetPropertyWithDirtyFlag(ref _description, value);
                Validate(value);
            }
        }

        public string Referring { get => _referring; set => SetPropertyWithDirtyFlag(ref _referring, value); }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public long PatientId { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public IcdCode? IcdCode
        {
            get => _icdCode;
            set
            {
                SetPropertyWithDirtyFlag(ref _icdCode, value);
                Validate(value); // need to validate to not get any error from a field that sets this value, like Pathology
            }
        }
        public bool Archived { get => _archived; set => SetPropertyWithDirtyFlag(ref _archived, value); }

        public DiagnosisForm(IDiagnosis initialState = null)
        {
            initialState?.CopyProperties(this);
            AcceptChanges();
        }

        private void UpdateSubcellRequired(Pathology? pathology)
        {
            switch (pathology)
            {
                case null:
                case Core.Enums.Pathology.Bcc:
                case Core.Enums.Pathology.Scc:
                    IsSubcellRequired = true;
                    break;
                case Core.Enums.Pathology.SccIs:
                case Core.Enums.Pathology.Keloid:
                case Core.Enums.Pathology.Basosquamous:
                default:
                    IsSubcellRequired = false;
                    break;
            }
        }
    }
}
