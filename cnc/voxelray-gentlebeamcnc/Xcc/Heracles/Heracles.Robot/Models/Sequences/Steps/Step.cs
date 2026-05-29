using Heracles.Application.Services;
using Heracles.Core.Models;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Heracles.Robot.Models.RobotArm;
using Heracles.Robot.Models.RobotArm.Enums;
using Heracles.Robot.Models.RobotArm.Interfaces;
using Heracles.Robot.Services;
using Xcc.Application.Common;
using Xcc.Application.Models.RobotArm.Enums;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Sequences.Steps
{
    public class Step : IStep
    {
        private delegate bool ActionDelegate(IList<string> actionValues);
        IList<string> _actuatorsPrecondition;
        IList<string> _actionValues;
        string _actionKey;
        Dictionary<string, ActionDelegate> _delegateDictionary;

        public string Name { get; private set; }
        public string Id { get; private set; }
        public string NextIdIfOk { get; private set; }
        public string NextIdIfFailed { get; private set; }

        IRobotArmService _robotArmService;
        IPositionsPresetsMonitor _positionsPresetsMonitor;
        ILogRepository _logWriter;
        IAcbService _acbService;
        IDialogService _dialogService;
        IHeraclesMainSettings _heraclesMainSettings;

        public Step(string id, string nextIdIfOk, string nextIdIfFailed, string action, IList<string> actionValues, IList<string> actuatorsPrecondition, IRobotArmService robotArmService, IPositionsPresetsMonitor positionsPresetsMonitor, ILogRepository logWriter, IAcbService acbService, IDialogService dialogService, IHeraclesMainSettings heraclesMainSettings)
        {
            Name = action;
            foreach (var actionValue in actionValues)
            {
                Name += " " + actionValue;
            }
            Name += " [" + (id != null ? id : string.Empty) + "]";
            Id = id;
            NextIdIfOk = nextIdIfOk;
            NextIdIfFailed = nextIdIfFailed;

            _actuatorsPrecondition = actuatorsPrecondition;
            _actionValues = actionValues;
            _actionKey = action;
            _delegateDictionary = new Dictionary<string, ActionDelegate>();
            _robotArmService = robotArmService;// Prism.Ioc.ContainerLocator.Container.Resolve(typeof(IRobotArmService)) as IRobotArmService;
            _positionsPresetsMonitor = positionsPresetsMonitor;
            _logWriter = logWriter;
            _acbService = acbService;
            _dialogService = dialogService;
            _heraclesMainSettings = heraclesMainSettings;

            switch (action)
            {
                case StepName.Translate:
                    _delegateDictionary.Add(action, Translate);
                    break;
                case StepName.Rotate:
                    _delegateDictionary.Add(action, Rotate);
                    break;
                case StepName.SetOperatingMode:
                    _delegateDictionary.Add(action, SetOperatingMode);
                    break;
                case StepName.HeadAction:
                    _delegateDictionary.Add(action, HeadAction);
                    break;
                case StepName.UserConfirmation:
                    _delegateDictionary.Add(action, UserConfirmation);
                    break;
                case StepName.CheckZone:
                    _delegateDictionary.Add(action, CheckZone);
                    break;
                case StepName.UpdateZoneTreshold:
                    _delegateDictionary.Add(action, UpdateZoneTreshold);
                    break;
                case StepName.MoveCustom:
                    _delegateDictionary.Add(action, MoveCustom);
                    break;
                case StepName.CheckActuators:
                    _delegateDictionary.Add(action, CheckActuators);
                    break;
                default:
                    throw new Exception("Invalid action " + action);
            }
        }
        public bool CheckActuatorsPrecondition()
        {
            bool result = _CheckActuatorsPrecondition(_actuatorsPrecondition);
            if (!result)
            {
                string message = "Operation execution stopped. Actuators state required: ";
                if (_actuatorsPrecondition is not null)
                {
                    message += string.Join(", ", _actuatorsPrecondition);
                }
                string caption = "Error";

                _dialogService.Report(
                    caption,
                    message,
                    ReportType.Error,
                    result => { });
            }
            return result;
        }
        public bool Do()
        {
            if (!_CheckActuatorsPrecondition(_actuatorsPrecondition))
            {
                return false;
            }
            var action = _delegateDictionary[_actionKey];
            return action(_actionValues);
        }
        public bool Undo()
        {
            return false;
        }

        public void Reset() { }

        private bool Translate(IList<string> actionValues)
        {
            try
            {
                Axis axis = RobotTypesConverter.AxisFromString(actionValues[0]);
                float value = float.Parse(actionValues[1]);
                return _robotArmService.TranslateAction(axis, value, CoordinateSystem.RobotFrame);
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }

        private bool Rotate(IList<string> actionValues)
        {
            try
            {
                Axis axis = RobotTypesConverter.AxisFromString(actionValues[0]);
                float value = float.Parse(actionValues[1]);
                return _robotArmService.RotateAction(axis, value, CoordinateSystem.RobotFrame);
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }
        private bool SetOperatingMode(IList<string> actionValues)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Info, LogRecordType.System);
            try
            {
                OperatingMode operatingMode = RobotTypesConverter.OperatingModeFromString(actionValues[0]);
                return _robotArmService.SetOperatingMode(operatingMode);
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }
        private bool HeadAction(IList<string> actionValues)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Info, LogRecordType.System);

            try
            {
                if (actionValues.Count >= 2 && actionValues[0] != null && actionValues[1] != null)
                {
                    string actuator = actionValues[0].Trim();
                    string command = actionValues[1].Trim();

                    var act = ActuatorTypesConverter.AcbActuatorIdFromString(actuator);
                    var cmd = ActuatorTypesConverter.AcbActuatorCommandFromString(command);

                    bool result = false;
                    while (result == false)
                    {
                        result = _acbService.SendCommand(act, cmd).GetAwaiter().GetResult();
                        if (result == false)
                        {
                            //if (_robotArmService.Status != Status.Activated)
                            //{
                            //    return result;
                            //}

                            bool R = false;
                            string message = "An error occurred while executing the command. Press OK to retry.";
                            string caption = "Confirm Actuators Action";

                            _dialogService.Report(
                                caption,
                                message,
                                ReportType.Confirmation,
                                result =>
                                {
                                    R = result.Result == ButtonResult.OK;
                                });

                            if (R == false)
                            {
                                return result;
                            }
                        }
                    };

                    return result;
                }
                else
                {
                    _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: The actionValues array must contain 2 elements, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }
        private bool CheckActuators(IList<string> actionValues)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Info, LogRecordType.System);

            try
            {
                if (actionValues.Count >= 3 && actionValues[0] != null && actionValues[1] != null && actionValues[2] != null)
                {
                    var flange = actionValues[0].Split("=");    // -> Flange = lock / unlock
                    var cradle = actionValues[1].Split("=");    // -> Imaging cradle / Treatment cradle = lock / unlock
                    var proximity = actionValues[2].Split("="); // -> Proximity = detected / not_detected

                    var flangeID = ActuatorTypesConverter.AcbActuatorIdFromString(flange[0].Trim());
                    var flangeState = ActuatorTypesConverter.AcbActuatorStateFromString(flange[1].Trim());
                    
                    var headID = ActuatorTypesConverter.AcbActuatorIdFromString(cradle[0].Trim());
                    var headState = ActuatorTypesConverter.AcbActuatorStateFromString(cradle[1].Trim());

                    var headProximity = ActuatorTypesConverter.AcbProxySensorStateFromString(proximity[1].Trim());
                    
                    // TODO: check laser

                    bool isSafe = (_acbService.GetActuatorState(flangeID) == flangeState) &&
                        _acbService.GetActuatorState(headID) == headState &&
                        _acbService.GetProxySensorState(headID) == headProximity;

                    return isSafe; 
                }
                else
                {
                    _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: The actionValues array must contain 3 elements, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }

        private bool UserConfirmation(IList<string> actionValues)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Info, LogRecordType.System);
            if (actionValues.Count >= 1)
            {
                bool R = false;
                string message = actionValues[0];
                for (int i = 1; i < actionValues.Count; ++i)
                {
                    message += ", " + actionValues[i];
                }

                string caption = "Confirm Robotic Arm Action";

                _dialogService.Report(
                    caption,
                    message,
                    ReportType.Confirmation,
                    result =>
                    {
                        R = result.Result == ButtonResult.OK;
                    });

                return R;
            }
            else
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: The actionValues array must contain 1 element, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                return false;
            }
        }
        private bool MoveCustom(IList<string> actionValues)
        {
            try
            {
                string presetName = actionValues[0];
                var presets = _positionsPresetsMonitor.PositionPresets;
                foreach (var p in presets)
                {
                    if (p.Name == presetName)
                    {
                        JointsPosition jp = new();
                        jp.JArray[0] = p.J1;
                        jp.JArray[1] = p.J2;
                        jp.JArray[2] = p.J3;
                        jp.JArray[3] = p.J4;
                        jp.JArray[4] = p.J5;
                        jp.JArray[5] = p.J6;
                        return _robotArmService.MoveCustomAction(jp);
                    }
                }
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }
        private bool CheckZone(IList<string> actionValues)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Info, LogRecordType.System);

            if (actionValues.Count >= 1 && actionValues[0] != null)
            {
                string zoneID = actionValues[0].Trim();

                bool result = false;
                // TODO remove set operational mode
                _robotArmService.SetOperatingMode(OperatingMode.RemoteControl);
                var positionMM = _robotArmService.PositionMm;
                if (positionMM != null)
                {
                    //bool no_zone = (positionMM.Z <= 450.0f) && (positionMM.Y <= 212);
                    bool noZone = (positionMM.Z <= _heraclesMainSettings.RobotSafeZoneThresholdZmm);// && (positionMM.Y <= _appSettings.RobotSafeZoneThresholdYmm);
                    result = !noZone;
                }
                return result;
            }
            else
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: The actionValues array must contain 1 element, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                return false;
            }
        }
        private bool UpdateZoneTreshold(IList<string> actionValues)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Info, LogRecordType.System);

            if (actionValues.Count >= 2 && actionValues[0] != null && actionValues[1] != null)
            {
                bool result = false;
                string axis = actionValues[0].Trim().ToLower();
                string thresholdString = actionValues[1].Trim();
                double threshold = 0;
                
                if (!double.TryParse(thresholdString, CultureInfo.InvariantCulture, out threshold))
                {
                    _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Can not parse double='{thresholdString}', actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                    return false;
                }
                
                if (axis == "z")
                {
                    _heraclesMainSettings.RobotSafeZoneThresholdZmm = threshold;
                    result = true;
                }
                else if (axis == "y")
                {
                    _heraclesMainSettings.RobotSafeZoneThresholdYmm = threshold;
                    result = true;
                }
                else
                {
                    _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Invalid axis='{axis}', use 'y' or 'z', actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                }

                return result;
            }
            else
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: The actionValues array must contain 2 element, actionValues = {string.Join(", ", actionValues)}", LogRecordSeverity.Error, LogRecordType.Error);
                return false;
            }
        }
        private bool _CheckActuatorsPrecondition(IList<string> actuatorsPrecondition)
        {
            _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Start, actuatorsPrecondition = {string.Join(", ", actuatorsPrecondition)}", LogRecordSeverity.Info, LogRecordType.System);

            try
            {
                if ((actuatorsPrecondition is null) || 
                    ((actuatorsPrecondition is not null) && (actuatorsPrecondition.Count == 0)))
                {
                    // no conditions
                    return true;
                }

                if (actuatorsPrecondition.Count >= 2 && actuatorsPrecondition[0] != null && actuatorsPrecondition[1] != null)
                {
                    var flange = actuatorsPrecondition[0].Split("=");    // -> Flange = lock / unlock
                    var cradle = actuatorsPrecondition[1].Split("=");    // -> Imaging cradle / Treatment cradle = lock / unlock

                    var flangeID = ActuatorTypesConverter.AcbActuatorIdFromString(flange[0].Trim());
                    var flangeState = ActuatorTypesConverter.AcbActuatorStateFromString(flange[1].Trim());

                    var headID = ActuatorTypesConverter.AcbActuatorIdFromString(cradle[0].Trim());
                    var headState = ActuatorTypesConverter.AcbActuatorStateFromString(cradle[1].Trim());

                    // flange->unlock
                    // flange->lock, headID light sensor->detected
                    bool lightSensorOk =
                        (flangeState == AcbActuatorState.Unlock) ||
                        ((flangeState == AcbActuatorState.Lock) && (_acbService.GetLightSensorState(headID) == AcbLightSensorState.Interrupt));

                    // headID->unlock 
                    // headID->lock, headID proximity->detected
                    bool proxySensorOk =
                        (headState == AcbActuatorState.Unlock) ||
                        ((headState == AcbActuatorState.Lock) && (_acbService.GetProxySensorState(headID) == AcbProxySensorState.Detected));

                    bool actuatorsOk = (_acbService.GetActuatorState(flangeID) == flangeState) &&
                        (_acbService.GetActuatorState(headID) == headState);

                    if (_heraclesMainSettings.UseDummyHeadActuators)
                    {
                        return true;
                    }

                    return lightSensorOk && proxySensorOk && actuatorsOk;
                }
                else
                {
                    _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: The actionValues array must contain 2 elements, actionValues = {string.Join(", ", actuatorsPrecondition)}", LogRecordSeverity.Error, LogRecordType.Error);
                    return false;
                }
            }
            catch (Exception e)
            {
                _logWriter.Log($"{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}: Exception {e.Message}, actuatorsPrecondition = {string.Join(", ", actuatorsPrecondition)}", LogRecordSeverity.Error, LogRecordType.Error);
            }
            return false;
        }
    }
}
