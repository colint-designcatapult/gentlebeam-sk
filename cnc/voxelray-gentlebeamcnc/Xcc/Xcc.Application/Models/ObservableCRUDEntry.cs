using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{

    public abstract class ObservableCRUDEntry<TData> : BaseEntry, IObservableCRUDEntry<TData>
        where TData : class, IEntry
    {
        public TData? Data
        {
            get => CRUD.Data;
        }
        public event EventHandler<CRUDEntryChangedArgs>? EntryChanged;

        protected ICRUDEntry<TData> CRUD { get; private set; }

        public ObservableCRUDEntry(
            IAsyncСRUDCommands<TData> crudCommands,
            TData? data = null)
        {
            CRUD = new BaseCRUDEntry<TData>(crudCommands, data);
        }

        public async Task<TData> CreateAsync()
        {
            if (CRUD.Data?.Id == NEW_ENTRY_ID)
            {
                var data = await CRUD.CreateAsync(CRUD.Data);
                OnCreated(data);
                return data;
            }
            else
            {
                throw new InvalidOperationException("Can't create an entry with valid ID");
            }
        }

        public async Task<TData> ReadAsync(long id)
        {
            var data = await CRUD.ReadDataAsync(id);
            OnDataChanged(data);
            return data;
        }

        public async Task<TData> UpdateAsync(TData newValue)
        {
            TData? data = null;
            if (newValue.Id == NEW_ENTRY_ID)
            {
                data = await CRUD.CreateAsync(newValue);
                OnCreated(data);
            }
            else
            {
                data = await CRUD.UpdateDataAsync(newValue);
                OnDataChanged(data);
            }
            return data;
        }

        public async Task DeleteAsync()
        {
            if (CanDelete())
            {
                await CRUD.DeleteAsync();
                OnDeleted();
            }
        }

        public void SetData(TData newValue)
        {
            CRUD.SetData(newValue);
            OnDataChanged(newValue);
        }

        public abstract bool CanDelete();

        private void OnCreated(TData newData)
        {
            EntryChanged?.Invoke(
                    this,
                    new CRUDEntryChangedArgs(CRUDEntryChangedAction.Create, newData)
                    );
        }
        private void OnDataChanged(TData newData)
        {
            EntryChanged?.Invoke(
                this,
                new CRUDEntryChangedArgs(CRUDEntryChangedAction.ChangeData, newData)
                );
        }

        private void OnDeleted()
        {
            EntryChanged?.Invoke(
                this,
                new CRUDEntryChangedArgs(CRUDEntryChangedAction.Delete)
                );
        }
    }


    public abstract class ObservableCRUDEntryNew<TData> : BaseEntry, IObservableCRUDEntry<TData>
        where TData : class, IEntry
    {
        public TData? Data
        {
            get => CRUD.Data;
        }
        public event EventHandler<CRUDEntryChangedArgs>? EntryChanged;

        protected ICRUDEntry<TData> CRUD { get; }

        public ObservableCRUDEntryNew(
            IAsyncСRUDCommands<TData> crudCommands,
            TData? data = null)
        {
            CRUD = new BaseCRUDEntryNew<TData>(crudCommands, data);
        }

        public async Task<TData> CreateAsync()
        {
            if (CRUD.Data?.Id == NEW_ENTRY_ID)
            {
                var data = await CRUD.CreateAsync(CRUD.Data);
                OnCreated(data);
                return data;
            }
            else
            {
                throw new InvalidOperationException("Can't create an entry with valid ID");
            }
        }

        public async Task<TData> ReadAsync(long id)
        {
            var data = await CRUD.ReadDataAsync(id);
            OnDataChanged(data);
            return data;
        }

        public async Task<TData> UpdateAsync(TData newValue)
        {
            TData data = null!;
            if (newValue.Id == NEW_ENTRY_ID)
            {
                data = await CRUD.CreateAsync(newValue);
                OnCreated(data);
            }
            else
            {
                data = await CRUD.UpdateDataAsync(newValue);
                OnDataChanged(data);
            }
            return data;
        }

        public async Task DeleteAsync()
        {
            if (CanDelete())
            {
                await CRUD.DeleteAsync();
                OnDeleted();
            }
        }

        public void SetData(TData newValue)
        {
            CRUD.SetData(newValue);
            OnDataChanged(newValue);
        }

        public abstract bool CanDelete();

        private void OnCreated(TData newData)
        {
            EntryChanged?.Invoke(
                    this,
                    new CRUDEntryChangedArgs(CRUDEntryChangedAction.Create, newData)
                    );
        }
        private void OnDataChanged(TData newData)
        {
            EntryChanged?.Invoke(
                this,
                new CRUDEntryChangedArgs(CRUDEntryChangedAction.ChangeData, newData)
                );
        }

        private void OnDeleted()
        {
            EntryChanged?.Invoke(
                this,
                new CRUDEntryChangedArgs(CRUDEntryChangedAction.Delete)
                );
        }
    }
}
