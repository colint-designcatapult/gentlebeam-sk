using System.Threading.Tasks;

namespace Heracles.Application.Services
{
    public interface IWakeOnLanService
    {
        Task WakeUpAsync();
    }
}
