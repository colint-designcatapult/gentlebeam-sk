using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Core.Models.RDBMS;
using Prism.Events;
using System;

namespace Heracles.Application.AppLayer.QualityAssurance.QualityCheck.Events
{
    public class OnQcSampleSelectionChangedEventArgs : EventArgs
    {
        public IQcSampleHeader SelectedSample;
    }
    public class OnQcSampleSelectionChanged : PubSubEvent<OnQcSampleSelectionChangedEventArgs>
    {
    }
}
