using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Models;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using Xcc.Core.Services;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class QualityCheckPlan(
        IDispatcherService dispatcherService,
        double fieldDuration) : BindableBase
    {
        private ObservableCollection<IQcSampleFieldEntry> _fields = [];
        private double _fieldDuration = fieldDuration;

        public ObservableCollection<IQcSampleFieldEntry> Fields
        {
            get => _fields;
            set
            {
                if (SetProperty(ref _fields, value))
                {
                    if (_fields != null)
                    {
                        _fields.CollectionChanged += (s, e) =>
                        {
                            CalculateTotalDuration();
                        };
                    }

                    CalculateTotalDuration();
                }
            }
        }

        public double FieldDuration => _fieldDuration;

        public double TotalDuration { get; protected set; }

        public bool IsEmpty => (Fields == null) ? true : Fields.Count == 0;

        #region Public methods
        public void ResetEntryCollectionActualTime()
        {
            if (Fields != null)
            {
                foreach (var field in Fields)
                {
                    field.Actual = 0.0f;
                }
            }
        }

        public void ResetEntries()
        {
            dispatcherService.Invoke(() => Fields = new ObservableCollection<IQcSampleFieldEntry>());
        }

        public void SetDuration(double fieldDuration)
        {
            _fieldDuration = fieldDuration;
            foreach(var field in Fields)
            {
                field.Duration = fieldDuration;
            }
        }

        public void SetEnergy(Energy energy)
        {
            foreach (var field in Fields)
            {
                field.Energy = energy;
            }
        }

        public IQcSampleFieldEntry AddField(IQcSampleFieldEntry entry)
        {
            entry.Duration = FieldDuration;
            dispatcherService.Invoke(() =>
            {
                var fields = Fields.ToList();
                fields.Add(entry);

                var orderedFields = fields.Order(new QcSampleFieldOrdering());
                Fields = new ObservableCollection<IQcSampleFieldEntry>(orderedFields);
            });
            return entry;
        }

        public bool ContainsField(TargetType collimatorType, Energy energy, ITreatmentField field)
        {
            return GetField(collimatorType, energy, field) != null;
        }

        public void RemoveField(IQcSampleFieldEntry fieldToRemove)
        {
            dispatcherService.Invoke(() =>
            {
                Fields.Remove(fieldToRemove);
            });
        }

        public void RemoveField(TargetType collimatorType, Energy energy, ITreatmentFieldEntry fieldToRemove)
        {
            IQcSampleFieldEntry qcSampleField = GetField(collimatorType, energy, fieldToRemove);

            if (qcSampleField == null)
                return;

            dispatcherService.Invoke(() =>
            {
                Fields.Remove(qcSampleField);
            });
        }
        #endregion Public methods
        
        
        #region Private methods
        private void CalculateTotalDuration()
        {
            TotalDuration = (Fields?.Count ?? 0.0f) * FieldDuration;
        }

        private IQcSampleFieldEntry GetField(TargetType collimatorType, Energy energy, ITreatmentField field)
        {
            return Fields.FirstOrDefault(f => f.Name == field.Name
                    && f.CollimatorType == collimatorType
                    && f.Energy == energy);
        }
        #endregion Private methods
    }
}
