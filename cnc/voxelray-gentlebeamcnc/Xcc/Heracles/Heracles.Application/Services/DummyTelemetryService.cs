using Empyrean.Common.Infra.Networking.Udp;

using Prism.Events;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Xcc.Application.Events;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Application.Services
{
    public class TelemetryPacket : UdpPacket
    {
        public TelemetryPacket(GcbStateNew state, uint packetId = 0)
            : base(packetType: (uint)GCBPacketType.TelemetryResponse,
                  packetCounter: packetId,
                  payloadLength: (uint)GCBTelemetryResponseField.PayloadFields)
        {
            Set((int)GCBTelemetryResponseField.SystemState, (int)state);
        }
    }

    public class DummyTelemetryService : ITelemetryService
    {
        ulong _mainCollimatorSerial = 0;//0xFEEDFACEDEADBEEF; // 70kV dummy collimator
        readonly ulong _secondCollimatorSerial = 0x78563412EFCDAB90; // 50kv dummy collimator
        //readonly ulong _secondCollimatorSerial = 0xCDCD1234CDCD1234; // QC dummy collimator
        public DummyTelemetryService(
            ILogWriter logWriter,
            IAppGlobals appGlobals,
            IEventAggregator eventAggregator,
            ISystemTelemetryChanged systemTelemetryChangedCallback,
            IDebugSettings debugSettings)
        {
            LogWriter = logWriter;
            AppGlobals = appGlobals;
            EventAggregator = eventAggregator;
            SystemTelemetryChangedCallback = systemTelemetryChangedCallback;
            DebugSettings = debugSettings;
            _mainCollimatorSerial = string.IsNullOrEmpty(DebugSettings.DummyCollimatorSerial) ? _mainCollimatorSerial : Convert.ToUInt64(DebugSettings.DummyCollimatorSerial, 16);
            _serial = _mainCollimatorSerial;

            EventAggregator.GetEvent<DummyXrayStatusChangedEvent>().Subscribe(OnDummyXrayStatusChangedEvent);

            // For now, let's just go to Cold directly, without Initializing
            SetGCBState(GcbStateNew.Cold);
            var tokenSource = CancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    for (int n = 0; n < 10; ++n)
                    {
                        await Task.Delay(1000, tokenSource.Token);
                        GenerateTelemetry((int)_state);
                    }
                    //// Switch serial to test collmator change
                    //if (_serial == _mainCollimatorSerial)
                    //{
                    //    _serial = _secondCollimatorSerial;
                    //}
                    //else
                    //{
                    //    _serial = _mainCollimatorSerial;
                    //}
                }
            }, tokenSource.Token);

        }

        public ILogWriter LogWriter { get; }
        public IAppGlobals AppGlobals { get; }
        public IEventAggregator EventAggregator { get; }
        public ISystemTelemetryChanged SystemTelemetryChangedCallback { get; }
        public IDebugSettings DebugSettings { get; }

        private int _currentOperationalPoint = 0;
        private int _totalOperationPoints = 0;
        private float _energy = 0;
        private float _currentTimerValue = 0.0f;
        private ulong _serial = 0x1234567890ABCDEF;//0;
        private GCBFaultBit _faultBit = GCBFaultBit.Reserved;
        private IList<GcbOperationalPoint> _emissionSteps;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource CancellationTokenSource { 
            get => _cancellationTokenSource; 
            set
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = value;
            }
        }
        private GcbStateNew _prevState;
        private GcbStateNew _state;


        #region ITelemetryService
        
        public void Start()
        {
        }

        public void Stop()
        {
        }
        #endregion ITelemetryService

        public void SetPlan(ICollection<GcbOperationalPoint> gcbOperationalPoints)
        {
            _emissionSteps = new List<GcbOperationalPoint>(gcbOperationalPoints);
            _totalOperationPoints = _emissionSteps.Count();
            for (_currentOperationalPoint = 0; _currentOperationalPoint < _totalOperationPoints; _currentOperationalPoint++)
            {
                if (_emissionSteps[_currentOperationalPoint].RemainingPointTime > 0.1)
                    break;
            }
        }

        private object ParseValue(PropertyInfo property, string value)
        {
            object parsedValue = null;

            if (property.PropertyType == typeof(bool))
            {
                parsedValue = bool.Parse(value);
            }
            else if (property.PropertyType == typeof(float))
            {
                parsedValue = float.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (property.PropertyType == typeof(uint))
            {
                parsedValue = int.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (property.PropertyType == typeof(int))
            {
                parsedValue = int.Parse(value, CultureInfo.InvariantCulture);
            }
            else
            {
                throw new ArgumentException($"Unsupported property type '{property.PropertyType}'");
            }

            return parsedValue;
        }
        private async void OnDummyXrayStatusChangedEvent(DummyXrayStatusChangedEventArgs args)
        {
            try
            {
                CancellationTokenSource.Cancel();
                switch (args.Status)
                {
                    case DummyXrayStatus.Initialize:
                        SetGCBState(GcbStateNew.Cold);
                        break;
                    case DummyXrayStatus.ClearErrors:
                        CancellationTokenSource = new CancellationTokenSource();
                        _faultBit = GCBFaultBit.Reserved;
                        GenerateClearErrorsTelemetry();
                        break;
                    case DummyXrayStatus.WarmingUp:
                        CancellationTokenSource = new CancellationTokenSource();
                        var timeMs = Convert.ToInt32(args.Parameter);
                        GenerateWarmUpTelemetry(timeMs);
                        break;

                    case DummyXrayStatus.Loading:
                        CancellationTokenSource = new CancellationTokenSource();
                        var energy = Convert.ToSingle(args.Parameter);
                        GenerateLoadingTelemetry(energy, 10);
                        break;

                    case DummyXrayStatus.Started:
                        CancellationTokenSource = new CancellationTokenSource();
                        GenerateBeamOnTelemetry(args.Parameter as IList<GcbOperationalPoint>);
                        break;

                    case DummyXrayStatus.Stopped:
                        GenerateStoppedTelemetry(_state);
                        break;
                    case DummyXrayStatus.ClearPlan:
                        _currentOperationalPoint = 0;
                        _totalOperationPoints = 0;
                        _emissionSteps = null;
                        CancellationTokenSource = new CancellationTokenSource();
                        GenerateDischargeTelemetry(Convert.ToInt32(args.Parameter));
                        break;
                    case DummyXrayStatus.Discharging:
                        CancellationTokenSource = new CancellationTokenSource();
                        GenerateDischargeTelemetry(Convert.ToInt32(args.Parameter));
                        break;
                    case DummyXrayStatus.ResetTimers:
                        _currentTimerValue = 0;
                        _energy = 0;
                        GenerateTelemetry((int)_state);
                        break;
                    case DummyXrayStatus.SetWarmupFault:
                        _faultBit = (GCBFaultBit)Convert.ToInt32(args.Parameter);
                        // Send one telemetry step
                        SetGCBState(GcbStateNew.WarmupFault);
                        break;
                    case DummyXrayStatus.SetFault:
                        _faultBit = (GCBFaultBit)Convert.ToInt32(args.Parameter);
                        // Send one telemetry step
                        GenerateTelemetry((int)_state);
                        CancellationTokenSource = new CancellationTokenSource();
                        await Task.Delay(300);
                        GenerateDischargeTelemetry(3000, GcbStateNew.Fault, GcbStateNew.Fault);
                        break;
                    case DummyXrayStatus.Conditioning:
                        CancellationTokenSource = new CancellationTokenSource();
                        var timeMs1 = Convert.ToInt32(args.Parameter);
                        GenerateWarmUpTelemetry(timeMs1, GcbStateNew.DailyWarmup, GcbStateNew.Primed);
                        break;

                    case DummyXrayStatus.SetPlan:
                        SetPlan(args.Parameter as IList<GcbOperationalPoint>);
                        GenerateTelemetry((int)_state);
                        break;

                    case DummyXrayStatus.Unspecified:
                    default:
                        //SetIsReadingFromFile(false);
                        //Start();
                        CancelTokenSource(CancellationTokenSource);
                        SetGCBState(GcbStateNew.Ready);
                        break;
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => LogWriter.Log($"OnDummyXrayStatusChangedEvent error: DummyXrayStatus={args.Status}, {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error));
            }
        }

        private void GenerateWarmUpTelemetry(int warmUpTimeMs, GcbStateNew processState = GcbStateNew.Warmup, GcbStateNew finalState = GcbStateNew.Primed)
        {
            int timeStep = 500;
            int stepsCount = warmUpTimeMs / timeStep;

            SetGCBState(processState);

            var tokenSource = CancellationTokenSource;

            Task.Run(async () =>
            {
                int i = 1;
                for (var duration = 0; duration < warmUpTimeMs; duration += timeStep)
                {
                    if (IsCancellationRequested(tokenSource))
                        return;

                    await Task.Delay(timeStep, tokenSource.Token);

                    float currentValue = i++ * (3750f / stepsCount);

                    var telemetryResponse = BuildBasicTelemetryPackage();
                    telemetryResponse.Set((int)GCBTelemetryResponseField.FilamentSetpoint, 3750f);
                    telemetryResponse.Set((int)GCBTelemetryResponseField.FilamentFeedback, currentValue);
                    telemetryResponse.UpdateCRC();
                    SetTelemetry(SystemTelemetry.Parse(telemetryResponse.Buffer));
                }
                SetGCBState(finalState);
            },
            AppGlobals.AppCancellationTokenSource.Token);
        }

        private UdpPacket BuildBasicTelemetryPackage()
        {
            int interlocks = (int)GcbInterlockFlags.BaseKey;
            var builder = new TelemetryPacket(_state)
                .Set((int)GCBTelemetryResponseField.SystemFaultFlags, 1 << (int)_faultBit)
                .Set((int)GCBTelemetryResponseField.Collimator1, (int)(_serial & 0xffffffff))
                .Set((int)GCBTelemetryResponseField.Collimator2, (int)(_serial >> 32))
                .Set((int)GCBTelemetryResponseField.CurrentPoint, _currentOperationalPoint)
                .Set((int)GCBTelemetryResponseField.TotalPoints, _totalOperationPoints)
                .Set((int)GCBTelemetryResponseField.InternalTimerValue, _currentTimerValue)
                .Set((int)GCBTelemetryResponseField.Timer1Value, _currentTimerValue)
                .Set((int)GCBTelemetryResponseField.Timer2Value, Math.Max(0, _currentTimerValue))
                .Set((int)GCBTelemetryResponseField.kVFeedback, _energy)
                .Set((int)GCBTelemetryResponseField.InterlockFlags, interlocks);

            return builder;
        }

        private void GenerateLoadingTelemetry(float energy, int stepCount)
        {
            var tokenSource = CancellationTokenSource;
            Task.Run(async () =>
            {
                SetGCBState(GcbStateNew.HVSetup);

                for (int i = 0; i < stepCount; i++)
                {
                    if (IsCancellationRequested(tokenSource))
                        return;

                    await Task.Delay(2000 / stepCount, tokenSource.Token);

                    var packet = BuildBasicTelemetryPackage();
                    float kvs = (energy * i) / stepCount;
                    packet.Set((int)GCBTelemetryResponseField.SystemState, (int)_state);
                    packet.Set((int)GCBTelemetryResponseField.kVFeedback, kvs);
                    packet.UpdateCRC();

                    SetTelemetry(SystemTelemetry.Parse(packet.Buffer));
                }

                SetGCBState(GcbStateNew.Ready);
            },
            AppGlobals.AppCancellationTokenSource.Token);
        }

        private void GenerateLaunchingTelemetry(double timerSec, float initialCurrent, float targetHeaterCurrent)
        {
            var tokenSource = CancellationTokenSource;
            Task.Run(async () =>
            {
                SetGCBState(GcbStateNew.Launching);
                const int steps = 10;

                for (int i = 0; i < steps; i++)
                {
                    if (IsCancellationRequested(tokenSource))
                        return;

                    float current = initialCurrent + (targetHeaterCurrent - initialCurrent) * i / steps;

                    var packet = BuildBasicTelemetryPackage();
                    packet.Set((int)GCBTelemetryResponseField.SystemState, (int)_state);
                    // TODO: if launching progress will be the same as the warmup's, with constant setpoint:
                    //packet.Set((int)GCBTelemetryResponseField.FilamentSetpoint, targetHeaterCurrent);
                    packet.Set((int)GCBTelemetryResponseField.FilamentSetpoint, current);
                    packet.Set((int)GCBTelemetryResponseField.FilamentFeedback, current);
                    packet.UpdateCRC();

                    SetTelemetry(SystemTelemetry.Parse(packet.Buffer));

                    //Debug.WriteLine($"DebugTelemetry: Launching point={_currentOperationalPoint} time={_currentTimerValue}");


                    await Task.Delay((int)(timerSec * 1000 / steps), tokenSource.Token);
                }

                SetGCBState(GcbStateNew.Emission);
            },
            AppGlobals.AppCancellationTokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void GenerateBeamOnTelemetry(ICollection<GcbOperationalPoint> emissionSteps)
        {
            var tokenSource = CancellationTokenSource;
            if (emissionSteps == null)
            {
                _faultBit = GCBFaultBit.FilamentFault;
                SetGCBState(GcbStateNew.Fault);
                return;
            }

            if (_emissionSteps == null)
            {
                SetPlan(emissionSteps);
            }

            var timerMs = 299;

            Task.Run(async () =>
            {
                _currentTimerValue = 0;
                _energy = 0;
                while (_currentOperationalPoint < _totalOperationPoints)
                {
                    GcbOperationalPoint step = _emissionSteps[_currentOperationalPoint];
                    _energy = step.SetpointKv;

                    float totalRemainingDuration = step.RemainingPointTime; // if it was interrupted

                    if (totalRemainingDuration < 0.05)
                    {
                        _currentOperationalPoint++;
                        continue;
                    }

                    _currentTimerValue = 0.0f;

                    float heaterCurrent = step.FilamentSetpoint;
                    float initialHeaterCurrent = heaterCurrent / 2.0f;
                    GenerateLaunchingTelemetry(2, initialHeaterCurrent, heaterCurrent);
                    await Task.Delay(timerMs);

                    float energyDeflection = 0.0f;

                    Func<float, float> calculateEnergyDeflection = (timerValue) => ((int)timerValue % 2 == 0) ? 0.1f * timerValue : -1.0f * 0.1f * timerValue;

                    _currentTimerValue = timerMs / 1000.0f;
                    while (_currentTimerValue <= totalRemainingDuration)
                    {

                        if (IsCancellationRequested(tokenSource))
                        {
                            _emissionSteps[_currentOperationalPoint] = step;
                            return;
                        }

                        energyDeflection = calculateEnergyDeflection(_currentTimerValue);
                        
                        //for debug: generate Interlock fault
                        //_faultBit = GCBFaultBit.InterlockFault;
                        //GcbStateNew st = GcbStateNew.Fault;
                        //SetGCBState(st);
                        //return;

                        GenerateEmissionTelemetry(1.23f + _currentTimerValue / 100.0f, _energy + energyDeflection);

                        _currentTimerValue += timerMs / 1000.0f;
                        step.RemainingPointTime = totalRemainingDuration - _currentTimerValue;
                        await Task.Delay(timerMs);

                        //// Temp: go to fault state during the emission
                        //await GenerateFaultDischargingTelemetry();
                        //return;
                    }

                    energyDeflection = calculateEnergyDeflection(_currentTimerValue);

                    // Finalize with total duration:
                    _currentTimerValue = totalRemainingDuration - 0.01f; // to imitate actual situation on the board with underexposure of .01 sec
                    GenerateEmissionTelemetry(1.23f + _currentTimerValue / 100.0f, _energy + energyDeflection);

                    _currentOperationalPoint++;
                    if (!step.AutoExecution)
                    {
                        break;
                    }
                }

                //_energy = 0;
                if (_totalOperationPoints > 0
                    && _currentOperationalPoint < _totalOperationPoints
                    && _emissionSteps.Last().AutoExecution == false)
                {
                    _currentTimerValue = 0; // reset timer to show proper next point telemetry
                    // go back to ready state
                    GenerateAfterEmissionTelemetry((int)GcbStateNew.Ready, _currentOperationalPoint);
                }
                else
                {
                    await GenerateTerminationTelemetry();
                    GenerateAfterEmissionTelemetry((int)GcbStateNew.Staged, _currentOperationalPoint);
                }
            },
            AppGlobals.AppCancellationTokenSource.Token);
        }

        private async Task GenerateTerminationTelemetry()
        {
            SetGCBState(GcbStateNew.Termination);
            int steps = 50;
            float energyStep = _energy / steps;
            for (int step = 0; step < steps; step++)
            {
                await Task.Delay(50);
                _energy = Math.Max(0, _energy - energyStep);
                GenerateTelemetry((int)_state);
            }
        }

        private async Task GenerateStartupTelemetry()
        {
            SetTelemetry(null!);

            await Task.Delay(3000);

            var packet = BuildBasicTelemetryPackage();
            packet.Set((int)GCBTelemetryResponseField.SystemState, (int)GcbStateNew.Startup);
            packet.Set((int) GCBTelemetryResponseField.InternalTimerValue, (float) 0.0f);
            packet.Set((int) GCBTelemetryResponseField.Timer1Value, (float) 0.0f);
            packet.Set((int) GCBTelemetryResponseField.Timer2Value, (float) 0.0f);
            
            packet.UpdateCRC();

            SetTelemetry(SystemTelemetry.Parse(packet.Buffer));
        }

        private async Task GenerateFaultDischargingTelemetry()
        {
            _faultBit = GCBFaultBit.CoolantFault;
            SetGCBState(GcbStateNew.Fault);

            int steps = 50;
            float energyStep = _energy / (float)steps;
            for (int step = 0; step < steps; step++)
            {
                await Task.Delay(50);
                _energy = Math.Max(0, _energy - energyStep);
                GenerateTelemetry((int)_state);
            }
        }

        private void GenerateStoppedTelemetry(GcbStateNew prevState)
        {
            CancellationTokenSource.Cancel();

            if (prevState < GcbStateNew.Primed)
            {
                SetGCBState(GcbStateNew.Cold);
                return;
            }
            else if (prevState <= GcbStateNew.Staged)
            {
                SetGCBState(GcbStateNew.Primed);
                return;
            }
            
            // Otherwise, we go to Staged through Termination:
            Task.Run(async () =>
            {
                SetGCBState(GcbStateNew.Termination);
                for (int step = 0; step < 3; ++step)
                {
                    await Task.Delay(400);
                    GenerateTelemetry((int)_state);
                }
                await Task.Delay(400);
                SetGCBState(GcbStateNew.Staged);
            });
        }

        private void GenerateClearErrorsTelemetry()
        {
            // Now we don't have auto-warmup, so on every fault we go to Cold
            SetGCBState(GcbStateNew.Cold);
            //if (_prevState < GcbStateNew.Primed
            //    || _prevState > GcbStateNew.Termination)
            //{
            //    SetGCBState(GcbStateNew.Cold);
            //}
            //else if (_prevState == GcbStateNew.Primed ||
            //        _prevState == GcbStateNew.Staging )
            //{
            //    SetGCBState(GcbStateNew.Primed);
            //}
            //else
            //{
            //    GenerateWarmUpTelemetry(2000, finalState: GcbStateNew.Staged);
            //}
        }

        private void GenerateDischargeTelemetry(
            int delay, 
            GcbStateNew state = GcbStateNew.Discharge,
            GcbStateNew finalState = GcbStateNew.Cold)
        {
            var tokenSource = CancellationTokenSource;
            Task.Run(async () =>
            {
                const int steps = 10;
                float kvStep = _energy / steps;

                await Task.Delay(delay / steps, tokenSource.Token);
                SetGCBState(state);

                for (int i = 0; i < steps; i++)
                {
                    _energy -= kvStep;
                    if (IsCancellationRequested(tokenSource))
                        return;

                    await Task.Delay(delay / steps, tokenSource.Token);
                    GenerateTelemetry((int)state);
                }

                if (_state != finalState)
                    SetGCBState(finalState);
            },
            AppGlobals.AppCancellationTokenSource.Token);
        }

        private void CancelTokenSource(CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource.Cancel();
        }

        private bool IsCancellationRequested(CancellationTokenSource cancellationTokenSource)
        {
            return cancellationTokenSource.IsCancellationRequested;
        }

        private void SetTelemetry(ISystemTelemetry? telemetry)
        {
            if (telemetry != null)
            {
                telemetry.CollimatorSerial = _serial;
            }

            SystemTelemetryChangedCallback.OnSystemTelemetryChanged(telemetry);
        }

        private void SetGCBState(GcbStateNew state)
        {
            _prevState = _state;
            _state = state;
            int faultFlags = (_faultBit == 0) ? 0 : 1 << (int)_faultBit;

            var packet = BuildBasicTelemetryPackage();
            packet.Set((int)GCBTelemetryResponseField.SystemFaultFlags, faultFlags);
            packet.UpdateCRC();

            SetTelemetry(SystemTelemetry.Parse(packet.Buffer));
        }

        private void GenerateTelemetry(int gcbState)
        {
            var packet = BuildBasicTelemetryPackage();
            packet.Set((int)GCBTelemetryResponseField.SystemState, gcbState);
            packet.UpdateCRC();

            SetTelemetry(SystemTelemetry.Parse(packet.Buffer));
        }

        private void GenerateEmissionTelemetry(float emissionCurrent, float energy)
        {
            var packet = BuildBasicTelemetryPackage();
            packet.Set((int)GCBTelemetryResponseField.SystemState, (int)GcbStateNew.Emission);
            packet.Set((int)GCBTelemetryResponseField.FilamentFeedback, emissionCurrent);
            packet.Set((int)GCBTelemetryResponseField.kVFeedback, energy);

            packet.UpdateCRC();
            //Debug.WriteLine($"DebugTelemetry: Emission point={_currentOperationalPoint} time={_currentTimerValue}");

            SetTelemetry(SystemTelemetry.Parse(packet.Buffer));
        }

        private void GenerateAfterEmissionTelemetry(int gcbState, int nextPointIndex)
        {
            var packet = BuildBasicTelemetryPackage();
            packet.Set((int)GCBTelemetryResponseField.SystemState, gcbState);
            packet.Set((int)GCBTelemetryResponseField.CurrentPoint, nextPointIndex);
            packet.UpdateCRC();

            SetTelemetry(SystemTelemetry.Parse(packet.Buffer));
            //Debug.WriteLine($"DebugTelemetry: Emission point={_currentOperationalPoint} time={_currentTimerValue}");
        }

        public void Dispose()
        {
        }
    }
}
