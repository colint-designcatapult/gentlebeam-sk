using System;
using System.Collections.Generic;
using System.Threading;
using Heracles.Robot.Models.RobotArm.Interfaces;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Sequences
{
    public class Sequence : ISequence
    {
        #region Constructors
        public Sequence(string name, IList<IStep> steps, ILogRepository logWriter)
        {
            Name = name;
            _steps = steps;
            _logWriter = logWriter;
            Reset();
            if (check() == false)
            {
                throw new Exception("Invalid sequence, see log for details");
            }
        }
        #endregion Constructors

        #region Constants
        const int _invalidStepIndex = -1;
        const int _delayBetweenStepsMs = 0000;
        const string _lastStepOkId = "stop_ok"; // sequence finished successfully
        const string _lastStepFailureId = "stop_failure"; // sequence finished with errors
        const string _unknownStepName = "Unknown";
        const string _completedStepName = "Completed";
        #endregion Constants

        #region Public Properties
        public event EventHandler StepDone;
        public string Name { get; private set; }
        public bool CanDoNextStep { get => isValidStepIndex(_nextStepIndex); }
        public string CurrentStepName { get => _currentStep == null ? _unknownStepName : _currentStep.Name; }
        public string NextStepName { get => _nextStep == null ? _completedStepName : _nextStep.Name; }
        #endregion Public Properties

        #region Private Properties
        ILogRepository _logWriter;
        IList<IStep> _steps;
        int _currentStepIndex; // actually done
        int _nextStepIndex; // will be executed
        IStep _currentStep { get => isValidStepIndex(_currentStepIndex) ? _steps[_currentStepIndex] : null; }
        IStep _nextStep { get => isValidStepIndex(_nextStepIndex) ? _steps[_nextStepIndex] : null; }
        #endregion Private Properties

        #region Public Methods
        public void Reset()
        {
            _currentStepIndex = _invalidStepIndex;
            _nextStepIndex = (_steps != null && _steps.Count > 0) ? 0 : _invalidStepIndex;
        }
        // DoNextStep:
        //   If OkId is stop_ok and step success - returns true
        //   If OkId is stop_failure and step success - returns false
        //   If FailureId is stop_ok and step failed - returns true
        //   If FailureId is stop_failure and step failed - returns false
        //   For other cases: returns false if step failed, true - otherwise
        public bool DoNextStep()
        {
            bool ok = false;
            if (_nextStep != null)
            {
                if (!_nextStep.CheckActuatorsPrecondition())
                {
                    // abort sequence execution
                    _nextStepIndex = _invalidStepIndex;
                    return false;
                }
                ok = _nextStep.Do();
                if (!ok)
                {
                    ok = handleNonCompletionStep();
                }
                ok = moveNext(ok);
            }
            return ok;
        }
        public bool Do()
        {
            bool result = false;
            while (CanDoNextStep)
            {
                result = DoNextStep();
                StepDone?.Invoke(this, new EventArgs());
                Thread.Sleep(_delayBetweenStepsMs);
            }
            return result;
        }
        public override string ToString()
        {
            return Name;
        }
        #endregion Public Methods

        #region Private Methods
        bool isValidStepIndex(int index)
        {
            return index >= 0 && index < _steps.Count;
        }

        int getIndexById(string id)
        {
            for (int i = 0; i < _steps.Count ; ++i)
            {
                if (_steps[i].Id == id)
                {
                    return i;
                }
            }
            return _invalidStepIndex;
        }
        int calcStepsById(string id)
        {
            int cnt = 0;
            for (int i = 0; i < _steps.Count; ++i)
            {
                if (_steps[i].Id == id)
                {
                    ++cnt;
                }
            }
            return cnt;
        }
        bool moveNextByStepId(string nextId, bool ok)
        {
            if (nextId == _lastStepOkId || nextId == _lastStepFailureId)
            {
                _currentStepIndex = _nextStepIndex;
                _nextStepIndex = _invalidStepIndex;
                return nextId == _lastStepOkId;
            }

            _currentStepIndex = _nextStepIndex;
            _nextStepIndex = getIndexById(nextId);

            return ok;
        }
        bool moveNext(bool ok)
        {
            if (ok)
            {
                return moveNextByStepId(_nextStep.NextIdIfOk, ok);
            }
            return moveNextByStepId(_nextStep.NextIdIfFailed, ok);
        }
        bool handleNonCompletionStep()
        {
            return false;
            //string caption = "Error";
            //var res = MessageBox.Show("Step '" + NextStepName + "' failed!\n\nIgnore?", caption, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.OK);
            //return res == MessageBoxResult.Yes;
        }

        bool checkStepId(string stepId)
        {
            bool ok = true;
            if (stepId == null)
            {
                ok = false;
            }
            else if (stepId == string.Empty)
            {
                ok = false;
            }
            else if ((stepId == _lastStepOkId) || (stepId == _lastStepFailureId))
            {
                // all ok
            }
            else if (getIndexById(stepId) == _invalidStepIndex)
            {
                ok = false;
            }
            return ok;
        }
        // check: returns true if all steps has valid Id, NextIdIfOk and NextIdIfFailed
        bool check()
        {
            if (_steps == null)
                return true;

            bool ok = true;
            foreach (var step in _steps) 
            {
                if (!checkStepId(step.Id))
                {
                    _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Invalid step.Id='{step.Id}' for  step.Name='{step.Name}' Sequence.Name='{Name}'", LogRecordSeverity.Error, LogRecordType.System);
                    ok = false;
                }
                if (!checkStepId(step.NextIdIfOk))
                {
                    _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Invalid step.NextIdIfOk='{step.NextIdIfOk}' for  step.Name='{step.Name}' Sequence.Name='{Name}'", LogRecordSeverity.Error, LogRecordType.System);
                    ok = false;
                }
                if (!checkStepId(step.NextIdIfFailed))
                {
                    _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Invalid step.NextIdIfFailed='{step.NextIdIfFailed}' for  step.Name='{step.Name}' Sequence.Name='{Name}'", LogRecordSeverity.Error, LogRecordType.System);
                    ok = false;
                }
                if (calcStepsById(step.Id) > 1)
                {
                    _logWriter.Log($"{this.GetType().FullName}.{System.Reflection.MethodInfo.GetCurrentMethod().Name}: The step ID must be unique step.Id='{step.Id}' for  step.Name='{step.Name}' Sequence.Name='{Name}'", LogRecordSeverity.Error, LogRecordType.System);
                    ok = false;
                }
            }
            return ok;
        }


        #endregion Private Methods
    }
}
