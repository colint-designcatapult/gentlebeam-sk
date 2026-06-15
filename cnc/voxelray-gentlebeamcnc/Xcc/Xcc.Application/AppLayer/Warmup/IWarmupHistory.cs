using System.Collections.Generic;
using System.ComponentModel;
using Xcc.Core.Models.RDBMS;

namespace Xcc.Application.AppLayer.Warmup
{
    public interface IWarmupHistory : INotifyPropertyChanged
    {
        IWarmUp LastConditioning { get; }
        ICollection<IWarmUp> WarmupRecords { get; }

        void SetWarmupHistory(ICollection<IWarmUp> warmupRecords);
        void OnNewWarmupEvent(IWarmUp newWarmup);
        bool ConditioningRequired();
    }
}
