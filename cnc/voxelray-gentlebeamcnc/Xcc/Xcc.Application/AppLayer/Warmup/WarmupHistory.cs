using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Empyrean.Common.Infra.Settings;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Models.RDBMS;

namespace Xcc.Application.AppLayer.Warmup
{
    
    public interface IWarmupObservableHistory : IWarmupHistory
    {
        public ObservableCollection<IWarmUp> ObservableWarmupRecords { get; }
    }

    public class WarmupHistory : BindableBase, IWarmupObservableHistory
    {
        public WarmupHistory(
            IWarmUpSettings warmUpSettings,
            ILogRepository logWriter)
        {
            this._warmUpSettings = warmUpSettings;
            this.logWriter = logWriter;
        }

        public ILogRepository logWriter;
        private readonly IWarmUpSettings _warmUpSettings;
        private ObservableCollection<IWarmUp> _warmupRecords = new();
        
        #region public properties
        public IWarmUp LastConditioning { get; private set; } = null!;
        public ObservableCollection<IWarmUp> ObservableWarmupRecords { get => _warmupRecords; private set => SetProperty(ref _warmupRecords, value); }
        public ICollection<IWarmUp> WarmupRecords => _warmupRecords;
        #endregion

        #region public methods
        public void OnNewWarmupEvent(IWarmUp warmUp)
        {
            if (warmUp.Type == WarmupType.Full)
                LastConditioning = warmUp;

            ObservableWarmupRecords.Insert(0, warmUp);

            _ = logWriter.LogAsync($"Warmup ({warmUp.Type.ToString()}) record added to history.", LogRecordSeverity.Info, LogRecordType.System);
        }


        public void SetWarmupHistory(ICollection<IWarmUp> warmupHistory)
        {
            ObservableWarmupRecords = new ObservableCollection<IWarmUp>(warmupHistory.OrderByDescending(w => w.Id));

            LastConditioning = WarmupRecords.FirstOrDefault(w => w.Type == WarmupType.Full)!;
        }

        public bool ConditioningRequired()
        {
            return LastConditioning == null
                ? true
                : DateTime.Now.Subtract(LastConditioning.CreationDate) >= TimeSpan.FromMinutes(_warmUpSettings.ConditioningIntervalMinutes);
        }
        #endregion public methods
    }
}
