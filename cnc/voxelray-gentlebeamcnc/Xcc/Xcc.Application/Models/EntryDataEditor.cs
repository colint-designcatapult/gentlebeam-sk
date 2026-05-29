using Prism.Mvvm;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public abstract class EntryDataEditor<TData> : BindableBase, ISubmitAction
        where TData : class, IEntry
    {
        private IAsyncDataEntry<TData> Entry { get; }

        private TData _data;
        public TData Data { get => _data; set => SetProperty(ref _data, value); }

        protected EntryDataEditor(IAsyncDataEntry<TData> entry, TData data = null)
        {
            Entry = entry;
            Data = CopyData(data ?? entry.Data);
        }
        protected abstract TData CopyData(TData data);

        public async Task SubmitAsync()
        {
            Data = await Entry.UpdateAsync(Data);
        }
    }
}
