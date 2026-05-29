namespace Heracles.Application.Models.Treatment
{
    public interface ILesionInfo
    {
        public double? LesionDepth { get; set; }

        public double? LesionSizeL { get; set; }

        public double? LesionSizeW { get; set; }
    }

    public struct LesionInfo : ILesionInfo
    {
        public double? LesionDepth { get; set; }

        public double? LesionSizeL { get; set; }

        public double? LesionSizeW { get; set; }
    }
}
