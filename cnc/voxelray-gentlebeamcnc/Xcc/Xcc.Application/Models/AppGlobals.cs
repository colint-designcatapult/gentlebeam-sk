using System.Threading;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public class AppGlobals(CancellationTokenSource? cancellationTokenSource) : IAppGlobals
    {
        public AppGlobals()
            : this(null)
        {
        }
        public CancellationTokenSource AppCancellationTokenSource { get; set; } = 
            (cancellationTokenSource != null) ? cancellationTokenSource : new CancellationTokenSource();
    }
}
