
namespace Xcc.Core.Domain.DataManagement.System.QualityAssurance
{
    public interface IIntensityEntry
    {
        double? Intensity { get; set; }
        double? Deviation { get; set; }
    }
}