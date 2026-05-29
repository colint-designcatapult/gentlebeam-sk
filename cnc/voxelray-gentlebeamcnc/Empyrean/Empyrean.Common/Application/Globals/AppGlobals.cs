namespace Empyrean.Common.Application.Globals
{
    public class AppGlobals : IAppGlobals
    {
        public CancellationTokenSource AppCancellationTokenSource { get; set; } = new CancellationTokenSource();
    }
}
