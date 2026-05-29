using System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Models.RDBMS;

namespace Xcc.Application.Models.RDBMS
{
    public class WarmUp : IWarmUp
    {
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public WarmupType Type { get; set; }
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
     
        /// <summary>
        /// [ma]
        /// </summary>
        public double HeaterCurrent { get; set; }
        public long HeadId { get; set; }
    }
}