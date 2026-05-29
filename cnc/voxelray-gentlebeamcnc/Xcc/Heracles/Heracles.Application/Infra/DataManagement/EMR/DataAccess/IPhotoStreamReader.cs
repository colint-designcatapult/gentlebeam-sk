using System.Threading;
using System.Threading.Tasks;
using Heracles.Core.Models.EMR;

namespace Heracles.Application.Infra.DataManagement.EMR.DataAccess;

public interface IPhotoStreamReader
{
    Task<IPhoto> ReceivePhotoAsync(IPhotoDescription photoDescription, CancellationToken token);
}