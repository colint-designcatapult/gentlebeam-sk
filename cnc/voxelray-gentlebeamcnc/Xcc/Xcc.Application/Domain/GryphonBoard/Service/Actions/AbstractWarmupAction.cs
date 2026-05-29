using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public abstract class AbstractWarmupAction : AbstractMainBoardAction
    {
        private readonly float heaterCurrent;

        public AbstractWarmupAction(
            IMainBoardState mainBoardState,
            IEnumerable<GcbStateNew> fromStates,
            IEnumerable<GcbStateNew> toStates,
            float heaterCurrentSetpoint)
            : base(mainBoardState, fromStates, toStates)
        {
            if (heaterCurrent < PhysicsValueRange.HeaterCurrentMin
            || heaterCurrent > PhysicsValueRange.HeaterCurrentMax)
            {
                throw new ArgumentOutOfRangeException($"Invalid heater current configuration: value={heaterCurrent} is out of range {PhysicsValueRange.HeaterCurrentMin}..{PhysicsValueRange.HeaterCurrentMax}");
            }
            heaterCurrent = heaterCurrentSetpoint;
        }

        protected override Task RunActionAsync(CancellationToken token)
        {
            return RunWarmupAsync(token, heaterCurrent);
        }

        abstract protected Task RunWarmupAsync(CancellationToken token, float heaterCurrent);
    }
}
