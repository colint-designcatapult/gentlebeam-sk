namespace Xcc.Core.Models;

public interface IAcbSettings
{
    public int AcbReceiveTimeout { get; }
    public bool UseDummyHeadActuators { get; }
}