using Empyrean.Common.Infra.Networking.Udp;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Xcc.Application.Events;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Xcc.Application.Domain.GryphonBoard
{
    public class DummyMainBoardModel : MainBoardModelBase
    {
        private const string DummyFaultFormat = "Dummy heater current %f mA exceeded limit %f mA.";
        private readonly List<FaultEntry> _faults = [];
        private uint _faultClearEpoch = 1;

        public DummyMainBoardModel(
            IGCBDataStore gcbDataStore,
            ILogWriter logWriter,
            IEventAggregator eventAggregator,
            IGcbCommandInterface gcbAPI)
            : base(gcbDataStore, logWriter, gcbAPI, eventAggregator)
        {
        }

        public override Task<bool> Initialize()
        {
            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Initialize,
                Parameter = null!
            });
            return Task.FromResult(true);
        }

        public override Task<FaultSnapshot> GetFaults()
        {
            _ = LogWriter.LogAsync("GetFaults", LogRecordSeverity.Info, LogRecordType.System);

            var snapshot = new FaultSnapshot(
                _faultClearEpoch,
                Array.AsReadOnly(_faults.ToArray()));
            GcbDataStore.ReplaceFaults(snapshot);
            return Task.FromResult(snapshot);
        }

        private void AddDummyFault(float actual, float limit)
        {
            if (_faults.Any(entry => string.Equals(entry.Format, DummyFaultFormat, StringComparison.Ordinal)) ||
                _faults.Count >= 4)
            {
                return;
            }

            byte[] formatBytes = Encoding.ASCII.GetBytes(DummyFaultFormat);
            string message = string.Create(
                CultureInfo.InvariantCulture,
                $"Dummy heater current {actual:G9} mA exceeded limit {limit:G9} mA.");
            var entry = new FaultEntry(
                SystemFault.OtherFault,
                CrcUtils.ComputeChecksum(formatBytes),
                SystemTelemetry?.ControlBoardState ?? GcbStateNew.NoComm,
                (uint)(SystemTelemetry?.SystemRuntime ?? 0),
                DummyFaultFormat,
                message);

            uint index = (uint)_faults.Count;
            _faults.Add(entry);
            GcbDataStore.ApplyFaultUpdate(new FaultUpdate(
                _faultClearEpoch,
                index,
                (uint)_faults.Count,
                entry));
        }

        protected override void UpdateCurrentPlanState(ISystemTelemetry? systemTelemetry)
        {
            if (systemTelemetry?.IsEmissionState() == true
                || systemTelemetry?.ControlBoardState == GcbStateNew.Termination)
            {
                int currentPoint = systemTelemetry.CurrentOperationalPoint;
                var index = currentPoint < CurrentPlan.TotalPoints ? currentPoint : CurrentPlan.TotalPoints - 1;

                var currentPointValue = CurrentPlan[index];

                if (systemTelemetry is { ControlBoardState: GcbStateNew.Emission or GcbStateNew.Termination })
                {
                    currentPointValue.RemainingPointTime = currentPointValue.InitialRemainingPointTime - systemTelemetry.PrimaryTimerValue;
                }
                else
                {
                    // in imaging, timers run in reverse, as a countdown:
                    currentPointValue.RemainingPointTime = systemTelemetry!.PrimaryTimerValue;
                }

                CurrentPlan.UpdatePoint(currentPointValue);
            }
        }

        public override Task ClearFaults()
        {
            _faults.Clear();
            _faultClearEpoch = unchecked(_faultClearEpoch + 1u);
            GcbDataStore.ApplyFaultUpdate(new FaultUpdate(_faultClearEpoch, 0, 0, null));
            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.ClearErrors
            });

            OnGcbActionCompletion(GcbActionType.ClearErrors);
            return Task.CompletedTask;
        }

        public override Task<VersionInfo> GetVersionInfo()
        {
            return Task.FromResult(new VersionInfo
            {
                Major = 44,
                Minor = 44,
                FirmwareChecksum = 55,
                Level = 66,
                Mode = FirmwareMode.Test
            });
        }
        
        public override async Task ResumePlan()
        {
            float energy = 0.0f;
            GcbOperationalPoint? point = CurrentPlan?.Points.FirstOrDefault();
            if (point != null)
            {
                energy = point.Value.SetpointKv;
            }

            var gcbPlanState = await QueryPlanFromGCB();
            foreach (var pt in gcbPlanState.Points)
            {
                CurrentPlan?.UpdatePoint(pt);
            }

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Loading,
                Parameter = energy
            });

            OnGcbActionCompletion(GcbActionType.ReleasePlan);
            _ = LogWriter.LogAsync($"XRay ResumePlan successfully: steps={CurrentPlan?.TotalPoints}", LogRecordSeverity.Info, LogRecordType.System);
        }
        
        public override Task<bool> Stop()
        {
            CancelCurrentTask();

            if (SystemTelemetry is null)
            {
                throw new GcbNoConnectionException("No connection to GCB: stop failed");
            }

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Stopped
            });

            OnGcbActionCompletion(GcbActionType.Stop);
            _= LogWriter.LogAsync("XRay source stopped successfully", LogRecordSeverity.Info, LogRecordType.System);

            return Task.FromResult(true);
        }

        public override Task ClearPlan()
        {
            int delay = 3000;

            if (SystemTelemetry is null)
            {
                IsPlanStaged = false;
                Session = null;

                OnGcbActionCompletion(GcbActionType.ClearPlan);

                return Task.CompletedTask;
            }

            if (SystemTelemetry.IsFaultState())
                throw new InvalidOperationException("Cannot clear the plan in the Fault state");

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.ClearPlan,
                Parameter = delay,
            });

            IsPlanStaged = false;
            Session = null;
            OnGcbActionCompletion(GcbActionType.ClearPlan);

            _= LogWriter.LogAsync("XRay source stopped successfully", LogRecordSeverity.Info, LogRecordType.System);

            return Task.CompletedTask;
        }

        public override Task<GcbOperationalPoint> QueryPointFromGCB(int index)
        {
            var gcbPoint = CurrentPlan[index];
            gcbPoint.RemainingPointTime = gcbPoint.TotalPointTime - gcbPoint.ActualDuration;

            return Task.FromResult(gcbPoint);
        }

        public override async Task<GcbEmissionPlan> QueryPlanFromGCB()
        {
            var telemetry = SystemTelemetry;
            if (telemetry == null)
                return null!;

            var totalPoints = telemetry.TotalOperationalPoints;

            GcbEmissionPlan plan = new();
            for (int i = 0; i < totalPoints; i++)
            {
                var point = await QueryPointFromGCB(i);
                plan.AddPoint(point);
            }
            return plan;
        }

        protected override async Task<bool> WarmUp(float heaterCurrentSetpoint, CancellationToken cancellationToken)
        {
            var random = new Random();
            if (random.Next(10) < 1)
            {
                int faultBit = (int)SystemFault.OtherFault;
                AddDummyFault(heaterCurrentSetpoint + 1.0f, heaterCurrentSetpoint);
                EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
                {
                    Status = DummyXrayStatus.SetWarmupFault,
                    Parameter = faultBit
                });
                return await Task.FromResult(true);
            }

            int delayMs = 5000;

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.WarmingUp,
                Parameter = delayMs
            });

            try
            {
                await WaitForState(GcbStateNew.Primed, cancellationToken);
                _ = LogWriter.LogAsync("XRay warmup successfully", LogRecordSeverity.Info, LogRecordType.System);
                return true;
            }
            catch (TaskCanceledException)
            {
                EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
                {
                    Status = DummyXrayStatus.Stopped,
                });
                throw;
            }
        }

        protected override async Task<bool> Conditioning(float heaterCurrentSetpoint, CancellationToken cancellationToken)
        {
            var random = new Random();
            if (random.Next(10) < 1)
            {
                int faultBit = (int)SystemFault.OtherFault;
                AddDummyFault(heaterCurrentSetpoint + 2.0f, heaterCurrentSetpoint);
                EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
                {
                    Status = DummyXrayStatus.SetWarmupFault,
                    Parameter = faultBit
                });
                return await Task.FromResult(true);
            }

            int delayMs = 10000;

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Conditioning,
                Parameter = delayMs
            });

            _ = LogWriter.LogAsync("XRay Conditioning successfully", LogRecordSeverity.Info, LogRecordType.System);
            return true;
        }

        protected override Task CallResetTimersAsync(CancellationToken cancellationToken)
        {
            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.ResetTimers,
            });

            _ = LogWriter.LogAsync("XRay did reset timers successfully", LogRecordSeverity.Info, LogRecordType.System);

            return Task.CompletedTask;
        }
        
        protected override Task StartPoint()
        {
            var telemetry = SystemTelemetry;
            if (telemetry is null)
            {
                throw new NullReferenceException(nameof(telemetry));
            }
            else
            {
                int currentPoint = telemetry.CurrentOperationalPoint;
                if (currentPoint < CurrentPlan.TotalPoints)
                {
                    var currentPointValue = CurrentPlan[currentPoint];
                    currentPointValue.InitialRemainingPointTime = currentPointValue.RemainingPointTime;
                    CurrentPlan.UpdatePoint(currentPointValue);
                }
            }

            if (CurrentPlan == null || CurrentPlan.TotalPoints == 0)
                return Task.CompletedTask;
            
            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Started,
                Parameter = new List<GcbOperationalPoint>(CurrentPlan.Points)
            });

            OnGcbActionCompletion(GcbActionType.StartBeamOn);
            _= LogWriter.LogAsync("XRay source started successfully", LogRecordSeverity.Info, LogRecordType.System);

            return Task.CompletedTask;
        }

        protected override Task<bool> StartPlan()
        {
            //if (EmissionPlanStepDurations != null && EmissionPlanStepDurations.Count > 0)
            //{
            //    int duration = EmissionPlanStepDurations[0] * 1000;
            //}

            if (CurrentPlan == null || CurrentPlan.TotalPoints == 0)
                return Task.FromResult(false); 

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Started,
                Parameter = new List<GcbOperationalPoint>(CurrentPlan.Points)
            });

            OnGcbActionCompletion(GcbActionType.ReleasePlan);
            _= LogWriter.LogAsync("XRay source started successfully", LogRecordSeverity.Info, LogRecordType.System);

            return Task.FromResult(true); 
        }

        protected override async Task<bool> LoadAndStartPlan(
            CancellationToken cancellationToken)
        {

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.SetPlan,
                Parameter = new List<GcbOperationalPoint>(CurrentPlan.Points)
            });

            _ = LogWriter.LogAsync($"SendOperationalPoints", LogRecordSeverity.Info, LogRecordType.System);
            foreach (var op in CurrentPlan.Points)
            {
                LogOperationalPoint(op);
            }

            foreach (var step in CurrentPlan.Points)
            {
                float duration;
                if (step.ActualDuration > 0)
                    duration = step.TotalPointTime - step.ActualDuration; // if it was interrupted
                else
                    duration = step.TotalPointTime;

                if (duration <= 0)
                    continue;

                await Task.Delay(300);
            }

            float energy = 0.0f;
            GcbOperationalPoint? point = CurrentPlan.Points.FirstOrDefault();
            if (point != null)
            {
                energy = point.Value.SetpointKv;
            }

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Publish(new DummyXrayStatusChangedEventArgs
            {
                Status = DummyXrayStatus.Loading,
                Parameter = energy
            });

            Session = new GcbSession(1, CurrentPlan.Points.Count());
            OnGcbActionCompletion(GcbActionType.NewSession);

            IsPlanStaged = true;
            OnGcbActionCompletion(GcbActionType.ReleasePlan);

            _= LogWriter.LogAsync($"XRay LoadPlan successfully: steps={CurrentPlan.TotalPoints}", LogRecordSeverity.Info, LogRecordType.System);

            return true;
        }
    }
}
