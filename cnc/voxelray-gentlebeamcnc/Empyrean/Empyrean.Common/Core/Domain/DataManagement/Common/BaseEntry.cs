namespace Empyrean.Common.Core.Domain.DataManagement.Common
{
    public abstract class BaseEntry(long id) : IEntry
    {
        public BaseEntry() 
            : this(NewEntryId)
        { }

        public const long NewEntryId = -1L;

        public long Id { get; set; } = id;

        public bool IsBlank => IsBlankEntry(this);

        public static bool IsBlankId(long id)
        {
            return id == NewEntryId;
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
