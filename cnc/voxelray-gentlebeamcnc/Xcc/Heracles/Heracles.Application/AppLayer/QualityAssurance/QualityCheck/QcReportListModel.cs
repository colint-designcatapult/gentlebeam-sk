using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xcc.Core.Services;

namespace Heracles.Application.AppLayer.QualityAssurance.QualityCheck
{
    public interface IQcReportListModel : INotifyPropertyChanged
    {
        long CurrentCollimatorConfigurationId { get; }
        ObservableCollection<QcSampleBindable> Items { get; }
        QcSampleBindable? ReferencedSample { get; }

        void Clear();
        void AddNewSample(QcSampleBindable sample);
        void SetList(ICollimatorConfiguration configuration, IEnumerable<QcSampleBindable> bindableSamples);
        IQcSample SetAsReferenced(QcSampleBindable sampleToMakeReference);
    }


    public class QcReportListModel(
        IDispatcherService dispatcherService) : BindableBase, IQcReportListModel
    {
        #region properties
        /// <summary>
        /// The configuration corresponding to the current Items list
        /// </summary>
        private ICollimatorConfiguration _qcSampleListConfiguration = null;
        public long CurrentCollimatorConfigurationId => _qcSampleListConfiguration?.Id ?? BaseEntry.NewEntryId;

        private ObservableCollection<QcSampleBindable> _qcSampleList = [];
        public ObservableCollection<QcSampleBindable> Items
        {
            get => _qcSampleList;
            private set => SetProperty(ref _qcSampleList, value);
        }

        private QcSampleBindable? _referenceSample;
        public QcSampleBindable? ReferencedSample { 
            get => _referenceSample;
            private set => SetProperty(ref _referenceSample, value);
        }
        #endregion properties

        #region public methods     
        public void Clear()
        {
            dispatcherService.Invoke(() =>
            {
                ReferencedSample = null;
                Items.Clear();
            });
        }

        public void AddNewSample(QcSampleBindable sample)
        {
            if (sample.CollimatorConfigurationId != CurrentCollimatorConfigurationId)
            {
                throw new ArgumentException("QcReportListModel: error on adding new sample - sample belongs to a different configuration");
            }
            // We insert it at the beginning of the list, as it should be sorted in reverse
            dispatcherService.Invoke(() =>
            {
                var reference = ReferencedSample;
                if (reference != null)
                {
                    sample.ApplyReference(reference);
                }
                Items.Insert(0, sample);
            });
        }

        public void SetList(ICollimatorConfiguration configuration, IEnumerable<QcSampleBindable> bindableSamples)
        {
            dispatcherService.Invoke(() =>
            {
                _qcSampleListConfiguration = configuration;
                Items = new(bindableSamples);
                ReferencedSample = Items.FirstOrDefault(x => x.Referenced);
            });
        }

        public IQcSample SetAsReferenced(QcSampleBindable sampleToMakeReferenced)
        {
            if (!Items.Contains(sampleToMakeReferenced))
            {
                throw new ArgumentException("Set as Referenced Error: proposed sample is not in the list");
            }
            var prevReferenced = ReferencedSample;
            if (prevReferenced != null && prevReferenced != sampleToMakeReferenced)
            {
                prevReferenced.Referenced = false;
            }
            sampleToMakeReferenced.Referenced = true;

            return ReferencedSample = sampleToMakeReferenced;
        }

        #endregion public methods
    }
}
