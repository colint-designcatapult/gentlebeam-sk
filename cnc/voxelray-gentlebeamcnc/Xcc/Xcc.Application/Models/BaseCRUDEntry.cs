using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    /// <summary>
    /// BaseCRUDEntry wraps TData state 
    /// to encapsulate Update logic with TData field mask evaluation
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class BaseCRUDEntry<TData> : ICRUDEntry<TData>
        where TData : class, IEntry
    {
        public TData? Data { get; protected set; }

        public IAsyncСRUDCommands<TData> CrudCommands { get; }


        public async Task<TData> CreateAsync(TData value)
        {
            if (value != null)
            {
                return Data = await CrudCommands.CreateAsync(value);
            }
            else
            {
                throw new ArgumentNullException("Can't create the entry: value is null");
            }
        }

        public async Task<TData> ReadDataAsync(long id)
        {
            return Data = await CrudCommands.ReadAsync(id);
        }

        public async Task<TData> UpdateDataAsync(TData value)
        {
            if (Data == null)
            {
                throw new NullReferenceException("Can't update the entry: internal state is null");
            }
            else if (value == null)
            {
                throw new ArgumentNullException("Can't update the entry: new value is null");
            }
            else
            {
                //var mask = Common.GenericExtensions.CompareProperties(Data, value);
                return Data = await CrudCommands.UpdateAsync(Data, value);
            }
        }

        public async Task DeleteAsync()
        {
            if (Data != null)
            {
                bool result = await CrudCommands.DeleteAsync(Data.Id);
                if (result)
                {
                    Data = null!;
                }
            }
            else
            {
                throw new NullReferenceException("Can't remove the entry: Data is null");
            }
        }

        public void SetData(TData value)
        {
            Data = value;
        }

        public BaseCRUDEntry(
            IAsyncСRUDCommands<TData> crudCommands,
            TData? data = null)
        {
            CrudCommands = crudCommands;
            Data = data;
        }
    }

    public class BaseCRUDEntryNew<TData> : ICRUDEntry<TData>
        where TData : class, IEntry
    {
        public TData? Data { get; protected set; }

        public IAsyncСRUDCommands<TData> CrudCommands { get; }


        public async Task<TData> CreateAsync(TData value)
        {
            if (value != null)
            {
                return Data = await CrudCommands.CreateAsync(value);
            }
            else
            {
                throw new ArgumentNullException("Can't create the entry: value is null");
            }
        }

        public async Task<TData> ReadDataAsync(long id)
        {
            return Data = await CrudCommands.ReadAsync(id);
        }

        public async Task<TData> UpdateDataAsync(TData value)
        {
            if (Data == null)
            {
                throw new NullReferenceException("Can't update the entry: internal state is null");
            }
            else if (value == null)
            {
                throw new ArgumentNullException("Can't update the entry: new value is null");
            }
            else
            {
                //var mask = Common.GenericExtensions.CompareProperties(Data, value);
                return Data = await CrudCommands.UpdateAsync(Data, value);
            }
        }

        public async Task DeleteAsync()
        {
            if (Data != null)
            {
                bool result = await CrudCommands.DeleteAsync(Data.Id);
                if (result)
                {
                    Data = null!;
                }
            }
            else
            {
                throw new NullReferenceException("Can't remove the entry: Data is null");
            }
        }

        public void SetData(TData value)
        {
            Data = value;
        }

        public BaseCRUDEntryNew(
            IAsyncСRUDCommands<TData> crudCommands,
            TData? data = null)
        {
            CrudCommands = crudCommands;
            Data = data;
        }
    }
}
