using Heracles.Core.Enums;

namespace Heracles.Core.Models
{
    public interface IPreview
    {
        public string Path { get; set; }

        public ESeriesFormat Format { get; set; }

        public string Modality { set; get; }
    }
}
