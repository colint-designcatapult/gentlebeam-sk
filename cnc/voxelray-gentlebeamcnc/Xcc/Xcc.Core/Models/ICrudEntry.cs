using System.Threading.Tasks;

namespace Xcc.Core.Models
{
    public interface ICRUDEntry<TData>
        where TData : class
    {
        TData? Data { get; }
        Task<TData> CreateAsync(TData value);
        Task<TData> ReadDataAsync(long id);
        Task<TData> UpdateDataAsync(TData value);
        Task DeleteAsync();
        void SetData(TData newValue);
    }
}
