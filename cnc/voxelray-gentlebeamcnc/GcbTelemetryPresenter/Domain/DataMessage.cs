using Xcc.Core.Domain.GryphonBoard;

namespace GcbTelemetryPresenter.Domain;

internal enum Command
{
    Send = 0,
    Receive
}

internal readonly struct DataMessage : IEquatable<DataMessage>
{
    public static DataMessage SendDataMessage => new DataMessage(DateTime.MinValue, Command.Send, null!);

    public DataMessage(DateTime dateTime, Command command, ISystemTelemetry? systemTelemetry)
    {
        DateTime = dateTime;
        Command = command;
        SystemTelemetry = systemTelemetry;
    }

    public DateTime DateTime { get; }
    public Command Command { get; }
    public ISystemTelemetry? SystemTelemetry { get; }

    public string GetFormattedDateTimeString()
    {
        return $"{DateTime.Date:dd-MM-yyyy} {DateTime:HH:mm:ss:ffff}";
    }


    public override string ToString()
    {
        return $"{GetFormattedDateTimeString()}{Environment.NewLine}{SystemTelemetry.GetVerticallyFormattedString()}";
    }
    
    #region  IEquatable

    public bool Equals(DataMessage other)
    {
        return DateTime.Equals(other.DateTime);
    }

    public override bool Equals(object? obj)
    {
        return obj is DataMessage other && Equals(other);
    }

    public override int GetHashCode()
    {
        return DateTime.GetHashCode();
    }
    #endregion
}