using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class InitializeBoard : AbstractMainBoardAction
    {
        private readonly IGcbCommandInterface gcbCommands;

        public InitializeBoard(
            IMainBoardState mainBoardState,
            IGcbCommandInterface gcbCommands)
            : base(mainBoardState, [GcbStateNew.Startup], [GcbStateNew.Cold])
        {
            this.gcbCommands = gcbCommands;
        }

        protected override async Task RunActionAsync(CancellationToken token)
        {
            await gcbCommands.Initialize();
        }
    }
}
