using System.Globalization;

namespace Empyrean.Common.Infra.Logging;

public class TextLogRecord
{
    public const string Delimiter = "   ";

    public string Severity { get; }
    public string Type { get; }
    public string Message { get; } 
    public DateTime TimeStamp { get; } = DateTime.Now;

    public TextLogRecord(string message, string severity, string type)
    {
        Message = message;
        Severity = severity;
        Type = type;
    }
    public TextLogRecord(string message, string severity, string type, DateTime timeStamp)
    {
        Message = message;
        Severity = severity;
        Type = type;
        TimeStamp = timeStamp;
    }

    public override string ToString() => $"{TimeStamp}{Delimiter}{Severity}{Delimiter}{Type}{Delimiter}{Message}";

    public static TextLogRecord? Parse(string? record)
    {
        if (string.IsNullOrWhiteSpace(record))
            return null;
            //throw new Exception($"Failed to parse empty log record.");

        var fields = record.Split(Delimiter);

        if (fields.Length < 4)
            return null;
            //throw new Exception($"Failed to parse log record from string {record}");

        if (DateTime.TryParse(fields[0], out DateTime timeStamp) == false)
            return null;
            //throw new Exception($"Failed to parse log record timestamp ({fields[0]})");
        
        return new TextLogRecord
        (
            fields[3],
            fields[1],
            fields[2],
            timeStamp
        );
    }
}