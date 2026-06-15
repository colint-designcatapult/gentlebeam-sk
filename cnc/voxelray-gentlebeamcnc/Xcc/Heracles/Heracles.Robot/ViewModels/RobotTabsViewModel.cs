using Heracles.Application.Services;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Interlock;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Heracles.Robot.Services;

using Prism.Events;
using Prism.Regions;

using System;
using System.Collections.ObjectModel;

using Xcc.Application.UI.Mvvm;

namespace Heracles.Robot.ViewModels
{
    public class LogEvent : PubSubEvent<string> { }

    public class RobotTabsViewModel : RegionViewModelBase
    {
        #region Properties
        IRobotArmService _robotArmService;
        IEventAggregator _eventAggregator;
        Models.Interlock.IInterlockService _interlockService;
        IAcbService _acbService;
        private ObservableCollection<string> _robotLog = new();
        private Status _status;
        private Heracles.Robot.Models.Interlock.State _interlockState;


        public ObservableCollection<string> RobotLog { get => _robotLog; set => _robotLog = value; }

        public Status Status { get => _status; private set => SetProperty(ref _status, value); }
        public Heracles.Robot.Models.Interlock.State InterlockState { get => _interlockState; private set => SetProperty(ref _interlockState, value); }
        private string _acbState = string.Empty;
        public string AcbState { get => _acbState; private set => SetProperty(ref _acbState, value); }
        private string _hardwareInfo = string.Empty;
        public string HardwareInfo { get => _hardwareInfo; private set => SetProperty(ref _hardwareInfo, value); }

        #endregion Properties


        #region Private Methods
        private void _robotArmService_MotionActionFeedback(object sender, MotionActionResponse e)
        {
            Log("MotionFeedback " + e.ToString());
        }

        private void _robotArmService_PingActionFeedback(object sender, PingActionResponse e)
        {
            Log("PingFeedback " + e.ToString());
        }
        private void _robotArmService_SetOperatingModeActionFeedback(object sender, SetOperatingModeActionResponse e)
        {
            Log("SetOperatingModeFeedback " + e.ToString());
        }

        private void UpdateRobotIsFakeState(bool? isFakeHardware)
        {
            string statusString = isFakeHardware switch
            {
                false => "Real robot",
                true => "Virtual robot",
                null => "Unknown"
            };
            HardwareInfo = $"Hardware type: {statusString}";
        }

        private void _robotArmService_StatusFeedback(object sender, Status e)
        {
            if (Status != e)
            {
                Status = e;
                bool? isFake = _robotArmService.IsFakeHardware();
                UpdateRobotIsFakeState(isFake);
            }
            //Log("StatusFeedback " + e.ToString());
        }
        private void _interlockService_StateChanged(object sender, State e)
        {
            InterlockState = e;
        }
        private void Log(string message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(
                () => RobotLog.Add(DateTime.Now.TimeOfDay.ToString() + " " + message)
            );
        }
        #endregion Private Methods


        #region Constructors
        public RobotTabsViewModel() : base(null)
        {

        }

        public RobotTabsViewModel(IRegionManager regionManager, IRobotArmService robotArmService, IEventAggregator eventAggregator, Models.Interlock.IInterlockService interlockService, IAcbService acbService) : base(regionManager)
        {
            _eventAggregator = eventAggregator;
            _robotArmService = robotArmService;
            _interlockService = interlockService;
            _robotArmService.PingActionFeedback += _robotArmService_PingActionFeedback;
            _robotArmService.MotionActionFeedback += _robotArmService_MotionActionFeedback;
            _robotArmService.SetOperatingModeActionFeedback += _robotArmService_SetOperatingModeActionFeedback;
            _robotArmService.StatusFeedback += _robotArmService_StatusFeedback;

            InterlockState = _interlockService.State;
            _interlockService.StateChanged += _interlockService_StateChanged;
            _eventAggregator.GetEvent<LogEvent>().Subscribe(Log);
            _acbService = acbService;
            _acbService.Updated += (s, e) =>
            {
                AcbState = "Actuators: " + ActuatorTypesConverter.AcbStateToString(_acbService.RobotActuator, _acbService.ImageActuator, _acbService.TreatmentActuator);
            };
            UpdateRobotIsFakeState(null);
        }
        #endregion Constructors

    }
}
