using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class FastWarmup : AbstractWarmupAction
    {
        private readonly IGcbCommandInterface gcbCommands;

        public FastWarmup(
            IMainBoardState mainBoardState,
            IGcbCommandInterface xRayService,
            float warmupHeaterCurrentSetpoint)
            : base(mainBoardState,
                  fromStates: [GcbStateNew.Cold],
                  toStates: [GcbStateNew.Primed, GcbStateNew.Staged],
                  heaterCurrentSetpoint: warmupHeaterCurrentSetpoint)
        {
            gcbCommands = xRayService;
        }

        protected override Task RunWarmupAsync(CancellationToken token, float heaterCurrent)
        {
            return gcbCommands.WarmUp(heaterCurrent);
        }
    }
}
