using Heracles.Application.Common;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;

using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Application.Helpers;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Application.Models.Treatment
{
    public class FieldModel : BindableBase
    {

        #region Constructor
        public FieldModel(
            IEmrDiagnosisCommands emrDiagnosisCommands,
            ITreatmentInfoStore treatmentInfoStore,
            ILogRepository logWriter)
        {
            EmrDiagnosisCommands = emrDiagnosisCommands;
            
            LogWriter = logWriter;

            TreatmentInfoStore = treatmentInfoStore;
            TreatmentInfoStore.PatientChanged += OnPatientChanged;
        }
        #endregion Constructor


        #region Read-only properties
        private IEmrDiagnosisCommands EmrDiagnosisCommands { get; }
        private ITreatmentInfoStore TreatmentInfoStore { get; }
        public ILogRepository LogWriter { get; }
        #endregion Read-only properties


        #region Properties
        private ObservableCollection<IDiagnosis> _fields = new();
        public ObservableCollection<IDiagnosis> Fields
        {
            get => _fields;
            set => SetProperty(ref _fields, value);
        }

        private DelegateCommand? _retryFieldTaskCommand;
        public DelegateCommand RetryFieldTaskCommand
        {
            get => _retryFieldTaskCommand;
            set => SetProperty(ref _retryFieldTaskCommand, value);
        }


        private ObservableTask _currentFieldTask;
        public ObservableTask CurrentFieldTask
        {
            get => _currentFieldTask;
            set => SetProperty(ref _currentFieldTask, value);
        }
        #endregion Properties


        #region Private methods
        public void FetchFieldList(Action continueWith = null)
        {
            RetryFieldTaskCommand = new DelegateCommand(() =>
            {
                CurrentFieldTask = new ObservableTask(FetchFieldListAsync(), StringConstants.EMR.FetchFieldsMessage, continueWith);
            });
            RetryFieldTaskCommand.Execute();
        }

        private async Task FetchFieldListAsync()
        {
            try
            {
                var fields = await FetchFieldListAsync(TreatmentInfoStore.Patient.Id);

                if (TreatmentInfoStore.Diagnosis is not null && fields is not null)
                {
                    // Here we need to just set the diagnosis entry to TreatmentInfo
                    TreatmentInfoStore.Diagnosis = fields.FirstOrDefault(x => x.Id == TreatmentInfoStore.Diagnosis.Id);
                }
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"{ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }

        private async Task<ObservableCollection<IDiagnosis>> FetchFieldListAsync(long patientId)
        {
            return Fields = new((await EmrDiagnosisCommands.ReadListAsync(patientId))
                .OrderBy(field => field.Id));
        }


        public void SaveField(IDiagnosisForm fieldToSave, Action continueWith = null)
        {
            RetryFieldTaskCommand = new DelegateCommand(() =>
            {
                CurrentFieldTask = new ObservableTask(SaveFieldAsync(fieldToSave), StringConstants.EMR.SaveFieldMessage, continueWith);
            });
            RetryFieldTaskCommand.Execute();
        }

        private async Task SaveFieldAsync(IDiagnosisForm fieldToSave)
        {
            try
            {
                TreatmentInfoStore.Diagnosis = await SaveAsync(fieldToSave);
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"{ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }

        private async Task<IDiagnosis> SaveAsync(IDiagnosisForm fieldToSave)
        {
            if (fieldToSave is null)
                throw new NullReferenceException("No field to save.");

            if (BaseEntry.IsBlankEntry(fieldToSave))
            {
                var savedValue = await EmrDiagnosisCommands.CreateAsync(fieldToSave);

                Fields.Add(savedValue);

                return savedValue;
            }
            else
            {
                var oldValue = Fields.FirstOrDefault(d => d.Id == fieldToSave.Id) ?? throw new ArgumentException("Failed to update fields: no such field in the list");
                var savedValue = await EmrDiagnosisCommands.UpdateAsync(oldValue, fieldToSave);

                Fields[Fields.IndexOf(oldValue)] = savedValue;

                return savedValue;
            }
        }


        private void OnPatientChanged(object sender, IPatient patient)
        {
            if (patient is null)
            {
                TreatmentInfoStore.Diagnosis = null;
            }
            else
            {
                FetchFieldList();
            }
        }
        #endregion Private methods
    }
}