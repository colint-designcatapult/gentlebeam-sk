namespace Empyrean.Common.Infra.Events;

public class ErrorEventArgs 
{
    public Exception? Exception { get; set; }
    public required string Message { get; set; }
}