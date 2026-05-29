using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Models.EMR;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class OutputFactorsViewModel : BindableBase
    {


        #region Constructors
        public OutputFactorsViewModel()
        {
            Store = new OutputFactorConfigurationStore();
        }

        public OutputFactorsViewModel(
            IOutputFactorConfigurationStore outputFactorConfigurationStore,
            ILogRepository logWriter,
            IDialogService dialogService)
        {
            Store = outputFactorConfigurationStore;
            Store.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IOutputFactorConfigurationStore.CollimatorConfiguration))
                {
                    // Clean the selection
                    SelectedTarget61Cell = null;
                    SelectedTarget13Cell = null;
                }
            };


            Store.IsValidChanged += (s, e) =>
            {
                SaveCommand.RaiseCanExecuteChanged();
            };
            Store.IsModifiedChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();

            LogWriter = logWriter;
            DialogService = dialogService;
        }
        #endregion Constructors


        #region Properties
        public IOutputFactorConfigurationStore Store { get; }
        public ILogRepository LogWriter { get; }
        public IDialogService DialogService { get; }


        private IOutputFactorEntry _selectedReferenceField;
        public IOutputFactorEntry SelectedReferenceField
        {
            get => _selectedReferenceField;
            set
            {
                if (SetProperty(ref _selectedReferenceField, value) && value != null)
                {
                    _selectedReferenceField.Factor = 1.0;
                    // change collimator control item selection:
                    switch (Store.Configuration)
                    {
                        case OutputFactorConfiguration13Cells:
                            SelectedTarget13Cell = TreatmentField.TreatmentFieldCollection13Cells
                                .FirstOrDefault(item => item.Name == value.FieldName);
                            break;
                        case OutputFactorConfiguration61Cells:
                            SelectedTarget61Cell = TreatmentField.TreatmentFieldCollection61Cells
                                .FirstOrDefault(item => item.Name == value.FieldName);
                            break;
                        case OutputFactorConfiguration7Cells:
                            SelectedTarget61Cell = TreatmentField.TreatmentFieldCollection7Cells
                                .FirstOrDefault(item => item.Name == value.FieldName);
                            break;
                        default: // just ignore for now
                            break;
                    }
                }
            }
        }
        #endregion Properties


        #region 61-cell collimator properties
        private ITreatmentField _selectedTarget61Cell;
        public ITreatmentField SelectedTarget61Cell
        {
            get => _selectedTarget61Cell;
            set
            {
                if (SetProperty(ref _selectedTarget61Cell, value) && value is not null)
                {
                    var f1 = (Store.Configuration as OutputFactorConfiguration61Cells).OutputFactors1To30
                       .FirstOrDefault(item => item.FieldName == value.Name);
                    SelectedOutputFactorEntry1To30 = f1;


                    var f2 = (Store.Configuration as OutputFactorConfiguration61Cells).OutputFactors31To61
                        .FirstOrDefault(item => item.FieldName == value.Name);

                    SelectedOutputFactorEntry31To61 = f2;
                }
            }
        }

        private IOutputFactorEntry _selectedOutputFactorEntry1To30;
        public IOutputFactorEntry SelectedOutputFactorEntry1To30
        {
            get => _selectedOutputFactorEntry1To30;
            set
            {
                if (SetProperty(ref _selectedOutputFactorEntry1To30, value) && value is not null)
                {
                    SelectedTarget61Cell = TreatmentField.TreatmentFieldCollection61Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private IOutputFactorEntry _selectedOutputFactorEntry31To61;
        public IOutputFactorEntry SelectedOutputFactorEntry31To61
        {
            get => _selectedOutputFactorEntry31To61;
            set
            {
                if (SetProperty(ref _selectedOutputFactorEntry31To61, value) && value is not null)
                {
                    SelectedTarget61Cell = TreatmentField.TreatmentFieldCollection61Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }
        #endregion 61-cell collimator properties


        #region 13-cell collimator properties
        private ITreatmentField _selectedTarget13Cell;
        public ITreatmentField SelectedTarget13Cell
        {
            get => _selectedTarget13Cell;
            set
            {
                if (SetProperty(ref _selectedTarget13Cell, value) && value is not null)
                {
                    SelectedOutputFactorEntry13Cell = (Store.Configuration as OutputFactorConfiguration13Cells).OutputFactors
                       .FirstOrDefault(item => item.FieldName == value.Name);
                }
            }
        }

        private IOutputFactorEntry _selectedOutputFactorEntry13Cell;
        public IOutputFactorEntry SelectedOutputFactorEntry13Cell
        {
            get => _selectedOutputFactorEntry13Cell;
            set
            {
                if (SetProperty(ref _selectedOutputFactorEntry13Cell, value) && value is not null)
                {
                    SelectedTarget13Cell = TreatmentField.TreatmentFieldCollection13Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        #endregion 13-cell collimator properties


        #region 7-cell collimator properties
        private ITreatmentField _selectedTarget7Cell;
        public ITreatmentField SelectedTarget7Cell
        {
            get => _selectedTarget7Cell;
            set
            {
                if (SetProperty(ref _selectedTarget7Cell, value) && value is not null)
                {
                    SelectedOutputFactorEntry7Cell = (Store.Configuration as OutputFactorConfiguration7Cells).OutputFactors
                       .FirstOrDefault(item => item.FieldName == value.Name);
                }
            }
        }

        private IOutputFactorEntry _selectedOutputFactorEntry7Cell;
        public IOutputFactorEntry SelectedOutputFactorEntry7Cell
        {
            get => _selectedOutputFactorEntry7Cell;
            set
            {
                if (SetProperty(ref _selectedOutputFactorEntry7Cell, value) && value is not null)
                {
                    SelectedTarget7Cell = TreatmentField.TreatmentFieldCollection7Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }
        #endregion 7-cell collimator properties


        #region Observable tasks
        private ObservableTask _currentOutputFactorsTask;
        public ObservableTask CurrentOutputFactorsTask
        {
            get => _currentOutputFactorsTask;
            set => SetProperty(ref _currentOutputFactorsTask, value);
        }

        private DelegateCommand? _retryOutputFactorsCommand;
        public DelegateCommand RetryOutputFactorsCommand
        {
            get => _retryOutputFactorsCommand;
            set => SetProperty(ref _retryOutputFactorsCommand, value);
        }

        private DelegateCommand? _cancelOutputFactorsCommand;
        public DelegateCommand CancelOutputFactorsCommand => _cancelOutputFactorsCommand ??= new DelegateCommand(
            () =>
            {
                CurrentOutputFactorsTask = null;
            });
        #endregion Observable tasks


        #region Commands
        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            SubmitOutputFactors,
            () => Store.IsValid && Store.IsModified);

        private DelegateCommand? _resetCommand;
        public DelegateCommand ResetCommand => _resetCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.Report(
                    StringConstants.Physics.OutputFactorDialogTitle,
                    StringConstants.Physics.OutputFactorResetWarning,
                    Xcc.Core.Enums.ReportType.Confirmation,
                    (result) =>
                    {
                        if (result.Result != ButtonResult.OK) 
                            return;

                        Store.Configuration.Reset();
                        Store.Configuration.AcceptChanges();
                    }
                );
            }).ObservesCanExecute(() => Store.HasValue);
        #endregion Commands


        #region Private methods
        private void SubmitOutputFactors()
        {
            RetryOutputFactorsCommand = new DelegateCommand(() =>
            {
                CurrentOutputFactorsTask = new ObservableTask(
                    SubmitOutputFactorsAsync(), 
                    StringConstants.Physics.OutputFactorsSaveErrorMessage);
            });
            RetryOutputFactorsCommand.Execute();
        }

        private async Task SubmitOutputFactorsAsync()
        {
            try
            {
                if (Store.Configuration.IsIncomplete())
                {
                    DialogService.ReportError(
                        StringConstants.Physics.OutputFactorDialogTitle,
                        StringConstants.Physics.OutputFactorSubmitError);
                    // Temporary solution: reset IsModified flag
                    // to disable Save button until future edits in it preventing repetitions
                    Store.IsModified = false;
                }
                else
                {
                    await Store.SubmitOutputFactorsAsync();
                    DialogService.Report(
                        StringConstants.Common.SettingsDialogTitle,
                        StringConstants.Common.RestartExternalOnSaveNotification,
                        Xcc.Core.Enums.ReportType.Info);
                }
            }
            catch (Exception ex)
            {
                LogWriter.Log(
                    $"{StringConstants.Physics.OutputFactorsSaveErrorMessage} {ex.Message}. {ex.InnerException?.Message}",
                    Xcc.Core.Enums.LogRecordSeverity.Error,
                    Xcc.Core.Enums.LogRecordType.Error);
                throw;
            }
        }
        #endregion Private methods
    }
}
