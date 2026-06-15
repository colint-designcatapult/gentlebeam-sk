using System;
using Xcc.Core.Domain.UPS;

namespace Xcc.Core.Services
{
    public interface IUpsService
    {
        event EventHandler<UpsTelemetryUpdatedArgs> UpsTelemetryUpdated;
        void Start();
    }
    public enum UpsType
    {
        Primary = 0,
        Secondary
    }

    public class UpsTelemetryUpdatedArgs(UpsType upsType, IUpsTelemetry? telemetry)
    {
        public UpsType UpsType => upsType;
        public IUpsTelemetry? Telemetry => telemetry;
    }
}
