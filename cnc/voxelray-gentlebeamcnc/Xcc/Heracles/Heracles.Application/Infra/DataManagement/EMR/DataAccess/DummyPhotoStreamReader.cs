using System;
using System.Threading;
using System.Threading.Tasks;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Models.EMR;
using Xcc.Infra.DataManagement.Common;

namespace Heracles.Application.Infra.DataManagement.EMR.DataAccess;

public class DummyPhotoStreamReader : IPhotoStreamReader
{
    public static readonly string[] PhotoPaths =
    [
        "pack://application:,,,/Xcc.Application;Component/UI/Resources/Images/DemoImageSet/Photos/1.jpeg",
        "pack://application:,,,/Xcc.Application;Component/UI/Resources/Images/DemoImageSet/Photos/2.jpeg",
    ];
    
    public Task<IPhoto> ReceivePhotoAsync(IPhotoDescription photoDescription, CancellationToken token)
    {
        IPhoto photo = new Photo(photoDescription)
        {
            Data = ResourceStreamReader.LoadImageFromPackUri(new Uri(photoDescription.Path))
        };

        return Task.FromResult(photo);
    }
}