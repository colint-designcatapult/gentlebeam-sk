namespace Empyrean.Common.Application.Globals
{
    public interface IAppGlobals
    {
        public CancellationTokenSource AppCancellationTokenSource { get; set; }
    }
}
