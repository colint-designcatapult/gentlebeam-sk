using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Core.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Infra.Persistence.DataAccess.Dummy
{
    public class DummyChildEntryCommands<TClientType, TStorageType> : IAsyncChildEntryCommands<TClientType>
            where TClientType : class, IEntry
            where TStorageType : TClientType, new()
    {
        public virtual Task<ICollection<TClientType>> ReadListAsync(long parentId)
        {
            Entries.TryGetValue(parentId, out IList<TClientType>? entryList);
            if (entryList == null)
            {
                entryList = new List<TClientType>();
            }
            return Task.FromResult((ICollection<TClientType>)entryList.Select(CloneEntry).ToList());
        }

        public virtual Task<TClientType> CreateAsync(TClientType entry)
        {
            long parentId = GetParentId(entry);

            TClientType newEntry = CloneEntry(entry);
            
            newEntry.Id = nextId++;

            if (!Entries.ContainsKey(parentId))
            {
                Entries[parentId] = new List<TClientType>();
            }
            Entries[parentId].Add(newEntry);

            return Task.FromResult((TClientType)newEntry);

        }

        public virtual Task<TClientType> ReadAsync(long entryId)
        {
            foreach (var entryList in Entries.Values)
            {
                TClientType? entry = entryList.FirstOrDefault(v => v.Id.Equals(entryId));
                if (entry != null)
                {
                    return Task.FromResult(CloneEntry(entry));
                }
            }
            throw new DataServiceException($"No such {typeof(TClientType)} in the DB");
        }


        public virtual Task<TClientType> UpdateAsync(TClientType oldEntry, TClientType newEntry)
        {
            long parentId = GetParentId(newEntry);


            foreach (var entryList in Entries.Values)
            {
                TClientType? entry = entryList.FirstOrDefault(v => v.Id.Equals(newEntry.Id));
                if (entry != null)
                {
                    TClientType replacementEntry = CloneEntry(newEntry);
                    // Same parent Id:
                    if (oldEntry is null || parentId == GetParentId(oldEntry))
                    {
                        Task.FromResult(entryList[entryList.IndexOf(entry)] = replacementEntry);
                    }
                    else // Different parents, remove from one and add to another:
                    {
                        entryList.Remove(entry);
                        Entries[GetParentId(oldEntry)].Add(replacementEntry);
                    }
                    return Task.FromResult(replacementEntry);
                }
            }

            throw new DataServiceException($"No such {typeof(TClientType)} field in the DB");
        }

        public virtual Task<bool> DeleteAsync(long entryId)
        {
            foreach (var entryList in Entries.Values)
            {
                TClientType? c = entryList.FirstOrDefault(v => v.Id.Equals(entryId));
                if (c != null)
                {
                    return Task.FromResult(entryList.Remove(c));
                }
            }
            throw new DataServiceException($"No such {typeof(TClientType)} in the DB");
        }

        public DummyChildEntryCommands(Func<TClientType, long> parentIdFunc)
        {
            _parentIdFunc = parentIdFunc;
        }

        protected IDictionary<long, IList<TClientType>> Entries { get; } = new Dictionary<long, IList<TClientType>>();
        protected Func<TClientType, long> _parentIdFunc;
        protected long GetParentId(TClientType entry) => _parentIdFunc(entry);

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
