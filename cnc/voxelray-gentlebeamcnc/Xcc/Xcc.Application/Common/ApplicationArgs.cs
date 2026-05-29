using System;
using System.IO;
using System.Linq;

namespace Xcc.Application.Common;

public static class ApplicationArgs
{
    public static string? GetAppSettings()
    {
        var args = Environment.GetCommandLineArgs();
        if (args is null)
            return null;

        var argsDictionary = args
            .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains('='))
            .Select(line => line.Split("=", 2, 0))
            .ToDictionary(split => split[0].Trim(), split => split[1].Trim());

        if (argsDictionary.TryGetValue("--appsettings", out var value) == false)
            return null;

        if (File.Exists(value) == false)
            return null;

        return value;
    }
}