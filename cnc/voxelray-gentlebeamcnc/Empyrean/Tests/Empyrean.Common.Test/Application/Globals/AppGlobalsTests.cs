
using Empyrean.Common.Application.Globals;

namespace Empyrean.Common.Test.Application.Globals
{
    public class AppGlobalsTests
    {
        [Test]
        public void AppCancellationTokenSource_NotNull_ByDefault()
        {
            var appGlobals = new AppGlobals();
            Assert.That(appGlobals.AppCancellationTokenSource, Is.Not.Null);
        }
        
        [Test]
        public void AppCancellationTokenSource_Changed_After_Set()
        {
            var appGlobals = new AppGlobals();
            var initialToken = appGlobals.AppCancellationTokenSource;

            appGlobals.AppCancellationTokenSource = new CancellationTokenSource(); 
            Assert.That(appGlobals.AppCancellationTokenSource, Is.Not.EqualTo(initialToken));
            
            appGlobals.AppCancellationTokenSource = null!;
            Assert.That(appGlobals.AppCancellationTokenSource, Is.Null);
            
        }
    }
}