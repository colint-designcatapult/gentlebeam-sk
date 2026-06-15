using System;
using System.Collections.Generic;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.Common;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels
{
    public class SitesViewModel : RegionViewModelBase
    {
        #region Contructors
        public SitesViewModel() : base(null) { }

        public SitesViewModel(
            ITreatmentInfoStore treatmentInfoStore,
            FieldModel fieldModel,
            IRegionManager regionManager,
            IDialogService dialogService,
            ILogRepository logWriter,
            IEventAggregator eventAggregator,
            IPopUpService popUpService) : base(regionManager, eventAggregator, dialogService)
        {

            FieldModel = fieldModel;

            TreatmentInfo = treatmentInfoStore;
            TreatmentInfo.DiagnosisChanged += OnDiagnosisChanged;
        }
        #endregion Constructors


        #region Read-only properties
        public ITreatmentInfoStore TreatmentInfo { get; }
        public FieldModel FieldModel { get; }
        #endregion Read-only properties


        #region Properties
        private IDiagnosisForm? _siteToEdit;
        public IDiagnosisForm? SiteToEdit
        {
            get => _siteToEdit;
            set
            {
                if (SetProperty(ref _siteToEdit, value))
                {
                    if (_siteToEdit is not null)
                    {
                        _siteToEdit.IsValidChanged += (s, e) => CommandsCanExecuteChanged();
                        _siteToEdit.IsModifiedChanged += (s, e) => CommandsCanExecuteChanged();
                        _siteToEdit.PathologyChanged += OnPathologyChanged;

                        UpdateAvailableSubcells(SiteToEdit?.Pathology);
                    }

                    CommandsCanExecuteChanged();
                }
            }
        }

        private bool _isSubcellRequired;
        public bool IsSubcellRequired
        {
            get { return _isSubcellRequired; }
            set 
            { 
                if (SetProperty(ref _isSubcellRequired, value))
                {
                    if (SiteToEdit != null)
                    {
                        SiteToEdit.IsSubcellRequired = _isSubcellRequired;
                    }
                }
            }
        }

        private IEnumerable<Celltype> _availableSubcells;
        public IEnumerable<Celltype> AvailableSubcells
        {
            get { return _availableSubcells; }
            set 
            {
                SetProperty(ref _availableSubcells, value);
            }
        }

        IDiagnosis PreviousDiagnosis { get; set; }
        #endregion Properties


        #region Commands
        private DelegateCommand? _deleteSiteCommand;
        public DelegateCommand DeleteSiteCommand => _deleteSiteCommand ??= new DelegateCommand(
        () =>
        {
            // TODO: to be implemented later, when we'll have requirements for this
        },
        canExecuteMethod: () => false); 


        private DelegateCommand? _acceptSiteCommand;
        public DelegateCommand AcceptSiteCommand => _acceptSiteCommand ??= new DelegateCommand(
            executeMethod: () =>
            {
                FieldModel.SaveField(SiteToEdit, () =>
                { 
                    SiteToEdit = null;
                });
            },
            canExecuteMethod: () => SiteToEdit is not null &&
                                    SiteToEdit.IsModified &&
                                    SiteToEdit.IsValid);

        
        private DelegateCommand? _editCommand;
        public DelegateCommand EditSiteCommand => _editCommand ??= new DelegateCommand(
            () =>
            {
                PreviousDiagnosis = TreatmentInfo.Diagnosis;
                SiteToEdit = new DiagnosisForm(TreatmentInfo.Diagnosis);
            },
            canExecuteMethod: () => TreatmentInfo.Diagnosis != null &&
                                    !TreatmentInfo.Diagnosis.Archived);


        private DelegateCommand? _archiveCommand;
        public DelegateCommand ArchiveCommand => _archiveCommand ??= new DelegateCommand(
            () =>
            {
                var confirmation = DialogService!.Confirmation(
                    Application.Common.StringConstants.EMR.FieldListTitle,
                    Application.Common.StringConstants.EMR.ArchiveFieldConfirmation);

                if (!confirmation)
                    return;
                
                IDiagnosisForm field = new DiagnosisForm(TreatmentInfo.Diagnosis)
                {
                    Archived = true
                };

                FieldModel.SaveField(field, 
                    () => TreatmentInfo.Diagnosis = field);
            },
            canExecuteMethod: () => TreatmentInfo.Diagnosis != null &&
                                    !TreatmentInfo.Diagnosis.Archived);


        private DelegateCommand? _newSiteCommand;
        public DelegateCommand NewSiteCommand => _newSiteCommand ??= new DelegateCommand(
            () =>
            {
                if (TreatmentInfo.Patient == null)
                    throw new InvalidOperationException("Cannot create a field: there is no patient selected");

                PreviousDiagnosis = TreatmentInfo.Diagnosis;
                TreatmentInfo.Diagnosis = null;

                SiteToEdit = new DiagnosisForm() 
                { 
                    Id = BaseEntry.NEW_ENTRY_ID,
                    PatientId = TreatmentInfo.Patient.Id 
                };
            },
            canExecuteMethod: () => true);


        private DelegateCommand? _cancelSiteCommand;
        public DelegateCommand CancelSiteCommand => _cancelSiteCommand ??= new DelegateCommand(
            () =>
            {
                SiteToEdit = null;
                TreatmentInfo.Diagnosis = PreviousDiagnosis;
            },
            canExecuteMethod: () => true);
        #endregion Commands


        #region Private methods
        private void UpdateAvailableSubcells(Pathology? pathology)
        {
            var availableSubcells = new List<Celltype>();
            switch (pathology)
            {
                case null:
                    availableSubcells = null;
                    break;

                case Pathology.Bcc:
                    availableSubcells = new List<Celltype>
                    {
                        Celltype.Aberrant,
                        Celltype.Adenoid,
                        Celltype.AtypicalBasaloidProliferation,
                        Celltype.BasosquamousMetatypical,
                        Celltype.AdnexalDifferentiation,
                        Celltype.SquamousDifferentiation,
                        Celltype.ClearRing,
                        Celltype.CysticCellCarcinoma,
                        Celltype.FibroepitheliomaOfPinkus,
                        Celltype.Infiltrative,
                        Celltype.Keratotic,
                        Celltype.MicroNodular,
                        Celltype.MixedPattern,
                        Celltype.MorphoeicSclerosingFibrosing,
                        Celltype.NodularClassicBasalCell,
                        Celltype.Nodulocystic,
                        Celltype.Pigmented,
                        Celltype.Pleomorphic,
                        Celltype.Polypoid,
                        Celltype.PoreLike,
                        Celltype.RodentUlcerJacobiUlcer,
                        Celltype.SuperficialMulticentric,
                        Celltype.Other
                    };
                    break;
                case Pathology.Scc:
                    availableSubcells = new List<Celltype>
                    {
                        Celltype.Acantholytic,
                        Celltype.AdenoidPseudoglandular,
                        Celltype.AtypicalSquamousProliferation,
                        Celltype.Basaloid,
                        Celltype.ClearCell,
                        Celltype.Erythroplasia,
                        Celltype.Intraepidermal,
                        Celltype.Invasive,
                        Celltype.Keratoacanthoma,
                        Celltype.LargeCellKeratinizing,
                        Celltype.LargeCellNonKeratinizing,
                        Celltype.Metaplasia,
                        Celltype.MixedPattern,
                        Celltype.ModeratelyDifferentiated,
                        Celltype.PoorlyDifferentiated,
                        Celltype.PapillaryCarcinoma,
                        Celltype.SignetRing,
                        Celltype.SmallCellKeratinizing,
                        Celltype.Superficial,
                        Celltype.SpindleCell,
                        Celltype.Verrucous,
                        Celltype.WellDifferentiated,
                        Celltype.SuperficiallyInvasive,
                        Celltype.Other
                    };
                    break;
                default:
                    availableSubcells = null;
                    break;
            }

            // to prevent premature validation
            IsSubcellRequired = availableSubcells != null;

            AvailableSubcells = availableSubcells;
        }

        private void OnDiagnosisChanged(object? sender, IDiagnosis diagnosis)
        {
            SiteToEdit = null;
            CommandsCanExecuteChanged();
        }

        private void OnPathologyChanged(object sender, Pathology? pathology)
        {
            UpdateAvailableSubcells(pathology);
        }

        private void CommandsCanExecuteChanged()
        {
            AcceptSiteCommand.RaiseCanExecuteChanged();
            EditSiteCommand.RaiseCanExecuteChanged();
            DeleteSiteCommand.RaiseCanExecuteChanged();
            ArchiveCommand.RaiseCanExecuteChanged();
        }
        #endregion
    }
}
