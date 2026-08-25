using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine
{
    public sealed class GameServiceManager : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceEntry> _services = new();

        public void AddSingleton<TInterface>(TInterface instance)
            where TInterface : class
        {
            Register(typeof(TInterface), new ServiceEntry
            {
                Lifetime = ServiceLifetime.Singleton,
                Instance = instance
            });
        }

        public void AddTransient<TInterface>(
            Func<TInterface> factory)
            where TInterface : class
        {
            Register(typeof(TInterface), new ServiceEntry
            {
                Lifetime = ServiceLifetime.Transient,
                Factory = () => factory()
            });
        }

        public TInterface Get<TInterface>()
            where TInterface : class
        {
            if (_services.TryGetValue(typeof(TInterface), out var entry))
            {
                return (TInterface)entry.GetInstance();
            }

            throw new InvalidOperationException(
                $"Service {typeof(TInterface).Name} not registered.");
        }

        public bool TryGet<TInterface>(out TInterface? service)
            where TInterface : class
        {
            var type = typeof(TInterface);

            if (!type.IsInterface)
                throw new InvalidOperationException(
                    "Only interfaces can be resolved.");

            if (_services.TryGetValue(type, out var entry))
            {
                service = (TInterface)entry.GetInstance();
                return true;
            }

            service = null;
            return false;
        }

        private void Register(Type type, ServiceEntry entry)
        {
            if (!type.IsInterface)
                throw new InvalidOperationException(
                    "Only interfaces are allowed.");

            _services[type] = entry;
        }

        public object? GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }
    }


    enum ServiceLifetime
    {
        Singleton,
        Transient
    }


    sealed class ServiceEntry
    {
        public ServiceLifetime Lifetime;
        public object? Instance;
        public Func<object>? Factory;


        public object GetInstance()
        {
            if (Lifetime == ServiceLifetime.Singleton)
                return Instance!;

            return Factory!();
        }
    }
}