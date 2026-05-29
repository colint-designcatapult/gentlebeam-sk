using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Core.Domain.DataManagement.System.QualityAssurance
{
    public interface IQaEntryBase : IEntry
    {
        DateTime CreationDate { get; set; }

        /// <summary>
        /// Duration[sec] of each field
        /// </summary>
        float Duration { get; set; }
        string? PerformedBy { get; set; }
    }
}
