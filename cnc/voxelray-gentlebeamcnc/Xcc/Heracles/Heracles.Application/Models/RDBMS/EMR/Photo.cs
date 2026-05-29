using Heracles.Core.Models.EMR;
using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR;

public class Photo : PhotoDescription, IPhoto
{
    public Photo(IPhotoDescription photoDescription)
    {
        photoDescription.CopyProperties(this);
    }

    public Photo()
    {
    }

    public byte[] Data { get; set; }
}