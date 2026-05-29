using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class LoadAndStagePlan : AbstractPlanSetupAction
    {
        public LoadAndStagePlan(
            IMainBoardState mainBoardState,
            IGcbCommandInterface gcbCommands,
            ILogWriter logService)
            : base(mainBoardState, gcbCommands, logService,
                  [GcbStateNew.Staging],
                  [GcbStateNew.Staged],
                  OperationalPointCmdType.Load)
        {
        }

        protected override Task FinalizePlanAsync(CancellationToken token)
        {
            return gcbCommands.StagePlan();
        }
    }
}
