
namespace Xcc.Core.Domain.DataManagement.System
{
    public interface IHeaterCurrentConfig : ISystemPresetEntry
    {
        double? HeaterCurrent { get; set; }
        bool IsSet { get; }
    }
}
