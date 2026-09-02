using System;
using System.Diagnostics;
using Prism.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Model.Indicators
{
    public class HVSetupProgress : BindableBase
    {
        readonly struct Context
        {
            public float SetpointKv { get; }

            public Context(float setpointKv)
            {
                SetpointKv = setpointKv;
            }

            public int GetProgress(float kvFeedback)
            {
                return (int)((kvFeedback / SetpointKv) * 100);
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
                    if (_context is null)
                    {
                        Value = 0;
                        _context = new Context(GetKvSetpoint());
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

        private float GetKvSetpoint()
        {
            return MainBoardModel.CurrentEmission?.SetpointKv
                ?? throw new InvalidOperationException("No current emission.");
        }
    }
}
