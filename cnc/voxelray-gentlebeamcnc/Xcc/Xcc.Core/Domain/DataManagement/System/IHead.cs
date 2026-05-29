using System;
using Xcc.Core.Domain.DataManagement.Common;

namespace Xcc.Core.Domain.DataManagement.System
{
    public interface IHead : IEntry
    {
        DateTime CreationDate { get; set; }
        string? Serial { get; set; }
        bool IsActive { get; set; }
    }
}
