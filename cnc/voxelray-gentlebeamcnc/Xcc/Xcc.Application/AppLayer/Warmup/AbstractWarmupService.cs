using System;
using System.Threading.Tasks;
using Xcc.Application.Commands;
using Xcc.Application.Models;
using Xcc.Application.Models.RDBMS;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models.RDBMS;

namespace Xcc.Application.AppLayer.Warmup
{
    public abstract class AbstractWarmupService : IWarmupService, INotifyWarmupEvent
    {
        private readonly IWarmupCommands warmupCommands;
        private readonly IMainBoardAPI mainBoardAPI;
        private readonly IWarmupHistory warmupHistory;

        public AbstractWarmupService(
            IWarmupCommands warmupCommands,
            IMainBoardAPI mainBoardAPI,
            IWarmupHistory warmupHistory)
        {
            this.warmupCommands = warmupCommands;
            this.mainBoardAPI = mainBoardAPI;
            this.warmupHistory = warmupHistory;
        }

        public event EventHandler<WarmupEventArgs> WarmupEvent = null!;

        public async Task UpdateWarmupHistoryAsync()
        {
            long headId = GetActiveHeadId();
            warmupHistory.SetWarmupHistory(await warmupCommands.ReadListAsync(headId));
        }

        protected abstract long GetActiveHeadId();

        public async Task RunSafeWarmupAsync(WarmupParameters warmupParameters)
        {
            WarmupEvent?.Invoke(this, WarmupEventArgs.Start(warmupParameters));

            bool warmupMade = await mainBoardAPI.SafeWarmup(
                warmupParameters);

            if (warmupMade)
            {
                WarmupEvent?.Invoke(this, WarmupEventArgs.Done(warmupParameters));
                if (warmupParameters.ActiveHeadId > 0)
                {
                    await WriteWarmupEventAsync(warmupParameters);
                    //var warmUp = new WarmUp
                    //{
                    //    CreationDate = DateTime.Now,
                    //    Type = warmupParameters.WarmupType,
                    //    HeaterCurrent = warmupParameters.HeaterCurrentSetpoint,
                    //    HeadId = warmupParameters.ActiveHeadId // todo: should be taken from Model, not from arguments
                    //};
                }
            }
        }

        private async Task<IWarmUp> WriteWarmupEventAsync(WarmupParameters warmupParameters)
        {
            IWarmUp storedWarmupEvent = await warmupCommands.CreateAsync(new WarmUp
            {
                Id = BaseEntry.NEW_ENTRY_ID,
                HeadId = warmupParameters.ActiveHeadId,
                HeaterCurrent = warmupParameters.HeaterCurrentSetpoint,
                Type = warmupParameters.WarmupType
            });
            warmupHistory.OnNewWarmupEvent(storedWarmupEvent);
            return storedWarmupEvent;
        }
    }
}
