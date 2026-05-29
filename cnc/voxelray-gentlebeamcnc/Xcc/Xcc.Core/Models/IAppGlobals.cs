using System.Threading;

namespace Xcc.Core.Models
{
    public interface IAppGlobals
    {
        public CancellationTokenSource AppCancellationTokenSource { get; set; }
    }
}
