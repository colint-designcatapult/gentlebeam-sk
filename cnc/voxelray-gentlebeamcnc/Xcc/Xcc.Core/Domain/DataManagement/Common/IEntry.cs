using System;

namespace Xcc.Core.Domain.DataManagement.Common
{
    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Core.Domain.DataManagement.Common
    /// </summary>
    [Obsolete]
    public interface IEntry
    {
        long Id { get; set; }
    }

    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Core.Domain.DataManagement.Common
    /// </summary>
    [Obsolete]
    public interface INamedEntry : IEntry
    {
        string Name { get; set; }
    }
}
