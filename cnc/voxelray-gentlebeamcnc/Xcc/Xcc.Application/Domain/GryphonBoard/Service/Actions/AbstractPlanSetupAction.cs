using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public abstract class AbstractPlanSetupAction : AbstractMainBoardAction
    {
        protected readonly IGcbCommandInterface gcbCommands;
        protected readonly ILogWriter logService;
        private readonly OperationalPointCmdType setupActionType;

        public AbstractPlanSetupAction(
            IMainBoardState mainBoardState,
            IGcbCommandInterface gcbCommands,
            ILogWriter logService,
            IEnumerable<GcbStateNew> fromStates,
            IEnumerable<GcbStateNew> toStates,
            OperationalPointCmdType setupActionType)
            : base(mainBoardState, fromStates, toStates)
        {
            this.gcbCommands = gcbCommands;
            this.logService = logService;
            this.setupActionType = setupActionType;
        }

        private async Task SendOperationalPointsAsync(
            GcbEmissionPlan plan,
            OperationalPointCmdType commandType,
            GcbSession session,
            CancellationToken token)
        {
            logService.Log($"SendOperationalPoints", LogRecordSeverity.Info, LogRecordType.System);

            foreach (var op in plan.Points)
            {
                logService.Log($"OperationalPointIndex={op.PointIndex}", LogRecordSeverity.Info, LogRecordType.System);
                logService.Log($"Energy={op.SetpointKv}", LogRecordSeverity.Info, LogRecordType.System);
                logService.Log($"TotalPointTime={op.TotalPointTime}", LogRecordSeverity.Info, LogRecordType.System);
                logService.Log($"RemainingPointTime={op.RemainingPointTime}", LogRecordSeverity.Info, LogRecordType.System);
                logService.Log($"TargetMA={op.TargetMA}", LogRecordSeverity.Info, LogRecordType.System);
                logService.Log($"FilamentSetpoint={op.FilamentSetpoint}", LogRecordSeverity.Info, LogRecordType.System);

                await gcbCommands.SendOperationalPoint(commandType, op, session);

                token.ThrowIfCancellationRequested();
            }
        }

        protected override async Task RunActionAsync(CancellationToken token)
        {
            await SendOperationalPointsAsync(
                MainBoard.CurrentPlan,
                setupActionType,
                MainBoard.Session!.Value,
                token);

            await FinalizePlanAsync(token);
        }

        protected abstract Task FinalizePlanAsync(CancellationToken token);
    }
}
