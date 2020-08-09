using System;

namespace Geolocation.DependencyInjection
{
    public class DependencyInjectionAttribute : Attribute
    {
        internal DependencyInjectionType DependencyInjectionType { get; }

        public DependencyInjectionAttribute(DependencyInjectionType dependencyInjectionType = DependencyInjectionType.Default)
        {
            DependencyInjectionType = dependencyInjectionType;
        }
    }

    public enum DependencyInjectionType
    {
        Default,
        PerHttpRequest,
        Singleton,
    }
}
