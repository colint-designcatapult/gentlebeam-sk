using Heracles.External.Models;
using Heracles.External.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Models;
using Xcc.Core.Services;

namespace Heracles.Outdoor.Test.ViewModels;

internal class ExternalTabsViewModelTests
{
    [Test]
    public async Task ClearFaultsCommand_ClearsMainBoardFaults()
    {
        var commandInvoked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var mainBoardApi = new Mock<IMainBoardAPI>();
        mainBoardApi
            .Setup(api => api.ClearFaults())
            .Returns(() =>
            {
                commandInvoked.SetResult();
                return Task.CompletedTask;
            });
        var authorizedUserStore = new Mock<IAuthorizedUserStore>();
        var viewModel = new ExternalTabsViewModel(
            Mock.Of<IRegionManager>(),
            Mock.Of<IGCBDataStore>(),
            new EventAggregator(),
            Mock.Of<IExitingModel>(),
            Mock.Of<IDialogService>(),
            Mock.Of<IUIStateMachine>(),
            authorizedUserStore.Object,
            mainBoardApi.Object,
            new SystemService(Mock.Of<ISystemCommands>()),
            Mock.Of<IPopUpService>());

        viewModel.ClearFaultsCommand.Execute();
        await commandInvoked.Task.WaitAsync(TimeSpan.FromSeconds(1));

        mainBoardApi.Verify(api => api.ClearFaults(), Times.Once);
    }
}
