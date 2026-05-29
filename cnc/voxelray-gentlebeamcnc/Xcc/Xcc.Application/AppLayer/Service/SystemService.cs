using System;
using System.Reflection;
using System.Threading.Tasks;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Application.AppLayer.Service;

public class SystemService(ISystemCommands systemCommands)
{
    public ISystemCommands SystemCommands { get; } = systemCommands;

    public async Task<string> GetSystemVersionInfo()
    {
        var versionInfo = await SystemCommands.GetSystemInfoAsync();

        Version? assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        if (assemblyVersion is null)
            throw new Exception("Failed to get assembly version");

        return $"Assembly version: {assemblyVersion}{Environment.NewLine}R&V version: {versionInfo.Version}";

    }
}