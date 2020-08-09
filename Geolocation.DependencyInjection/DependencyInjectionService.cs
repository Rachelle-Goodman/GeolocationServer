using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Geolocation.DependencyInjection
{
    public static class DependencyInjectionService
    {
        public static void RegisterDependencies(IServiceCollection services)
        {
            IEnumerable<(Type Type, DependencyInjectionAttribute Attribute)> typesToInject = GetTypesToInject();

            foreach (var typeToInject in typesToInject)
            {
                var implementedInterface = GetImplementedInterface(typeToInject.Type);
                InjectByDependencyInjectionType(services, typeToInject, implementedInterface);
            }
        }

        private static IEnumerable<(Type Type, DependencyInjectionAttribute Attribute)> GetTypesToInject()
        {
            IEnumerable<Assembly> assemblies = GetAssemblies();

            return from assembly in assemblies
                   from type in assembly.GetTypes()
                   let attributes = type.GetCustomAttributes(typeof(DependencyInjectionAttribute), true)
                   where attributes != null && attributes.Length > 0
                   select (Type: type, Attribute: attributes.Cast<DependencyInjectionAttribute>().First());
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            return Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Geolocation.*.dll")
                .Select(libraryName => Assembly.Load(AssemblyName.GetAssemblyName(libraryName)));
        }

        private static IEnumerable<Assembly> GetReferencedAssemblies(Assembly assembly)
        {
            yield return assembly;

            assembly.GetReferencedAssemblies()
                .Where(x => x.Name.StartsWith("Geolocation."))
                .Select(assemblyName => GetReferencedAssemblies(Assembly.Load(assemblyName)))
                .Distinct();
        }

        private static Type GetImplementedInterface(Type typeToInject)
            => typeToInject.GetInterfaces()[0];

        private static void InjectByDependencyInjectionType(IServiceCollection services, (Type Type, DependencyInjectionAttribute Attribute) typeToInject, Type implementedInterface)
        {
            switch (typeToInject.Attribute.DependencyInjectionType)
            {
                case DependencyInjectionType.Default:
                    services.AddTransient(implementedInterface, typeToInject.Type);
                    break;

                case DependencyInjectionType.PerHttpRequest:
                    services.AddScoped(implementedInterface, typeToInject.Type);
                    break;

                case DependencyInjectionType.Singleton:
                    services.AddSingleton(implementedInterface, typeToInject.Type);
                    break;

                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
