using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Application.Domain.GryphonBoard.Service.Actions
{
    public class Conditioning : AbstractWarmupAction
    {
        private readonly IGcbCommandInterface _gcbCommands;

        public Conditioning(
            IMainBoardState mainBoardState,
            IGcbCommandInterface gcbCommands,
            float conditioningHeaterCurrentSetpoint)
            : base(mainBoardState,
                  fromStates: [GcbStateNew.Cold],
                  toStates: [GcbStateNew.StandBy],
                  heaterCurrentSetpoint: conditioningHeaterCurrentSetpoint)
        {
            _gcbCommands = gcbCommands;
        }

        protected override async Task RunWarmupAsync(CancellationToken token, float heaterCurrent)
        {
            await _gcbCommands.Conditioning(heaterCurrent);

            // Ensure that the conditioning at least started
            await WaitForState(GcbStateNew.DailyWarmup, token);
        }
    }
}
