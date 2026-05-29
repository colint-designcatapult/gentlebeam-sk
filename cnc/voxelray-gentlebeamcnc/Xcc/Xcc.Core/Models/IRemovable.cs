using System.Threading.Tasks;

namespace Xcc.Core.Models
{
    public interface IsRemovable
    {
        bool CanRemove();
    }

    public interface IRemovable : IsRemovable
    {
        void Remove();
    }

    public interface IRemovableAsync : IsRemovable
    {
        Task RemoveAsync();
    }
}
