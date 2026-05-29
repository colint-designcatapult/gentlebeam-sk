namespace Empyrean.Common.Infra.Settings
{
    public interface ILogSettings
    {
        public int LogPageSize { get; }
    }

    public interface ITextLogSettings : ILogSettings
    {
        public string LogFilename { get; }
        public string AppLogFolder { get; }
    }
}
