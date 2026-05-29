using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Infra.Persistence.DataAccess.Dummy
{
    public class DummyRootEntryCommands<TClientType, TStorageType> : IAsyncRootEntryCommands<TClientType>
        where TClientType : class, IEntry
        where TStorageType : TClientType, new()
    {
        public virtual Task<ICollection<TClientType>> ReadAllAsync()
        {
            return Task.FromResult((ICollection<TClientType>)Entries.Select(CloneEntry).ToList());
        }

        public virtual Task<TClientType> CreateAsync(TClientType entry)
        {
            TClientType newEntry = CloneEntry(entry);

            newEntry.Id = nextId++;
            Entries.Add(newEntry);
            return Task.FromResult(newEntry);
        }

        public virtual Task<TClientType> ReadAsync(long entryId)
        {
            var entry = Entries.FirstOrDefault(e => e.Id == entryId);
            if (entry is not null)
            {
                return Task.FromResult(CloneEntry(entry));
            }
            else
            {
                throw new DataServiceException("No such data in the DB");
            }
        }


        public virtual Task<TClientType> UpdateAsync(TClientType oldEntry, TClientType newEntry)
        {
            var entry = Entries.FirstOrDefault(e => e.Id == newEntry.Id);
            if (entry is not null)
            {
                TClientType replacementEntry = CloneEntry(newEntry);

                Entries[Entries.IndexOf(entry)] = replacementEntry;
                return Task.FromResult(replacementEntry);
            }
            else
            {
                throw new DataServiceException("No such data in the DB");
            }
        }
        public virtual Task<bool> DeleteAsync(long entryId)
        {
            var entry = Entries.FirstOrDefault(e => e.Id == entryId);
            if (entry is not null)
            {
                return Task.FromResult(Entries.Remove(entry));
            }
            else
            {
                throw new DataServiceException("No such data in the DB");
            }
        }

        public DummyRootEntryCommands()
        {
        }

        protected IList<TClientType> Entries = new List<TClientType>();
        long nextId = 1;

        protected virtual TClientType CloneEntry(TClientType entry)
        {
            TClientType newEntry = new TStorageType();
            entry.CopyProperties(newEntry);

            return newEntry;
        }

        protected virtual TClientType CloneEntryWithoutCollections(TClientType entry)
        {
            TClientType newEntry = new TStorageType();
            var ignoreList = GenericExtensions.GetEnumerablePropertyNames(entry);
            entry.CopyProperties(newEntry, ignoreList: ignoreList);

            return newEntry;
        }
    }
}
