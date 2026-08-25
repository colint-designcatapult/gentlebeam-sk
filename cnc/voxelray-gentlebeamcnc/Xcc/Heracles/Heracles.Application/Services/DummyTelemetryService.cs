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
            IDecodedTelemetryFrameSink decodedTelemetryFrameSink,
            IDebugSettings debugSettings)
        {
            LogWriter = logWriter;
            AppGlobals = appGlobals;
            EventAggregator = eventAggregator;
            SystemTelemetryChangedCallback = systemTelemetryChangedCallback;
            DecodedTelemetryFrameSink = decodedTelemetryFrameSink;
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
        public IDecodedTelemetryFrameSink DecodedTelemetryFrameSink { get; }
        public IDebugSettings DebugSettings { get; }

        private int _currentOperationalPoint = 0;
        private int _totalOperationPoints = 0;
        private float _energy = 0;
        private float _currentTimerValue = 0.0f;
        private ulong _serial = 0x1234567890ABCDEF;//0;
        private SystemFault _faultBit = SystemFault.Reserved;
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
                        _faultBit = SystemFault.Reserved;
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
                        _faultBit = (SystemFault)Convert.ToInt32(args.Parameter);
                        // Send one telemetry step
                        SetGCBState(GcbStateNew.WarmupFault);
                        break;
                    case DummyXrayStatus.SetFault:
                        _faultBit = (SystemFault)Convert.ToInt32(args.Parameter);
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

                    SetTelemetry(BuildBasicTelemetry(
                        heaterCurrentSetpoint: 3750f,
                        heaterCurrentFeedback: currentValue));
                }
                SetGCBState(finalState);
            },
            AppGlobals.AppCancellationTokenSource.Token);
        }

        private SystemNormalTelemetry BuildBasicTelemetry(
            GcbStateNew? state = null,
            int? currentOperationalPoint = null,
            float? primaryTimerValue = null,
            float? secondaryTimer1Value = null,
            float? secondaryTimer2Value = null,
            float? kvFeedback = null,
            float? heaterCurrentSetpoint = null,
            float? heaterCurrentFeedback = null)
        {
            const ulong availableFaults = (1UL << 24) - 2;
            const ulong availableInterlocks = (uint)GcbInterlockFlags.All;

            var rawFaults = _faultBit == SystemFault.Reserved ? 0u : 1u << (int)_faultBit;
            var activeFaults = _faultBit == SystemFault.Reserved ? 0UL : 1UL << (int)_faultBit;
            const uint rawInterlocks = 1u << 19;
            const uint rawRequiredInterlocks = rawInterlocks;
            const ulong activeInterlocks = 1UL << (int)SystemInterlock.BaseKeyOn;
            const ulong requiredInterlocks = activeInterlocks;

            return new SystemNormalTelemetry
            {
                ControlBoardState = state ?? _state,
                Faults = new SystemFaults(rawFaults, null, activeFaults, availableFaults),
                Interlocks = new SystemInterlocks(
                    rawInterlocks,
                    rawRequiredInterlocks,
                    activeInterlocks,
                    availableInterlocks,
                    requiredInterlocks),
                RingLedState = RingLedState.TBD,
                BaseLedState = BaseLedState.TBD,
                CollimatorId1 = (uint)(_serial & 0xffffffff),
                CollimatorId2 = (uint)(_serial >> 32),
                CollimatorSerial = _serial,
                CurrentOperationalPoint = currentOperationalPoint ?? _currentOperationalPoint,
                TotalOperationalPoints = _totalOperationPoints,
                PrimaryTimerValue = primaryTimerValue ?? _currentTimerValue,
                SecondaryTimer1Value = secondaryTimer1Value ?? _currentTimerValue,
                SecondaryTimer2Value = secondaryTimer2Value ?? Math.Max(0, _currentTimerValue),
                Hvps = new HvpsTelemetryStatus(0, 0, null),
                KvSetpoint = 0,
                KvFeedback = kvFeedback ?? _energy,
                HeaterCurrentSetpoint = heaterCurrentSetpoint ?? 0,
                HeaterCurrentFeedback = heaterCurrentFeedback ?? 0,
                EmissionCurrentLimit = 0,
                HvpsPowerSetpoint = 0,
                GridSetpoint = 0,
                Mag1 = new TelemetryVector3(),
                Mag2 = new TelemetryVector3(),
            };
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

                    float kvs = (energy * i) / stepCount;
                    SetTelemetry(BuildBasicTelemetry(
                        state: _state,
                        kvFeedback: kvs));
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

                    SetTelemetry(BuildBasicTelemetry(
                        state: _state,
                        heaterCurrentSetpoint: current,
                        heaterCurrentFeedback: current));

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
                _faultBit = SystemFault.FilamentFault;
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

            SetTelemetry(BuildBasicTelemetry(
                state: GcbStateNew.Startup,
                primaryTimerValue: 0,
                secondaryTimer1Value: 0,
                secondaryTimer2Value: 0));
        }

        private async Task GenerateFaultDischargingTelemetry()
        {
            _faultBit = SystemFault.CoolantFault;
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
            if (telemetry is not null && DecodedTelemetryFrameSink.IsEnabled)
            {
                DecodedTelemetryFrameSink.Publish(
                    new DecodedTelemetryFrame(
                        DateTimeOffset.UtcNow,
                        telemetry,
                        ReadOnlyMemory<byte>.Empty));
            }

            SystemTelemetryChangedCallback.OnSystemTelemetryChanged(telemetry);
        }

        private void SetGCBState(GcbStateNew state)
        {
            _prevState = _state;
            _state = state;
            SetTelemetry(BuildBasicTelemetry(state: state));
        }

        private void GenerateTelemetry(int gcbState)
        {
            SetTelemetry(BuildBasicTelemetry(state: (GcbStateNew)gcbState));
        }

        private void GenerateEmissionTelemetry(float emissionCurrent, float energy)
        {
            SetTelemetry(BuildBasicTelemetry(
                state: GcbStateNew.Emission,
                kvFeedback: energy,
                heaterCurrentFeedback: emissionCurrent));
        }

        private void GenerateAfterEmissionTelemetry(int gcbState, int nextPointIndex)
        {
            SetTelemetry(BuildBasicTelemetry(
                state: (GcbStateNew)gcbState,
                currentOperationalPoint: nextPointIndex));
            //Debug.WriteLine($"DebugTelemetry: Emission point={_currentOperationalPoint} time={_currentTimerValue}");
        }

        public void Dispose()
        {
        }
    }
}
