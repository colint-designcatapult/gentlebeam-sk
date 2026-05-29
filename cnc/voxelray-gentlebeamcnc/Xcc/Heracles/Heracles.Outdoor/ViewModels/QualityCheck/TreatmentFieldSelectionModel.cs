using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Prism.Mvvm;
using System.Collections;
using System.Collections.ObjectModel;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class TreatmentFieldSelectionModel : BindableBase
    {
        public TreatmentFieldSelectionModel(QualityCheckPlan qcPlan)
        {
            QcPlan = qcPlan;
        }

        private IList _treatmentFieldListSelection;
        private ITreatmentField _honeycombSelection;
        private TargetType _selectedCollimatorType;
        private ObservableCollection<ITreatmentField> _currentHoneycombItemCollection;
        private IQcSampleFieldEntry _editableTreatmentFieldListEntry;

        #region Properties
        public IList TreatmentFieldListSelection
        {
            get => _treatmentFieldListSelection;
            set
            {
                if (SetProperty(ref _treatmentFieldListSelection, value))
                {
                    if (TreatmentFieldListSelection != null && TreatmentFieldListSelection.Count == 1)
                    {
                        IQcSampleFieldEntry qcSampleFieldEntry = TreatmentFieldListSelection[0] as IQcSampleFieldEntry;

                        EditableTreatmentFieldListEntry = qcSampleFieldEntry;

                        // Try to select corresponding collimator to switch control view according to selection
                        SelectedCollimatorType = qcSampleFieldEntry.CollimatorType;
                    }
                    else
                    {
                        EditableTreatmentFieldListEntry = null;
                    }
                    // To highlight the actual selection in the Honeycomb, update it
                    UpdateHoneycombSelection();
                }
            }
        }

        /// <summary>
        /// Is not null only if a single item is selected in the treatment field list
        /// </summary>
        public IQcSampleFieldEntry EditableTreatmentFieldListEntry
        {
            get => _editableTreatmentFieldListEntry;
            private set => SetProperty(ref _editableTreatmentFieldListEntry, value);
        }

        public ITreatmentField HoneycombSelection
        {
            get => _honeycombSelection;
            set
            {
                if (SetProperty(ref _honeycombSelection, value))
                {
                    // To highlight the actual selection in the list as well, update it
                    UpdateTreatmentFieldListSelection();
                }
            }
        }

        public TargetType SelectedCollimatorType
        {
            get => _selectedCollimatorType;
            set
            {
                if (SetProperty(ref _selectedCollimatorType, value))
                {
                    CurrentHoneycombItemCollection = TreatmentField.GetTreatmentFieldCollection(value);
                    HoneycombSelectDefault();
                }
            }
        }

        private ObservableCollection<ITreatmentField> CurrentHoneycombItemCollection
        {
            get => _currentHoneycombItemCollection;
            set => SetProperty(ref _currentHoneycombItemCollection, value);
        }

        private QualityCheckPlan QcPlan { get; }


        #endregion Properties

        public void SelectField(IQcSampleFieldEntry fieldToSelect)
        {
            if (fieldToSelect is not null)
            {
                TreatmentFieldListSelection = new List<IQcSampleFieldEntry> { fieldToSelect };
            }
            else
            {
                TreatmentFieldListSelection = null;
            }
        }

        private void UpdateTreatmentFieldListSelection()
        {
            if (HoneycombSelection == null)
            {
                // If there was one field selected in the list, and it does not match current collimator, then deselect it.
                // Otherwise, it's just a collimator switch or multiselect which is not supported by Honeycomb now:
                if (EditableTreatmentFieldListEntry != null && EditableTreatmentFieldListEntry.CollimatorType != SelectedCollimatorType)
                {
                    TreatmentFieldListSelection = new List<IQcSampleFieldEntry>();
                }
            }
            else
            {
                // Honeycomb gets a new field selection.
                // Replace any other previos list selection by this one.
                var itemToSelect = QcPlan?.Fields?.FirstOrDefault(item =>
                    item.CollimatorType.Equals(SelectedCollimatorType) &&
                    HoneycombSelection.Name.Equals(item.Name));
                // If previous selection matches the new one, do nothing with it,
                // and only in the opposite case we apply any changes:
                bool sameSelection = EditableTreatmentFieldListEntry?.Equals(itemToSelect) ?? false;
                if (!sameSelection)
                {
                    var newListSelection = new List<IQcSampleFieldEntry>();
                    if (itemToSelect != null)
                    {
                        newListSelection.Add(itemToSelect);
                    }
                    TreatmentFieldListSelection = newListSelection;
                }
            }
        }

        private void UpdateHoneycombSelection()
        {
            if (TreatmentFieldListSelection == null || TreatmentFieldListSelection.Count > 1)
            {
                // Full reset or multiselect which is not supported by Honeycomb control by now
                //HoneycombSelection = null;
                HoneycombSelectDefault();
            }
            else if (EditableTreatmentFieldListEntry != null)
            {
                // Single selection changes from the list
                HoneycombSelection = CurrentHoneycombItemCollection?.FirstOrDefault(
                    item => EditableTreatmentFieldListEntry.Name.Equals(item.Name));
            }
        }

        private void HoneycombSelectDefault()
        {
            if (CurrentHoneycombItemCollection?.Count == 1)
            {
                HoneycombSelection = CurrentHoneycombItemCollection[0];
            }
            else
            {
                HoneycombSelection = null;
            }
        }
    }
}
