using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class NewSession : AbstractMainBoardAction
    {
        private readonly IMainBoardStateManagement mainBoardStateManagement;
        private readonly IGcbCommandInterface gcbCommands;

        public NewSession(
            IMainBoardStateManagement mainBoardState,
            IGcbCommandInterface gcbCommands)
            : base(mainBoardState, [GcbStateNew.Primed], [GcbStateNew.Staging])
        {
            this.mainBoardStateManagement = mainBoardState;
            this.gcbCommands = gcbCommands;
        }

        protected override async Task RunActionAsync(CancellationToken token)
        {
            mainBoardStateManagement.SetSession(
                await gcbCommands.NewSession(MainBoard.CurrentPlan.TotalPoints));
        }
    }
}
