using System.Text.RegularExpressions;
using GcbTelemetryPresenter.Domain;
using Xcc.Infra.GryphonBoard;

namespace GcbTelemetryPresenter.Infra;

internal static class TelemetrySerializer
{
    public static DataMessage Deserialize(string line)
    {
        var pattern = @"^(.+)\s+(\w+):\s+([A-F0-9]+)$";
        var match = Regex.Match(line, pattern);

        if (match is {Success: true, Groups.Count: 4})
        {
            if (!DateTime.TryParse(match.Groups[1].Value, out var dateTime))
            {
                throw new FormatException("Invalid DateTime format");
            }

            var commandString = match.Groups[2].Value;
            if (!Enum.TryParse(commandString, out Command command))
            {
                throw new FormatException("Unknown command");
            }

            if (command == Command.Send)
                return DataMessage.SendDataMessage;
            
            var bytes = Convert.FromHexString(match.Groups[3].Value);

            return new DataMessage(dateTime, command, SystemTelemetry.Parse(bytes));
        }

        throw new FormatException("Invalid line format");
    }
}