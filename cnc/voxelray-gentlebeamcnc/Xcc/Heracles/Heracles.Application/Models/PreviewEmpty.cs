using Heracles.Core.Enums;
using Heracles.Core.Models;

namespace Heracles.Application.Models
{
    public class PreviewEmpty : IPreview
    {
        public string Path { get; set; }
        public ESeriesFormat Format { get; set; }
        public string Modality { get; set; }
    }
}
