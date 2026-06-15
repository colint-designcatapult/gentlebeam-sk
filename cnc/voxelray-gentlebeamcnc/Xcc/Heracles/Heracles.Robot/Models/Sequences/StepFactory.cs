using Heracles.Application.Services;
using Heracles.Core.Models;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using Heracles.Robot.Models.RobotArm.Interfaces;
using Heracles.Robot.Services;
using Xcc.Core.Logging;

namespace Heracles.Robot.Models.Sequences
{

    public class StepFactory : IStepFactory
    {
        IPositionsPresetsMonitor _positionsPresetsMonitor;
        IRobotArmService _robotArmService;
        ILogRepository _logWriter;
        IAcbService _acbService;
        IDialogService _dialogService;
        IHeraclesMainSettings _heraclesMainSettings;

        public StepFactory(IRobotArmService robotArmService, IPositionsPresetsMonitor positionsPresetsMonitor, ILogRepository logWriter, IAcbService acbService, IDialogService dialogService, IHeraclesMainSettings heraclesMainSettings)
        {
            _robotArmService = robotArmService;
            _positionsPresetsMonitor = positionsPresetsMonitor;
            _logWriter = logWriter;
            _acbService = acbService;
            _dialogService = dialogService;
            _heraclesMainSettings = heraclesMainSettings;
        }
        IStep IStepFactory.Create(string id, string nextIdIfOk, string nextIdIfFailed, string action, IList<string> actionValues, IList<string> actuatorsPrecondition)
        {
            return new Steps.Step(id, nextIdIfOk, nextIdIfFailed, action, actionValues, actuatorsPrecondition, _robotArmService, _positionsPresetsMonitor, _logWriter, _acbService, _dialogService, _heraclesMainSettings);
        }
    }
}
