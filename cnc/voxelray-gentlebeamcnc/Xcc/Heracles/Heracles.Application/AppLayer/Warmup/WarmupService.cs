using Heracles.Application.AppLayer.Collimators;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Commands;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.Application.AppLayer.Warmup
{
    public class WarmupService(
        IWarmupCommands warmupCommands,
        IMainBoardAPI mainBoardAPI,
        IWarmupHistory warmupHistory,
        ICollimatorModel collimatorModel
        ) : AbstractWarmupService(warmupCommands, mainBoardAPI, warmupHistory)
    {
        protected override long GetActiveHeadId()
        {
            return collimatorModel.ActiveHead?.Id ?? BaseEntry.NEW_ENTRY_ID;
        }
    }
}
