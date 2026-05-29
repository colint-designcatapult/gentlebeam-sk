using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Models;
using Heracles.Application.Models.QualityCheck;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.External.Models
{
    public interface ISafetyCheckModel : INotifyPropertyChanged
    {
        ISafetyCheck SafetyCheck { get; }
        ObservableCollection<IFieldEntryBase> Fields { get; set; }
        IFieldEntryBase SelectedField { get; set; }
        TargetType CollimatorType { get; }
        float TotalDuration { get; }
        ObservableCollection<ISafetyCheck> SafetyChecks { get; set; }
        ISafetyCheck SelectedSafetyCheck { get; set; }

        Task SaveAsync();
        void SetSafetyCheck(ISafetyCheck safetyCheck);
        void CreateEntryCollection();
        ISafetyCheck CreateBlank();
        Task FetchSafetyCheckListAsync();
    }

    public class SafetyCheckModel(
        IHeraclesExternalSettings heraclesExternalSettings,
        ICollimatorModel collimatorModel,
        ISafetyCheckCommands safetyCheckCommands,
        IAuthorizedUserStore userStore,
        ILogWriter logWriter,
        IDispatcherService dispatcherService) : BindableBase, ISafetyCheckModel
    {
        #region properties
        public int FieldDuration => heraclesExternalSettings.SafetyCheckFieldDuration;
        public float TotalDuration { get; protected set; }
        public TargetType CollimatorType { get; protected set; }

        private ObservableCollection<IFieldEntryBase> _fields;
        public ObservableCollection<IFieldEntryBase> Fields
        {
            get { return _fields; }
            set { 
                if (SetProperty(ref _fields, value))
                {
                    SelectedField = null;

                    CalculateTotalDuration();
                }
            }
        }

        private IFieldEntryBase _selectedField;
        public IFieldEntryBase SelectedField
        {
            get => _selectedField;
            set
            {
                SetProperty(ref _selectedField, value);
            }
        }

        private ISafetyCheck _safetyCheck;
        public ISafetyCheck SafetyCheck
        {
            get => _safetyCheck;
            private set
            {
                SetProperty(ref _safetyCheck, value);
            }
        }

        private ISafetyCheck _selectedSafetyCheck;
        public ISafetyCheck SelectedSafetyCheck
        {
            get => _selectedSafetyCheck;
            set
            {
                SetProperty(ref _selectedSafetyCheck, value);
            }
        }

        private ObservableCollection<ISafetyCheck> _safetyChecks = new();
        public ObservableCollection<ISafetyCheck> SafetyChecks
        {
            get { return _safetyChecks; }
            set { SetProperty(ref _safetyChecks, value); }
        }

        #endregion

        #region public methods
        public ISafetyCheck CreateBlank()
        {
            SafetyCheck = new SafetyCheck
            {
                Duration = FieldDuration,
                PerformedBy = userStore.AuthorizedUser.EmailAddress
            };

            return SafetyCheck;
        }

        public async Task SaveAsync()
        {
            if (SafetyCheck == null)
                throw new ArgumentNullException(nameof(SafetyCheck));

            await SaveAsync(SafetyCheck);
        }                

        public void SetSafetyCheck(ISafetyCheck safetyCheck)
        {
            SafetyCheck = safetyCheck;

            if (SafetyCheck == null)
            {
                Fields.Clear();
                TotalDuration = 0.0f;
            }
            else
            {
                CreateEntryCollection();
            }
        }

        public void CreateEntryCollection()
        {
            var collimatorConfiguration = collimatorModel.ActiveCollimator?.Configuration;
            if (collimatorConfiguration == null)                            
                throw new Exception("Active applicator configuration is undefined");
            
            CollimatorType = collimatorConfiguration.Type;

            // only central cell should be prepared
            var centralCellIndex = TargetTypeConverter.GetCentralCellIndex(collimatorConfiguration.Type);
            var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(collimatorConfiguration.Type);
            var centralFieldName = fieldNameMapping[centralCellIndex];

            var collection = new List<IFieldEntryBase>
            {
               new FieldEntryBase
                {
                    Actual = 0.0f,
                    Current = Convert.ToSingle(CurrentCalculator.CalculateCurrent(collimatorConfiguration.Energy)),
                    Duration = FieldDuration,
                    DwellTime = FieldDuration,
                    Energy = collimatorConfiguration.Energy,
                    Name = centralFieldName,
                    DisplayValue = centralCellIndex               
                }
            };

            dispatcherService.Invoke(() =>
            {
                Fields = new ObservableCollection<IFieldEntryBase>(collection);
            });
        }

        public async Task FetchSafetyCheckListAsync()
        {
            var collection = await safetyCheckCommands.ReadAllAsync();

            SafetyChecks = new ObservableCollection<ISafetyCheck>(collection.OrderByDescending(x => x.Id));
        }
        #endregion

        #region private methods        

        private async Task SaveAsync(ISafetyCheck entry)
        {
            var created = await safetyCheckCommands.CreateAsync(entry);

            _ = logWriter.LogAsync($"SafetyCheck saved: id = {created.Id} by {userStore.AuthorizedUser.EmailAddress}", LogRecordSeverity.Info, LogRecordType.System);

            SafetyChecks.Insert(0, created);

            SelectedSafetyCheck = created;

            var blank = CreateBlank();
            SetSafetyCheck(blank);
        }

        private void CalculateTotalDuration()
        {
            if (SafetyCheck == null)
            {
                TotalDuration = 0.0f;
                return;
            }

            TotalDuration = Fields.Count * SafetyCheck.Duration;
        }
        #endregion
    }
}
