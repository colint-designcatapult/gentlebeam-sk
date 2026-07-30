using Heracles.Ucsi.ViewModels;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.External.Services;

public sealed class ExternalUcsiHostCommands(
    IMainBoardAPI mainBoardApi) : IUcsiHostCommands
{
    public bool CanClearFaults => true;
    public string ClearFaultsUnavailableReason => string.Empty;
    public Task ClearFaultsAsync() => mainBoardApi.ClearFaults();
}
