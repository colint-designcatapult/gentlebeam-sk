using Heracles.Core.Enums;

namespace Heracles.Application.Models.Treatment
{
    public interface IAcquisitionResultStore
    {
        ILesionInfo LesionInfo { get; set; }
        string Location { get; set; }
        ImageType Type { get; set; }
    }

    public class AcquisitionResultStore : IAcquisitionResultStore
    {
        public ILesionInfo LesionInfo { get; set; } = new LesionInfo();
        public string Location {  get; set; }   
        public ImageType Type { get; set; }
    }
}