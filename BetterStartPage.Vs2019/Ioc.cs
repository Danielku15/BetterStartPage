using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterStartPage
{
    public class Ioc
    {
        private readonly Dictionary<Type, Type> _registrations;
        private readonly Dictionary<Type, object> _instances;

        public static Ioc Instance { get; }

        static Ioc()
        {
            Instance = new Ioc();
        }

        public Ioc()
        {
            _registrations = new Dictionary<Type, Type>();
            _instances = new Dictionary<Type, object>();
        }

        public void Register<TClass>()
        {
            Register<TClass, TClass>();
        }

        public void RegisterInstance<TClass>(TClass instance)
        {
            _instances[typeof(TClass)] = instance;
        }

        public void Register<TInterface, TClass>()
        {
            Register(typeof(TInterface), typeof(TClass));
        }

        public void Register(Type interfaceType, Type classType)
        {
            _registrations[interfaceType] = classType;
        }

        public void Unregister<TClass>()
        {
            Unregister(typeof(TClass));
        }

        private void Unregister(Type type)
        {
            _registrations.Remove(type);
        }

        public TClass Resolve<TClass>()
        {
            return (TClass)Resolve(typeof(TClass));
        }

        public object Resolve(Type interfaceType)
        {
            object instance;
            if (_instances.TryGetValue(interfaceType, out instance))
            {
                return instance;
            }

            _instances[interfaceType] = instance = CreateInstance(interfaceType);

            return instance;
        }

        private object CreateInstance(Type interfaceType)
        {
            Type implementationType;
            if (!_registrations.TryGetValue(interfaceType, out implementationType))
            {
                throw new InvalidOperationException($"No implementation for type {interfaceType.FullName} registered");
            }

            var constructor = implementationType
                .GetConstructors()
                .Where(c => c.IsPublic)
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
            {
                throw new InvalidOperationException($"No public constructor for type {interfaceType.FullName} found");
            }

            var parameters = constructor
                .GetParameters()
                .Select(p => Resolve(p.ParameterType))
                .ToArray();

            return constructor.Invoke(parameters);
        }
    }
}
