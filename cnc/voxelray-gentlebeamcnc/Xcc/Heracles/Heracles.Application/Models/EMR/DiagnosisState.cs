using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;

using System;

using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.EMR
{
    public class DiagnosisState : DirtyFlaggedBindableBase, IDiagnosisState
    {
        private long _id = BaseEntry.NEW_ENTRY_ID;
        private string _siteName = String.Empty;
        private string _referring = String.Empty;
        private Pathology? _pathology = null;
        private SiteLocation? _siteLocation = null;
        private IcdCode? _icdCode = null;
        private Celltype? _cellType1;
        private Celltype? _cellType2;
        private Description? _description;
        private bool _archived;

        public long Id { get => _id; set => SetPropertyWithDirtyFlag(ref _id, value); }
        public string SiteName { get => _siteName; set => SetPropertyWithDirtyFlag(ref _siteName, value); }
        public string Referring { get => _referring; set => SetPropertyWithDirtyFlag(ref _referring, value); }
        public DateTime CreationDate { get; set; }
        public long PatientId { get; set; }

        public bool Archived
        {
            get => _archived;
            set
            {
                SetPropertyWithDirtyFlag(ref _archived, value);
            }
        }
        public Description? Description
        {
            get => _description;
            set
            {
                SetPropertyWithDirtyFlag(ref _description, value);
            }
        }
        public Celltype? SubcellOne
        {
            get => _cellType1;
            set
            {
                SetPropertyWithDirtyFlag(ref _cellType1, value);
            }
        }
        public Celltype? SubcellTwo
        {
            get => _cellType2;
            set
            {
                SetPropertyWithDirtyFlag(ref _cellType2, value);
            }
        }
        public Pathology? Pathology
        {
            get => _pathology;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _pathology, value))
                {
                    IcdCode = Core.Constants.IcdCodes.GetCode(SiteLocation, value);
                }
            }
        }
        public SiteLocation? SiteLocation
        {
            get => _siteLocation;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _siteLocation, value))
                {
                    IcdCode = Core.Constants.IcdCodes.GetCode(value, Pathology);
                }
            }
        }
        public IcdCode? IcdCode { get => _icdCode; set => SetPropertyWithDirtyFlag(ref _icdCode, value); }
        public DiagnosisState(IDiagnosis initialState = null)
        {
            initialState?.CopyProperties(this);
            AcceptChanges();
        }
    }
}
