using System.Threading.Tasks;

namespace Xcc.Core.Models
{
    public interface IObservableCRUDEntry<TData> : IObservableDataEntry<TData>
        where TData : class
    {
        Task<TData> CreateAsync();
        Task DeleteAsync();
        bool CanDelete();
    }
}
