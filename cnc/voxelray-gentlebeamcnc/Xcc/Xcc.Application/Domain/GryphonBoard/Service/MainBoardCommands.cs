using System.Threading.Tasks;
using Xcc.Application.Domain.GryphonBoard.Service.Actions;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.Domain.GryphonBoard.Service
{

    public interface IAsyncActionCommand
    {
        bool CanRun();
        Task RunAsync();
    }

    public class AsyncActionCommand : IAsyncActionCommand
    {
        private readonly ActionSequentialExecutor actionExecutor;
        private readonly IAsyncAction action;
        private readonly ILogWriter logService;

        public AsyncActionCommand(
            ActionSequentialExecutor actionExecutor,
            IAsyncAction action,
            ILogWriter logService)
        {
            this.actionExecutor = actionExecutor;
            this.action = action;
            this.logService = logService;
        }

        public virtual bool CanRun()
        {
            return !actionExecutor.IsBusy() && action.CanRun();
        }

        public virtual async Task RunAsync()
        {
            try
            {
                await actionExecutor.Execute(action);
            }
            catch (TaskCanceledException)
            {
                logService.Log($"Action {GetType().Name} was cancelled", LogRecordSeverity.Warn, LogRecordType.System);
                throw;
            }
        }
    }

    public class AsyncActionCommand<TAction> : AsyncActionCommand
        where TAction : IAsyncAction
    {
        public AsyncActionCommand(
            ActionSequentialExecutor actionExecutor,
            TAction action,
            ILogWriter logService)
            : base(actionExecutor, action, logService)
        {
        }
    }

    internal class ResumePlan : AsyncActionCommand
    {
        public ResumePlan(
            ActionSequentialExecutor actionExecutor,
            ResumePlanAction resumePlanAction,
            ILogWriter logService)
            : base(actionExecutor, resumePlanAction, logService)
        {
        }
    }


    internal class ResumePlanAction : MacroAction
    {
        public ResumePlanAction(
            IMainBoardModel mainBoardModel,
            IGcbCommandInterface gcbAPI,
            ILogWriter logService)
            : base(actions: [
                // First we update the plan from the board if we can
                // and clear it from the board if we don't have any session:
                new MainBoardOptionalActionWrapper(new ClearOrKeepPlanOnBoard(mainBoardModel, mainBoardModel, gcbAPI, tryKeepSamePlan: true)),
                // Now we probably need to re-establish a new session and load the plan again:
                new MainBoardOptionalActionWrapper(new NewSession(mainBoardModel, gcbAPI)),
                new MainBoardOptionalActionWrapper(new LoadAndStagePlan(mainBoardModel, gcbAPI, logService)),
                new ConfirmAndReleasePlan(mainBoardModel, gcbAPI, logService)
                ])
        {
        }
    }
}
