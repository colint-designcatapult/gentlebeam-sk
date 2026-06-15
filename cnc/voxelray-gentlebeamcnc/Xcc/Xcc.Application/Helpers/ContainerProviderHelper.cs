using System;
using Prism.Ioc;

namespace Xcc.Application.Helpers
{
    public static class ContainerProviderHelper
    {
        public static void DisposeByType<T>(IContainerProvider container)
        {
            if (container.IsRegistered<T>() &&
                container.Resolve<T>() is IDisposable instance)
            {
                instance.Dispose();
            }
        }
    }
}
