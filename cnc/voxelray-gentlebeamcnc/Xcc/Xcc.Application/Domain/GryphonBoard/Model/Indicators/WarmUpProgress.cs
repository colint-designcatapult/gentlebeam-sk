using System;
using System.ComponentModel;
using Prism.Mvvm;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Model.Indicators
{
    public interface IWarmupProgress : INotifyPropertyChanged
    {
        double Value { get; }
        void Reset(WarmupParameters warmupParameters);
    }

    public class WarmUpProgress : BindableBase
    {
        public WarmUpProgress(
            INotifyWarmupEvent warmupEventSource,
            ILogWriter logWriter)
        {
            warmupEventSource.WarmupEvent += OnWarmupEvent;
            _logWriter = logWriter;
        }

        private const double SetpointEpsilon = 0.0001;
        private readonly ILogWriter _logWriter;

        public double WarmupSetpoint { get; private set; } = 0;
        public WarmupType WarmupType { get; private set; } = WarmupType.Fast;

        private double _value = 0;
        public double Value
        {
            get => _value;
            set { SetProperty(ref _value, value); }
        }

        private double? _initialHeaterCurrentValue = null;

        public void Reset(WarmupParameters? warmupParameters = null)
        {
            if (warmupParameters != null)
            {
                WarmupSetpoint = warmupParameters.Value.HeaterCurrentSetpoint;
                WarmupType = warmupParameters.Value.WarmupType;
            }
            Value = 0.0f;
            _initialHeaterCurrentValue = null;
        }

        public void OnSystemTelemetryChanged(ISystemTelemetry? telemetry)
        {
            if (telemetry == null)
                return;

            if (IsWarmingUp(telemetry.ControlBoardState))
            {
                float heaterCurrentFeedback = telemetry.HeaterCurrentFeedback;

                if (heaterCurrentFeedback == 0.0f)
                    return;

                if (_initialHeaterCurrentValue is null)
                {
                    _initialHeaterCurrentValue = heaterCurrentFeedback;
                    _ = _logWriter.LogAsync(
                        $"WarmupProgress: initialize warmup with {heaterCurrentFeedback}mA start value",
                        LogRecordSeverity.Info, LogRecordType.System);
                }

                var range = WarmupSetpoint - _initialHeaterCurrentValue;

                if (Math.Abs(range.Value) < SetpointEpsilon)
                {
                    // range is close to 0, prevent division by 0
                    Value = 100.0;
                }
                else
                {
                    var currentValue = (heaterCurrentFeedback - _initialHeaterCurrentValue.Value) / range.Value * 100.0;
                    Value = Math.Max(Value, Math.Min(currentValue, 100)); // prevent progress bar from degrading, taking max achieved value
                }
            }
            else
            {
                Reset();
            }
        }

        private bool IsWarmingUp(GcbStateNew gcbState)
        {
            return WarmupType switch
            {
                WarmupType.Fast => gcbState == GcbStateNew.Warmup,
                WarmupType.Full => gcbState == GcbStateNew.DailyWarmup,
                _ => throw new NotImplementedException()
            };
        }

        private void OnWarmupEvent(object? sender, WarmupEventArgs e)
        {
            if (e.EventType == WarmupEventType.Start)
            {
                _ = _logWriter.LogAsync(
                    $"WarmupProgress: start warmup with {e.WarmupParameters.HeaterCurrentSetpoint}mA setpoint",
                    LogRecordSeverity.Info, LogRecordType.System);
                Reset(e.WarmupParameters);
            }
        }
    }
}
