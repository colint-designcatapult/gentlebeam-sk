using System;
using Prism.Mvvm;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;

namespace Xcc.Application.Domain.QualityAssurance
{
    public class BaseQaEntry : BindableBase, IQaEntryBase
    {
        public long Id { set; get; } = BaseEntry.NEW_ENTRY_ID;
        public string? PerformedBy { set; get; } = null;
        public long PerformedById { set; get; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { set; get; } = DateTime.Now;
        public float Duration { set; get; }
    }
}
