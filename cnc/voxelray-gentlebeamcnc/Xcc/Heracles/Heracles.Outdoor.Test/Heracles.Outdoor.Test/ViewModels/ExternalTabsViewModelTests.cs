using Heracles.External.Services;
using Moq;
using NUnit.Framework;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.Outdoor.Test.ViewModels;
internal class ExternalUcsiHostCommandsTests
{
    [NUnit.Framework.Test]
    public async Task ClearFaultsAsync_ClearsMainBoardFaults()
    {
        var mainBoardApi = new Mock<IMainBoardAPI>();
        mainBoardApi
            .Setup(api => api.ClearFaults())
            .Returns(Task.CompletedTask);
        var commands = new ExternalUcsiHostCommands(mainBoardApi.Object);

        await commands.ClearFaultsAsync();

        Assert.That(commands.CanClearFaults, Is.True);
        mainBoardApi.Verify(api => api.ClearFaults(), Times.Once);
    }
}
