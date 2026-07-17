using System;

namespace Xcc.Core.Services
{

    public interface ITelemetryService: IDisposable
    {
        public void Start();
        
        public void Stop();
    }
}
