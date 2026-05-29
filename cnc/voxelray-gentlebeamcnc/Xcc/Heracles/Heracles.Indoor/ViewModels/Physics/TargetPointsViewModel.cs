using System;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Models.EMR;
using Heracles.Core.Models.RDBMS;
using Prism.Commands;
using Prism.Mvvm;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class TargetPointsViewModel : BindableBase
    {

        #region Constructors
        /// <summary>
        /// Default constructor. Intended for design-time use only
        /// </summary>
        public TargetPointsViewModel()
        {
            Store = new CoilConfigurationStore();
        }

        public TargetPointsViewModel(
            ICoilConfigurationStore coilConfigurationStore,
            IPopUpService popUpService,
            ILogWriter logWriter)
        {
            Store = coilConfigurationStore;
            PopUpService = popUpService;
            LogWriter = logWriter;

            Store.IsModifiedChanged += (s, e) => SaveCommand.RaiseCanExecuteChanged();
            Store.IsValidChanged += (s, e) => 
                {
                    SaveCommand.RaiseCanExecuteChanged();
                };
        }
        #endregion Constructors


        #region Properties
        public ICoilConfigurationStore Store { get; }
        public IPopUpService PopUpService { get; }
        public ILogWriter LogWriter { get; }
        #endregion Properties


        #region 61-cell collimator properties
        private ITreatmentField _selectedTarget61Cell;
        public ITreatmentField SelectedTarget61Cell
        {
            get => _selectedTarget61Cell;
            set
            {
                if(SetProperty(ref _selectedTarget61Cell, value) && value is not null)
                {
                     var f1 = (Store.Configuration as CoilConfiguration61Cell).CoilConfiguration1To30
                        .FirstOrDefault(item => item.FieldName == value.Name);
                    SelectedCoilConfigurationEntry1To30 = f1;


                    var f2 = (Store.Configuration as CoilConfiguration61Cell).CoilConfiguration31To61
                        .FirstOrDefault(item => item.FieldName == value.Name);

                    SelectedCoilConfigurationEntry31To61 = f2;
                }
            }
        }

        private CoilConfigurationForm _selectedCoilConfigurationEntry1To30;
        public CoilConfigurationForm SelectedCoilConfigurationEntry1To30
        {
            get => _selectedCoilConfigurationEntry1To30;
            set
            {
                if(SetProperty(ref _selectedCoilConfigurationEntry1To30, value) && value is not null)
                {
                    SelectedTarget61Cell = TreatmentField.TreatmentFieldCollection61Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selectedCoilConfigurationEntry31To61;
        public CoilConfigurationForm SelectedCoilConfigurationEntry31To61
        {
            get => _selectedCoilConfigurationEntry31To61;
            set
            {
                if (SetProperty(ref _selectedCoilConfigurationEntry31To61, value) && value is not null)
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
                    Selected13CoilConfigurationEntry1To3 = (Store.Configuration as CoilConfiguration13Cell).CoilConfiguration1To3
                       .FirstOrDefault(item => item.FieldName == value.Name);

                    Selected13CoilConfigurationEntry4To6 = (Store.Configuration as CoilConfiguration13Cell).CoilConfiguration4To6
                       .FirstOrDefault(item => item.FieldName == value.Name);

                    Selected13CoilConfigurationEntry7 = (Store.Configuration as CoilConfiguration13Cell).CoilConfiguration7
                       .FirstOrDefault(item => item.FieldName == value.Name);

                    Selected13CoilConfigurationEntry8To10 = (Store.Configuration as CoilConfiguration13Cell).CoilConfiguration8To10
                        .FirstOrDefault(item => item.FieldName == value.Name);
                    
                    Selected13CoilConfigurationEntry11To13 = (Store.Configuration as CoilConfiguration13Cell).CoilConfiguration11To13
                        .FirstOrDefault(item => item.FieldName == value.Name);
                }
            }
        }

        private CoilConfigurationForm _selected13CoilConfigurationEntry1To3;
        public CoilConfigurationForm Selected13CoilConfigurationEntry1To3
        {
            get => _selected13CoilConfigurationEntry1To3;
            set
            {
                if (SetProperty(ref _selected13CoilConfigurationEntry1To3, value) && value is not null)
                {
                    SelectedTarget13Cell = TreatmentField.TreatmentFieldCollection13Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selected13CoilConfigurationEntry4To6;
        public CoilConfigurationForm Selected13CoilConfigurationEntry4To6
        {
            get => _selected13CoilConfigurationEntry4To6;
            set
            {
                if (SetProperty(ref _selected13CoilConfigurationEntry4To6, value) && value is not null)
                {
                    SelectedTarget13Cell = TreatmentField.TreatmentFieldCollection13Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selected13CoilConfigurationEntry7;
        public CoilConfigurationForm Selected13CoilConfigurationEntry7
        {
            get => _selected13CoilConfigurationEntry7;
            set
            {
                if (SetProperty(ref _selected13CoilConfigurationEntry7, value) && value is not null)
                {
                    SelectedTarget13Cell = TreatmentField.TreatmentFieldCollection13Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selected13CoilConfigurationEntry8To10;
        public CoilConfigurationForm Selected13CoilConfigurationEntry8To10
        {
            get => _selected13CoilConfigurationEntry8To10;
            set
            {
                if (SetProperty(ref _selected13CoilConfigurationEntry8To10, value) && value is not null)
                {
                    SelectedTarget13Cell = TreatmentField.TreatmentFieldCollection13Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selected13CoilConfigurationEntry11To13;
        public CoilConfigurationForm Selected13CoilConfigurationEntry11To13
        {
            get => _selected13CoilConfigurationEntry11To13;
            set
            {
                if (SetProperty(ref _selected13CoilConfigurationEntry11To13, value) && value is not null)
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
                    Selected7CoilConfigurationEntry1To3 = (Store.Configuration as CoilConfiguration7Cell).CoilConfiguration1To3
                       .FirstOrDefault(item => item.FieldName == value.Name);

                    Selected7CoilConfigurationEntry4 = (Store.Configuration as CoilConfiguration7Cell).CoilConfiguration4
                       .FirstOrDefault(item => item.FieldName == value.Name);

                    Selected7CoilConfigurationEntry5To7 = (Store.Configuration as CoilConfiguration7Cell).CoilConfiguration5To7
                       .FirstOrDefault(item => item.FieldName == value.Name);
                }
            }
        }

        private CoilConfigurationForm _selected7CoilConfigurationEntry1To3;
        public CoilConfigurationForm Selected7CoilConfigurationEntry1To3
        {
            get => _selected7CoilConfigurationEntry1To3;
            set
            {
                if (SetProperty(ref _selected7CoilConfigurationEntry1To3, value) && value is not null)
                {
                    SelectedTarget7Cell = TreatmentField.TreatmentFieldCollection7Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selected7CoilConfigurationEntry4;
        public CoilConfigurationForm Selected7CoilConfigurationEntry4
        {
            get => _selected7CoilConfigurationEntry4;
            set
            {
                if (SetProperty(ref _selected7CoilConfigurationEntry4, value) && value is not null)
                {
                    SelectedTarget7Cell = TreatmentField.TreatmentFieldCollection7Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }

        private CoilConfigurationForm _selected7CoilConfigurationEntry5To7;
        public CoilConfigurationForm Selected7CoilConfigurationEntry5To7
        {
            get => _selected7CoilConfigurationEntry5To7;
            set
            {
                if (SetProperty(ref _selected7CoilConfigurationEntry5To7, value) && value is not null)
                {
                    SelectedTarget7Cell = TreatmentField.TreatmentFieldCollection7Cells
                        .FirstOrDefault(item => item.Name == value.FieldName);
                }
            }
        }
        #endregion 7-cell collimator properties


        #region Observable tasks
        private ObservableTask _currentTargetPointsTask;
        public ObservableTask CurrentTargetPointsTask
        {
            get => _currentTargetPointsTask;
            set => SetProperty(ref _currentTargetPointsTask, value);
        }

        private DelegateCommand? _retryTargetPointsCommand;
        public DelegateCommand RetryTargetPointsCommand
        {
            get => _retryTargetPointsCommand;
            set => SetProperty(ref _retryTargetPointsCommand, value);
        }

        private DelegateCommand? _cancelTargetPointsCommand;
        public DelegateCommand CancelTargetPointsCommand => _cancelTargetPointsCommand ??= new DelegateCommand(
            () =>
            {
                CurrentTargetPointsTask = null;
            });
        #endregion Observable tasks


        #region Commands
        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            SubmitTargetPointsParameters,
            () => Store is {IsModified: true, IsValid: true});
        #endregion Commands


        private void SubmitTargetPointsParameters()
        {
            RetryTargetPointsCommand = new DelegateCommand(() =>
            {
                CurrentTargetPointsTask = new ObservableTask(
                    SubmitCoilConfigurationAsync(), 
                    StringConstants.Physics.TargetPointsSaveErrorMessage
                    );
            });
            RetryTargetPointsCommand.Execute();
        }

        private async Task SubmitCoilConfigurationAsync()
        {
            try
            {
                await Store.SubmitCollimatorConfigurationAsync();

                PopUpService.ShowMessage(
                    StringConstants.Common.SettingsDialogTitle,
                    StringConstants.Common.RestartExternalOnSaveNotification,
                    Xcc.Core.Enums.ReportType.Info);
            }
            catch (Exception ex)
            {
                LogWriter.Log(
                    $"{StringConstants.Physics.TargetPointsSaveErrorMessage} {ex.Message}. {ex.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.Error);
                throw;
            }
        }
    }
}
