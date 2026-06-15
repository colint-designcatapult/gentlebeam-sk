namespace Xcc.Core.Models;

public interface ICoreSettings
{
    public ISystemEndPoint GCBTelemetryEndPoint { get; set; }
    public ISystemEndPoint GCBCommandsEndPoint { get; set; }
    public ISystemEndPoint QcbCommandsEndPoint { get; set; }
    public ISystemEndPoint DataCommandsEndPoint { get; set; }
    public ISystemEndPoint UpsBroadcastServiceEndPoint { get; set; }

    public int GrpcTimeout { get; }
    public string StorageRoot { get; }
    public string StartupLoginUsername { get; }
    public string? CameraUriSource { get; set; }
}