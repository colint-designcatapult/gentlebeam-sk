using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Common.Application.Globals;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Application.Infra.DataManagement.EMR.DataAccess;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Core.Common;

namespace Heracles.Application.AppLayer.Patient
{
    public interface IPhotoService
    {
        Task<(ObservableCollection<IPhoto> photos, CancellationTokenSource token)> GetPhotosAsync(long diagnosisId);
        Task<IPhoto> GetPhotoAsync(IPhotoDescription photoDescription, CancellationToken token);
        Task SendPhotosAsync(IEnumerable<IPhoto> photos);
    }
    
    public class PhotoService : IPhotoService
    {
        private const int ChunkSize = 256 * 1024; // todo: can be configured from AppSettings

        public PhotoService(
            IEmrPhotoCommands photoCommands,
            IPhotoStreamReader photoStreamReader,
            IAppGlobals appGlobals)
        {
            PhotoCommands = photoCommands;
            StreamReader = photoStreamReader;
            AppGlobals = appGlobals;
        }

        public IEmrPhotoCommands PhotoCommands { get; }
        public IPhotoStreamReader StreamReader { get; }
        public IAppGlobals AppGlobals { get; }

        public async Task<(ObservableCollection<IPhoto> photos, CancellationTokenSource token)> GetPhotosAsync(long diagnosisId)
        {
            var photoDescriptions = await PhotoCommands.ReadListAsync(diagnosisId);
            var photos = new ObservableCollection<IPhoto>();
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(AppGlobals.AppCancellationTokenSource.Token);

            foreach (var photoDescription in photoDescriptions)
            {
                var photo = new Photo(photoDescription);
                    photos.Add(photo);
            }

            _ = Task.Run(async () =>
            {
                foreach (var photo in photos.ToList())
                {
                    var photoWithImage = await GetPhotoAsync(photo, cancellationTokenSource.Token);
                    if (photoWithImage != null)
                        System.Windows.Application.Current.Dispatcher.Invoke(() => photos[photos.IndexOf(photo)] = photoWithImage);
                }
            }, cancellationTokenSource.Token);

            return (photos, cancellationTokenSource);
        }

        public Task<IPhoto> GetPhotoAsync(IPhotoDescription photoDescription, CancellationToken token)
        {
            return StreamReader.ReceivePhotoAsync(photoDescription, token);
        }

        public async Task SendPhotosAsync(IEnumerable<IPhoto> photos)
        {
            foreach (var photo in photos)
            {
                if (photo.Id == BaseEntry.NewEntryId)
                {
                    var photoDescription = await PhotoCommands.CreateAsync(photo);
                    photoDescription.CopyProperties(photo);
                }

                await PhotoCommands.SendPhotoAsync(photo, ChunkSize, AppGlobals.AppCancellationTokenSource.Token);
            }
        }
    }
}
