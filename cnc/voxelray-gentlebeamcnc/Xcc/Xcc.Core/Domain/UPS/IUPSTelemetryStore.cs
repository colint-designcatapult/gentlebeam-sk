using System;
using System.ComponentModel;

namespace Xcc.Core.Domain.UPS;

public interface IUPSTelemetryStore : INotifyPropertyChanged
{
    IUpsTelemetry? Primary { set; get; }

    event EventHandler<bool?> BatteryInUseStateUpdated;
}