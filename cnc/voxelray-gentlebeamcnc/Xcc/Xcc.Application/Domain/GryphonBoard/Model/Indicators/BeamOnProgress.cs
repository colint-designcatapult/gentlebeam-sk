using Prism.Mvvm;
using System;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.Domain.GryphonBoard.Model.Indicators
{
    public class BeamOnProgress : BindableBase
    {
        public BeamOnProgress(IMainBoardState mainBoardState)
        {
            this.mainBoardState = mainBoardState;
        }

        private readonly IMainBoardState mainBoardState;
        private double _initialRemainingTime = 0;
        private double _value = 0;
        public double Value { get => _value; set => SetProperty(ref _value, value); }
        public void Reset()
        {
            _initialRemainingTime = mainBoardState.CurrentPlan.TotalTime;
            Value = 0;
        }

        public void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry?.IsEmissionState() == true
                || systemTelemetry?.ControlBoardState == Core.Enums.GcbStateNew.Termination)
            {
                if (_initialRemainingTime > double.Epsilon)
                {
                    double remainingTime = mainBoardState.CurrentPlan.RemainingTime;
                    if (remainingTime < 0.1)
                    {
                        Value = 100.0;
                    }
                    else 
                    {
                        Value = Math.Max(
                            Value,
                            (_initialRemainingTime - remainingTime) * 100 / _initialRemainingTime);

                    }
                }
            }
        }
    }
}
