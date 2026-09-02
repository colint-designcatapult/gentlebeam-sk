using System;
using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Model.Indicators
{
    public class LaunchingProgress : BindableBase
    {
        readonly struct Context
        {
            public float TargetCurrentValue { get; }
            private float InitialCurrentValue { get; }

            public Context(float targetCurrentValue, float initialCurrentValue)
            {
                TargetCurrentValue = targetCurrentValue;
                InitialCurrentValue = initialCurrentValue;
            }

            public int GetProgress(float currentValue)
            {
                return Convert.ToInt32(
                    (currentValue - InitialCurrentValue) / (TargetCurrentValue - InitialCurrentValue) * 100.0);
            }
        }

        private IMainBoardModel MainBoardModel { get; }

        private int _value = 0;
        public int Value
        {
            get => _value;
            set { SetProperty(ref _value, value); }
        }

        private Context? _context = null;

        public LaunchingProgress(IMainBoardModel mainBoardModel) 
        {
            MainBoardModel = mainBoardModel;
        }

        public void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry == null)
            {
                return;
            }

            if (systemTelemetry.ControlBoardState == GcbStateNew.Launching
                || systemTelemetry.ControlBoardState == GcbStateNew.LaunchingForImaging)
            {
                var actualValue = systemTelemetry.HeaterCurrentFeedback;
                if (actualValue != 0)
                {
                    if (_context is null)
                    {
                        Value = 0;
                        _context = new Context(
                            targetCurrentValue: GetCurrentSetpoint(),
                            initialCurrentValue: actualValue);
                    }

                    // Feedback can already be above the target value.
                    if (actualValue < _context?.TargetCurrentValue)
                    {
                        Value = Math.Max(Value,
                                         _context?.GetProgress(actualValue) ?? 0);
                    }
                    else
                    {
                        Value = 100;
                    }
                }
            }
            else if (!systemTelemetry.IsEmissionState() && 
                     systemTelemetry.ControlBoardState != GcbStateNew.Termination &&
                     systemTelemetry.ControlBoardState != GcbStateNew.Discharge) // do not reset progress on these states
            {
                Value = 0;
                _context = null;
            }
            else
            {
                Value = 100; // emission, so launching is done
            }
        }
        private float GetCurrentSetpoint()
        {
            return MainBoardModel.CurrentEmission?.FilamentSetpoint
                ?? throw new InvalidOperationException("No current emission.");
        }
    }
}
