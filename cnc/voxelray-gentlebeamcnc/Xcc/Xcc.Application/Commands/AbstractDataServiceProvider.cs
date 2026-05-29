using System;
using System.Collections.Generic;
using System.Threading;

namespace Xcc.Application.Commands
{
    public interface IDataServiceProvider
    {
        TService MakeServiceInstance<TService>() where TService : class;
    }


    public abstract class AbstractDataServiceProvider : IDataServiceProvider
    {
        private readonly Dictionary<System.Type, Func<object>> services = new();
        private readonly SynchronizationContext? syncContext = SynchronizationContext.Current;

        public void AddService<TServiceType>(Func<TServiceType> factoryMethod)
            where TServiceType : class
        {
            var serviceType = typeof(TServiceType);
            if (services.ContainsKey(serviceType))
            {
                throw new InvalidOperationException($"Can't add new service {typeof(TServiceType).Name}: service type does already exist");
            }
            else
            {
                services[serviceType] = factoryMethod;
            }
        }

        public TServiceType MakeServiceInstance<TServiceType>() where TServiceType : class
        {
            lock (services)
            {
                Func<object>? existingServiceFactoryMethod = null;

                if (!services.TryGetValue(typeof(TServiceType), out existingServiceFactoryMethod))
                {
                    throw new NotImplementedException($"Can't provide {typeof(TServiceType).Name} service");
                }
                else
                {
                    return (TServiceType)existingServiceFactoryMethod();
                }
            }
        }
    }
}
