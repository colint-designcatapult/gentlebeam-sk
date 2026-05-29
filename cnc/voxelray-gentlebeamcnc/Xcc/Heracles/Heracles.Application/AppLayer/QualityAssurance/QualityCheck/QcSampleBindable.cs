using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Core.Common;
using IQcSampleHeader = Heracles.Application.Domain.DataManagement.System.QualityCheck.IQcSampleHeader;

namespace Heracles.Application.AppLayer.QualityAssurance.QualityCheck
{
    /// <summary>
    /// QCSample to show as a selected QCSample report
    /// </summary>
    public class QcSampleBindable : BaseQaEntry, IQcSample
    {
        private IList<QcReportField> _fields;

        public QcSampleBindable(IQcSampleHeader qcSample, IEnumerable<QcReportField> fields = null)
        {
            qcSample.CopyProperties(this);
            if (fields != null)
            {
                SetFields(fields);
            }
        }

        #region properties
        public long CollimatorConfigurationId { set; get; } = BaseEntry.NewEntryId;
        public long HeadConfigurationId { set; get; } = BaseEntry.NewEntryId;
        public float EmissionCurrent { set; get; }
        public float HeaterCurrent { set; get; }
        public bool Success { get; set; } = false;

        private string _approvedBy = string.Empty;
        public string ApprovedBy { 
            get => _approvedBy;
            set
            {
                if (SetProperty(ref _approvedBy, value))
                    RaisePropertyChanged(nameof(IsApproved));
            }
        }
        public string Notes { get; set; } = string.Empty;

        public bool IsApproved => !string.IsNullOrEmpty(ApprovedBy);

        // We need Referenced as a bindable property, as we update it on reference selection
        private bool _referenced = false;
        public bool Referenced { get => _referenced; set => SetProperty(ref _referenced, value); }

        public IList<QcReportField> Fields { get => _fields; private set => SetProperty(ref _fields, value); }
        #endregion properties


        #region public methods
        public void SetFields(IEnumerable<QcReportField> fields)
        {
            Fields = [.. fields];
        }

        public void ApplyReference(IQcSample? reference)
        {
            Fields = [.. Fields.Select(x =>
            {
                // We apply the reference for the field if there's one, 
                // and reset it if it is missing
                var referenceField = reference?.Fields?.FirstOrDefault(y => x.FieldName == y.FieldName);
                return referenceField != null ? x.ApplyReference(referenceField) : x.NoReference();
            })];
        }

        /// <summary>
        /// Tests if the QcSample has acceptable deviation over all of its fields
        /// </summary>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public bool IsDeviationAcceptable(double threshold)
        {
            return Fields.All(f => f.IsDeviationAcceptable(threshold));
        }
        #endregion public methods
    }
}
