using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class ConfirmAndReleasePlan : AbstractPlanSetupAction
    {
        public ConfirmAndReleasePlan(
            IMainBoardState mainBoardState,
            IGcbCommandInterface gcbCommands,
            ILogWriter logService
            )
            : base(mainBoardState, gcbCommands, logService,
                  [GcbStateNew.Staged],
                  [GcbStateNew.Ready],
                  OperationalPointCmdType.Confirmation)
        {
        }

        protected override Task FinalizePlanAsync(CancellationToken token)
        {
            return gcbCommands.ReleasePlan(
                GCBReleaseCommandScope.Plan,
                MainBoard.Session!.Value);
        }
    }
}
