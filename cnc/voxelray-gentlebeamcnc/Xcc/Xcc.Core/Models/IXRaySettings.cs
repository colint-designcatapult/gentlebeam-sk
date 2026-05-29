namespace Xcc.Core.Models;

public interface IXRaySettings
{
    /// <summary>
    /// [W]
    /// </summary>
    public double XrayTubePower { get; }
    public double XrayTubePower50kV { get; }
    public double XrayTubePower70kV { get; }
    public double XrayTubePower100kV { get; }
    int QcFieldDuration { get; }
    int SafetyCheckFieldDuration { get; }
}