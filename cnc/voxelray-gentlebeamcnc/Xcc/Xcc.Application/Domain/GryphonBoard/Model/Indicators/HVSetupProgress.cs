using System;
using System.Diagnostics;
using System.Linq;
using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Model.Indicators
{
    public class HVSetupProgress : BindableBase
    {
        struct Context
        {
            public int PointIndex { get; } = 0;
            public float Setpoint_kV { get; } = 0;

            public Context(int pointIndex, float setpoint_kV)
            {
                PointIndex = pointIndex;
                Setpoint_kV = setpoint_kV;
            }

            public int GetProgress(float kvFeedback)
            {
                return (int)((kvFeedback / Setpoint_kV) * 100);
            }
        }

        public IMainBoardModel MainBoardModel { get; }

        private int _value = 0;
        public int Value
        {
            get => _value;
            set { SetProperty(ref _value, value); }
        }

        private Context? _context = null;

        public HVSetupProgress(IMainBoardModel mainBoardModel)
        {
            MainBoardModel = mainBoardModel;
        }

        public void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry == null)
            {
                return;
            }

            try
            {
                if (systemTelemetry.ControlBoardState == GcbStateNew.HVSetup)
                {
                    if (_context?.PointIndex != systemTelemetry.CurrentOperationalPoint)
                    {
                        var pointIndex = systemTelemetry.CurrentOperationalPoint;
                        Value = 0;
                        _context = new Context(pointIndex, setpoint_kV: GetKvSetpoint(pointIndex));
                    }

                    Value = Math.Max(
                        Value,
                        Math.Min(_context?.GetProgress(systemTelemetry.KvFeedback) ?? 0, 100));
                }
                else
                {
                    ResetContext();
                }
            }
            catch(Exception ex)
            {
                // TODO: what should we do here?
                Debug.WriteLine($"HVSetupProgress error: {ex.Message}");
                ResetContext();
            }

        }

        private void ResetContext()
        {
            Value = 0;
            _context = null;
        }

        private float GetKvSetpoint(int pointIndex)
        {
            return MainBoardModel.CurrentPlan.Points.ElementAt(pointIndex).SetpointKv;
        }
    }
}
