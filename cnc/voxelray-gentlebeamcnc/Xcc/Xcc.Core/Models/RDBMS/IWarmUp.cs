using System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;

namespace Xcc.Core.Models.RDBMS
{
    public interface IWarmUp : IEntry
    {
        DateTime CreationDate { get; set; }
        WarmupType Type { get; set; }
        /// <summary>
        /// [ma]
        /// </summary>
        double HeaterCurrent { get; set; }
        long HeadId { get; set; }
    }
}
