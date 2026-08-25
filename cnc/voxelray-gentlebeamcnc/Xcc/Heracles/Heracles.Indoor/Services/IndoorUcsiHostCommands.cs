using System;
using System.Threading.Tasks;
using Heracles.Ucsi.ViewModels;

namespace Heracles.Indoor.Services;

/// <summary>
/// Indoor application implementation of IUcsiHostCommands.
/// Disables fault clearing for safety - prevents accidental emission start in embedded mode.
/// </summary>
public sealed class IndoorUcsiHostCommands : IUcsiHostCommands
{
    public bool CanClearFaults => false;
    public string ClearFaultsUnavailableReason => "Clear Faults is disabled in embedded mode for safety.";
    public Task ClearFaultsAsync() => Task.CompletedTask;
}
