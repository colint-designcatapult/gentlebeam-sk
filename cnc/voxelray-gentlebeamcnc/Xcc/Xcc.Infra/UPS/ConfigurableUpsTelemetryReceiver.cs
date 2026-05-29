using System;
using Xcc.Core.Models;

namespace Xcc.Infra.UPS
{
    /// <summary>
    /// Specified UPS telemetry receiver 
    /// derived for dependency injection with global settings
    /// </summary>
    /// <param name="appGlobals"></param>
    public class ConfigurableUpsTelemetryReceiver(
        ICoreSettings coreSettings,
        IAppGlobals appGlobals) 
        : UpsTelemetryReceiver(
            coreSettings.UpsBroadcastServiceEndPoint.Port ??
                throw new NullReferenceException("UpsTelemetryReceiver error: no broadcast endpoint"),
            appGlobals.AppCancellationTokenSource.Token)
    {
    }
}
