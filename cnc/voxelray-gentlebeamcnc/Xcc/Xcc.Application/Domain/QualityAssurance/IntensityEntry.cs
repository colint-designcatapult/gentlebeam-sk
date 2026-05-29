using Xcc.Core.Domain.DataManagement.System.QualityAssurance;

namespace Xcc.Application.Domain.QualityAssurance
{
    public class IntensityEntry : IIntensityEntry
    {
        public double? Intensity { get; set; }
        public double? Deviation { get; set; }
    }
}
