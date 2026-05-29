using System;

namespace Xcc.Core.Domain.DataManagement.Common
{    
    /// <summary>
    /// Deprecated. Use from Empyrean.Common.Core.Domain.DataManagement.Common
    /// </summary>
    [Obsolete]
    public abstract class BaseEntry : IEntry
    {
        public const long NEW_ENTRY_ID = -1L;

        public long Id { get; set; } = NEW_ENTRY_ID;

        public static bool IsBlankId(long id)
        {
            return id == NEW_ENTRY_ID;
        }

        public static bool IsBlankEntry(IEntry entry)
        {
            return IsBlankId(entry.Id);
        }

        public static bool IsNullOrBlankEntry(IEntry? entry)
        {
            return entry == null || IsBlankEntry(entry);
        }
    }

}
