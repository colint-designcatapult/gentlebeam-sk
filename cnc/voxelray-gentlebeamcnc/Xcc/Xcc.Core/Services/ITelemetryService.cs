using System;

namespace Xcc.Core.Services
{
    public enum TelemetryServiceMode : int
    {
        // Do nothing
        None = 0,
        // No telemetry requests, listening only
        Passive = 1, 
        // Request and receive telemetry
        Active = 2,
    }


    public interface ITelemetryService: IDisposable
    {
        TelemetryServiceMode Mode { get; }

        public void Start(TelemetryServiceMode mode);
        
        public void Stop();
    }
}
