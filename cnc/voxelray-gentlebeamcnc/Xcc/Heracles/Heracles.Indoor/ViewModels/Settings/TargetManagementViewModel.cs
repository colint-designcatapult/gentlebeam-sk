using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Core.Enums;
using Prism.Commands;
using Prism.Mvvm;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Settings
{
    public interface ICollimatorForm : ICollimator, IDirtyFlaggedBindableBase
    {
    }

    public class ConfigurationForm : DirtyFlaggedBindableBase, ICollimatorForm
    {
        private string _serial = "";
        private TargetType? _collimatorType;
        private Energy? _energy;
        private bool _isActive = true;

        private readonly ICollimatorModel _collimatorModel;

        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = Application.Common.StringConstants.SystemSettings.Validation.NoInstalledApplicator)]
        public string Serial
        {
            get => _serial;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _serial, value))
                {
                    Validate(value);
                }
            }
        }

        [Required(ErrorMessage = Application.Common.StringConstants.SystemSettings.Validation.ApplicatorTypeRequired)]
        public TargetType? CollimatorType
        {
            get => _collimatorType;
            set 
            {
                if (SetPropertyWithDirtyFlag(ref _collimatorType, value))
                {
                    SelectMatchingConfiguration();
                    Validate(value);
                    if (value == TargetType.TargetType_QC_Collimator)
                    {
                        Energy = Core.Enums.Energy.Energy_50;
                    }
                    SsdType = (value == TargetType.TargetType_30mm_SSD_7_Fields) 
                        ? Core.Enums.SsdType.SsdType30mm : Core.Enums.SsdType.SsdType50mm;
                }
            }
        }

        private void SelectMatchingConfiguration()
        {
            Configuration =
                (CollimatorType is not null && Energy is not null)
                ? _collimatorModel.FindConfigurationByType(CollimatorType.Value, Energy.Value)
                : null;


            if (Configuration == null && CollimatorType is not null && Energy is not null && SsdType is not null)
            {
                // Create a new blank configuration for this type and energy:
                Configuration = new CollimatorConfiguration { Type = CollimatorType.Value, Energy = Energy.Value, SsdType = SsdType.Value };
            }

            CollimatorConfigurationId = Configuration?.Id ?? BaseEntry.NEW_ENTRY_ID;
        }

        [Required(ErrorMessage = Application.Common.StringConstants.SystemSettings.Validation.ApplicatorEnergyRequired)]
        public Energy? Energy { 
            get => _energy;
            set
            {
                if (SetPropertyWithDirtyFlag(ref _energy, value))
                {
                    SelectMatchingConfiguration();
                    Validate(value);
                }
            }
        }

        private SsdType? _ssdType;

        public SsdType? SsdType
        {
            get { return _ssdType; }
            set { 
                if (SetPropertyWithDirtyFlag(ref _ssdType, value))
                {
                    SelectMatchingConfiguration();
                }
            }
        }

        public bool IsActive { get => _isActive; set => SetPropertyWithDirtyFlag(ref _isActive, value); }
        public long CollimatorConfigurationId { get; set; }
        public long HeadId { get; set; }
        public ICollimatorConfiguration? Configuration { get; set; }

        public ConfigurationForm(ICollimatorModel collimatorModel,
            ICollimator? collimator)
        {
            _collimatorModel = collimatorModel;
            collimator?.CopyProperties(this);
            CollimatorType = collimator?.Configuration?.Type;
            Energy = collimator?.Configuration?.Energy;
            AcceptChanges();
        }
    }

    public class TargetManagementViewModel : BindableBase
    {
        #region Contructors
        public TargetManagementViewModel()
        {
            CollimatorToEdit = null;
        }

        public TargetManagementViewModel(
            ICollimatorModel collimatorModel,
            CollimatorService collimatorService,
            IPopUpService popUpService,
            ILogRepository logWriter)
        {
            CollimatorModel = collimatorModel;
            CollimatorService = collimatorService;
            PopUpService = popUpService;
            LogWriter = logWriter;

            CollimatorModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ICollimatorModel.ActiveCollimator))
                {
                    // Commented out serial update,
                    // as we disable Add button now for existing collimators,
                    // and we don't want the form to change the serial value after we start adding a new one
                    //string serial = CollimatorModel.ActiveCollimator?.Serial ?? string.Empty;
                    //if (CollimatorToEdit is not null && BaseEntry.IsBlankEntry(CollimatorToEdit))
                    //{
                    //    CollimatorToEdit.Serial = serial;
                    //}
                    NewApplicatorCommand.RaiseCanExecuteChanged();
                }
            };
        }

        #endregion Contructors

        #region Properties
        public IEnumerable<TargetType> AvailableTargetTypeValues { get; } = 
            Enum.GetValues<TargetType>().Skip(4);

        private ICollimator? _selectedCollimator;
        public ICollimator? SelectedCollimator 
        { 
            get => _selectedCollimator;
            set
            {
                SetProperty(ref _selectedCollimator, value);
                NewApplicatorCommand.RaiseCanExecuteChanged();
                EditApplicatorCommand.RaiseCanExecuteChanged();
            }
        }

        ICollimatorForm? _collimatorToEdit;
        public ICollimatorForm? CollimatorToEdit
        {
            get => _collimatorToEdit;
            set
            {
                SetProperty(ref _collimatorToEdit, value);
                NewApplicatorCommand.RaiseCanExecuteChanged();
                EditApplicatorCommand.RaiseCanExecuteChanged();
                if (value is not null)
                {
                    value.IsModifiedChanged += (s, e) => SaveCollimatorCommand.RaiseCanExecuteChanged();
                    value.IsValidChanged += (s, e) =>
                    {
                        SaveCollimatorCommand.RaiseCanExecuteChanged();
                    };
                }
                SaveCollimatorCommand.RaiseCanExecuteChanged();
            }
        }
        public ICollimatorModel CollimatorModel { get; }
        public CollimatorService CollimatorService { get; }
        public IPopUpService PopUpService { get; }
        public ILogRepository LogWriter { get; }
        Task CurrentTask { get; set; }
        #endregion Properties

        #region Commands
        private DelegateCommand? _saveCollimatorCommand;
        public DelegateCommand SaveCollimatorCommand => _saveCollimatorCommand ??= new DelegateCommand(
            () =>
            {
                if (CollimatorToEdit != null)
                    CurrentTask = SaveCollimatorAsync(CollimatorToEdit);
            }, 
            () => CollimatorToEdit is not null && CollimatorToEdit.IsValid);

        private async Task SaveCollimatorAsync(ICollimatorForm collimatorToEdit)
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    if (BaseEntry.IsBlankEntry(collimatorToEdit))
                    {
                        SelectedCollimator = await CollimatorService.CreateCollimatorAsync(
                            collimatorToEdit.Serial,
                            collimatorToEdit.Configuration.Type,
                            collimatorToEdit.Configuration.Energy);
                    }
                    else
                    {
                        SelectedCollimator = await CollimatorService.UpdateCollimatorAsync(
                            collimatorToEdit.Serial,
                            collimatorToEdit.Configuration.Type,
                            collimatorToEdit.Configuration.Energy);
                    }
                    PopUpService.ShowMessage(
                            StringConstants.Common.SettingsDialogTitle,
                            StringConstants.Common.RestartExternalOnSaveNotification,
                            Xcc.Core.Enums.ReportType.Info);
                    CollimatorToEdit = null;
                });
            }
            catch (Exception ex)
            {
                _= LogWriter.LogAsync($"Failed to save applicator record: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
            }
        }


        private DelegateCommand? _newApplicatorCommand;
        public DelegateCommand NewApplicatorCommand => _newApplicatorCommand ??= new DelegateCommand(
            () =>
            {
                CollimatorToEdit = new ConfigurationForm(CollimatorModel, new Collimator(CollimatorModel.ActiveCollimator));
            },
            CanAddNewCollimator);

        private bool CanAddNewCollimator()
        {
            return CollimatorToEdit is null
                && CollimatorModel.ActiveCollimator is not null
                && BaseEntry.IsBlankEntry(CollimatorModel.ActiveCollimator);
        }

        private DelegateCommand? _editApplicatorCommand;
        public DelegateCommand EditApplicatorCommand => _editApplicatorCommand ??= new DelegateCommand(
            () =>
            {
                CollimatorToEdit = new ConfigurationForm(CollimatorModel, SelectedCollimator);
            },
            () => 
            SelectedCollimator is not null &&
            CollimatorToEdit is null);

        private DelegateCommand? _cancelEditCommand;
        public DelegateCommand CancelEditCommand => _cancelEditCommand ??= new DelegateCommand(
            () =>
            {
                CollimatorToEdit = null;
            });
        #endregion Commands

        #region Private methods
        #endregion Private methods
    }
}
