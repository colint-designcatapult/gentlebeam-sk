using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Core.Models.RDBMS.EMR
{
    public interface ISeries : INamedEntry
    {
        public DateTime CreationDate { get; set; }
    }
}
