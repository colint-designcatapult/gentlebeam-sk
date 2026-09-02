﻿using System;
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

        private async Task SendOperationalPointAsync(
            GcbOperationalPoint emission,
            OperationalPointCmdType commandType,
            GcbSession session,
            CancellationToken token)
        {
            logService.Log("SendOperationalPoint", LogRecordSeverity.Info, LogRecordType.System);
            logService.Log($"Energy={emission.SetpointKv}", LogRecordSeverity.Info, LogRecordType.System);
            logService.Log($"TotalPointTime={emission.TotalPointTime}", LogRecordSeverity.Info, LogRecordType.System);
            logService.Log($"RemainingPointTime={emission.RemainingPointTime}", LogRecordSeverity.Info, LogRecordType.System);
            logService.Log($"TargetMA={emission.TargetMA}", LogRecordSeverity.Info, LogRecordType.System);
            logService.Log($"FilamentSetpoint={emission.FilamentSetpoint}", LogRecordSeverity.Info, LogRecordType.System);

            await gcbCommands.SendOperationalPoint(commandType, emission, session);
            token.ThrowIfCancellationRequested();
        }

        protected override async Task RunActionAsync(CancellationToken token)
        {
            var emission = MainBoard.CurrentEmission
                ?? throw new InvalidOperationException("Cannot configure the board without an emission.");
            await SendOperationalPointAsync(
                emission,
                setupActionType,
                MainBoard.Session!.Value,
                token);

            await FinalizePlanAsync(token);
        }

        protected abstract Task FinalizePlanAsync(CancellationToken token);
    }
}
