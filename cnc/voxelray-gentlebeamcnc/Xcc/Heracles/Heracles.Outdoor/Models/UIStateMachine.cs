using System.ComponentModel;
using Heracles.Application.Enums;
using Prism.Mvvm;
using Xcc.Application.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.External.Models
{
    public enum UIMacroState : int
    {
        StandBy = 0,    // Plan is not loaded, waiting for inputs
        Preparation,    // Loading plan for treatment
        Emission,       // Running emission loop
        ResumePlan      // Staged after emission fault or preparation stop (plan is loaded)
    }

    public enum UILeftButtonState : int
    {
        Prepare = 0,
        WarmUpProgress,
        LoadingProgress,
        ClearPlan
    }

    public enum UICentralButtonState : int
    {
        BeamOn = 0,
        EmissionProgress,
        ClearErrors,
        ResetTimers
    }

    public enum UIRightButtonState : int
    {
        Stop = 0,
        Resume
    }

    public class LeftButtonInfo : ButtonInfo
    {
        public UILeftButtonState State { get; set; }
    }

    public class CentralButtonInfo : ButtonInfo
    {
        public UICentralButtonState State { get; set; }
    }

    public class RightButtonInfo : ButtonInfo
    {
        public UIRightButtonState State { get; set; }
    }

    public interface IUIStateMachine : INotifyPropertyChanged
    {
        public UIMacroState State { get; }
        public LeftButtonInfo LeftButton { get; }
        public CentralButtonInfo CentralButton { get; }
        public RightButtonInfo RightButton { get; }

        // Temporary property, to not depend on plan model to check it
        bool IsPlanStaged { get; set; }
        bool IsPlanLoadedForTreatment { get; set; }
        ExternalTabName TabName { get; set; }

        void RequestStateSwitch(UIMacroState newState);
        void OnGcbStateChange(GcbStateNew gcbState);
        void OnReboot();
    }

    public class UIStateMachine : BindableBase, IUIStateMachine
    {
        private LeftButtonInfo _leftButton = null;
        private CentralButtonInfo _centralButton = null;
        private RightButtonInfo _rightButton = null;
        private bool _isPlanLoaded = false;
        private UIMacroState _prevState = UIMacroState.StandBy;
        private bool _isPlanLoadedForTreatment;
        private UIMacroState _state = UIMacroState.StandBy;

        public UIMacroState State { get => _state; private set => SetProperty(ref _state, value); }
        public GcbStateNew LastGcbState { get; private set; }

        public LeftButtonInfo LeftButton { get => _leftButton; private set => SetProperty(ref _leftButton, value); }
        public CentralButtonInfo CentralButton { get => _centralButton; private set => SetProperty(ref _centralButton, value); }
        public RightButtonInfo RightButton { get => _rightButton; private set => SetProperty(ref _rightButton, value); }        
        public bool IsPlanStaged { get => _isPlanLoaded; set => SetProperty(ref _isPlanLoaded, value); }
        public bool IsPlanLoadedForTreatment { get => _isPlanLoadedForTreatment; set => SetProperty(ref _isPlanLoadedForTreatment, value); }

        private ExternalTabName _tabName;

        public ExternalTabName TabName
        {
            get => _tabName;
            set => SetProperty(ref _tabName, value);
        }

        public ILogWriter LogWriter { get; }

        public UIStateMachine(ILogWriter logWriter)
        {
            // Initialize buttons with StandBy in Startup state by default to have them consistent
            OnGcbStateChange(GcbStateNew.Startup);
            LogWriter = logWriter;
        }

        public void RequestStateSwitch(UIMacroState newState)
        {
            bool canTransit = true;
            // Verify if we can do this transition:
            switch (newState)
            {
                case UIMacroState.StandBy: // Ok, we can clear the plan from any step
                    break;
                case UIMacroState.Preparation: // For now, from StandBy/Preparation/Resume, not from Emission
                    if (State == UIMacroState.Emission)
                    {
                        canTransit = false;
                    }
                    break;
                //case UIMacroState.Setup:
                //    if (State == UIMacroState.StandBy)
                //    {
                //        canTransit = false;
                //    }
                //    break;
                case UIMacroState.Emission: // Can't be done from standby now
                    if (State == UIMacroState.StandBy)
                    {
                        canTransit = false;
                    }
                    break;
                case UIMacroState.ResumePlan: // From emission/preparation only
                    if (State != UIMacroState.Emission && State != UIMacroState.Preparation)
                    {
                        canTransit = false;
                    }
                    break;
            }

            if (canTransit)
            {
                _ = LogWriter.LogAsync($"Switch UI state machine from {State} to {newState}", LogRecordSeverity.Info, LogRecordType.System);
                if (State != newState)
                {
                    _prevState = State;
                }
                State = newState;
                OnGcbStateChange(LastGcbState);
            }
            else
            {
                _ = LogWriter.LogAsync($"Invalid UI state switch request from {State} to {newState}", LogRecordSeverity.Info, LogRecordType.System);
                throw new InvalidOperationException("Invalid system state transition requested");
            }
        }

        public void OnGcbStateChange(GcbStateNew gcbState)
        {
            switch (State)
            {
                case UIMacroState.StandBy:
                    UpdateInStandBy(gcbState);
                    break;
                case UIMacroState.Preparation:
                    UpdateInPreparation(gcbState);
                    break;
                case UIMacroState.Emission:
                    UpdateInEmission(gcbState);
                    break;
                case UIMacroState.ResumePlan:
                    UpdateInResumePlan(gcbState);
                    break;
            }
            // For the stop button, it's always almost the same
            UpdateRightButton(gcbState);

            LastGcbState = gcbState;
        }

        public void OnReboot()
        {
            State = UIMacroState.StandBy;
            IsPlanLoadedForTreatment = false;
            OnGcbStateChange(LastGcbState);
        }

        private void UpdateRightButton(GcbStateNew gcbState)
        {
            // Right button ('Stop') can stop ongoing processes only,
            // and becomes 'Resume' for Staged GCBState with ResumePlan state machine macrostate
            switch (gcbState)
            {
                case GcbStateNew.DailyWarmup:
                case GcbStateNew.Warmup:
                case GcbStateNew.HvpsCheck:
                case GcbStateNew.HVSetup:
                case GcbStateNew.Staging:
                case GcbStateNew.Launching:
                case GcbStateNew.Emission:
                    RightButton = new RightButtonInfo { IsEnabled = true, State = UIRightButtonState.Stop };
                    break;
                case GcbStateNew.Primed: // for preparation loop only, to not blink through staging loop
                case GcbStateNew.Staged: // for preparation loop only, to not blink up to HVSetup:
                case GcbStateNew.StandBy:
                case GcbStateNew.Cold:
                    RightButton = new RightButtonInfo
                    {
                        IsEnabled = State == UIMacroState.ResumePlan,
                        State = (State == UIMacroState.ResumePlan) ? UIRightButtonState.Resume : UIRightButtonState.Stop
                    };
                    break;
                case GcbStateNew.Ready: // for emission loop only, to not blink on the transitive Ready state
                    RightButton = new RightButtonInfo { IsEnabled = State == UIMacroState.Emission, State = UIRightButtonState.Stop };
                    break;
                default:
                    RightButton = new RightButtonInfo { IsEnabled = false, State = UIRightButtonState.Stop };
                    break;
            }
        }

        private void UpdateInStandBy(GcbStateNew gcbState)
        {
            // Left button is the same in StandBy
            switch (gcbState)
            {
                case GcbStateNew.Ready:
                case GcbStateNew.Staged:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = true };
                    break;
                case GcbStateNew.Warmup:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.WarmUpProgress, IsEnabled = true };
                    break;
                case GcbStateNew.NoComm:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.Prepare, IsEnabled = false };
                    break;
                default:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.Prepare, IsEnabled = true };
                    break;
            }
            // Central button
            switch (gcbState)
            {
                case GcbStateNew.Cold:
                case GcbStateNew.StandBy:
                case GcbStateNew.Staged:
                case GcbStateNew.Warmup:
                case GcbStateNew.Discharge:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ResetTimers, IsEnabled = true };
                    break;
                case GcbStateNew.Fault:
                case GcbStateNew.ColdFault:
                case GcbStateNew.WarmupFault:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ClearErrors, IsEnabled = true };
                    break;
                case GcbStateNew.NoComm:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ClearErrors, IsEnabled = false };
                    break;
                default:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.BeamOn, IsEnabled = false };
                    break;
            }
        }

        private void UpdateInPreparation(GcbStateNew gcbState)
        {
            // Left button:
            switch (gcbState)
            {
                case GcbStateNew.Cold:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.Prepare, IsEnabled = true };
                    break;
                case GcbStateNew.Warmup:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.WarmUpProgress, IsEnabled = true };
                    break;
                case GcbStateNew.Primed:
                case GcbStateNew.StandBy:
                case GcbStateNew.Staging:
                case GcbStateNew.HvpsCheck:
                case GcbStateNew.HVSetup:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.LoadingProgress, IsEnabled = true };
                    break;
                case GcbStateNew.Ready:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = true };
                    break;
                case GcbStateNew.Termination: // going to staged after Stop
                    if (IsPlanStaged)
                        LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = false };
                    else
                        LeftButton = new LeftButtonInfo { State = UILeftButtonState.Prepare, IsEnabled = false };
                    break;
                case GcbStateNew.Staged:
                case GcbStateNew.NoComm:
                    if (IsPlanStaged)
                        LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = true };
                    else
                        LeftButton = new LeftButtonInfo { State = UILeftButtonState.LoadingProgress, IsEnabled = true };
                    break;
                default:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.Prepare, IsEnabled = false };
                    break;
            }

            // Central button:
            switch (gcbState)
            {
                case GcbStateNew.ColdFault:
                case GcbStateNew.WarmupFault:
                case GcbStateNew.Fault:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ClearErrors, IsEnabled = true };
                    break;
                case GcbStateNew.NoComm:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ClearErrors, IsEnabled = false };
                    break;
                default:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.BeamOn, IsEnabled = gcbState == GcbStateNew.Ready };
                    break;
            }
        }

        private void UpdateInEmission(GcbStateNew gcbState)
        {
            // Left button:
            switch (gcbState)
            {
                case GcbStateNew.Warmup:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.WarmUpProgress, IsEnabled = true };
                    break;
                case GcbStateNew.Cold:
                case GcbStateNew.StandBy:
                case GcbStateNew.Staged:
                case GcbStateNew.Primed: // we should not get into Primed from emission, but just in case
                    // TODO: this state's IsEnabled should depend on timers state (enable only after timer reset)
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = true };
                    break;
                default:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = false };
                    break;
            }

            // Central button:
            switch (gcbState)
            {
                case GcbStateNew.Cold:
                case GcbStateNew.StandBy:
                case GcbStateNew.Staged:
                case GcbStateNew.Primed: // we should not get into Primed from emission, but just in case
                    // TODO: should get disabled after timers get reset
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ResetTimers, IsEnabled = true };
                    break;
                case GcbStateNew.HvpsCheck:
                case GcbStateNew.HVSetup:
                case GcbStateNew.Ready:
                case GcbStateNew.Launching:
                case GcbStateNew.Emission:
                case GcbStateNew.Termination:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.EmissionProgress, IsEnabled = true };
                    break;
                case GcbStateNew.Fault:
                case GcbStateNew.ColdFault:
                case GcbStateNew.WarmupFault:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ClearErrors, IsEnabled = true };
                    break;
                default:
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ClearErrors, IsEnabled = false };
                    break;
            }
        }

        private void UpdateInResumePlan(GcbStateNew gcbState)
        {
            // It is different in the Staged state only,
            // as we need an additional Resume button here:
            switch (gcbState)
            {
                case GcbStateNew.Cold:
                case GcbStateNew.StandBy:
                case GcbStateNew.Staged:
                    LeftButton = new LeftButtonInfo { State = UILeftButtonState.ClearPlan, IsEnabled = true };
                    CentralButton = new CentralButtonInfo { State = UICentralButtonState.ResetTimers, IsEnabled = true };
                    break;
                default:
                    // If we're in any other state, just use the previos macrostate
                    // (we actually need its states to be able to clear the plan and errors)
                    if (_prevState == UIMacroState.Preparation || gcbState == GcbStateNew.Ready || gcbState == GcbStateNew.NoComm) {
                        UpdateInPreparation(gcbState);
                    }
                    else {
                        UpdateInEmission(gcbState);
                    }
                    break;
            }
        }
    }
}
