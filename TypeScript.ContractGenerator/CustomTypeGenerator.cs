using System;
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

using SkbKontur.TypeScript.ContractGenerator.CodeDom;
using SkbKontur.TypeScript.ContractGenerator.Internals;
using SkbKontur.TypeScript.ContractGenerator.TypeBuilders;

namespace SkbKontur.TypeScript.ContractGenerator
{
    public class Redirect
    {
        public string Name { get; set; }
        public string Location { get; set; }
    }

    public class CustomTypeGenerator : ICustomTypeGenerator
    {
        public string GetTypeLocation(Type type)
        {
            if (typeLocations.TryGetValue(type, out var getLocation))
                return getLocation(type);
            return string.Empty;
        }

        public ITypeBuildingContext ResolveType(string initialUnitPath, Type type, ITypeScriptUnitFactory unitFactory)
        {
            if (typeRedirects.TryGetValue(type, out var redirect))
                return TypeBuilding.RedirectToType(redirect.Name, redirect.Location, type);
            if (typeBuildingContexts.TryGetValue(type, out var createContext))
                return createContext(type);
            return null;
        }

        public TypeScriptTypeMemberDeclaration ResolveProperty(TypeScriptUnit unit, ITypeGenerator typeGenerator, Type type, PropertyInfo property)
        {
            return null;
        }

        public CustomTypeGenerator WithTypeLocation<T>(Func<Type, string> getLocation)
        {
            typeLocations[typeof(T)] = getLocation;
            return this;
        }

        public CustomTypeGenerator WithTypeRedirect<T>(string name, string location)
        {
            typeRedirects[typeof(T)] = new Redirect {Name = name, Location = location};
            return this;
        }

        public CustomTypeGenerator WithTypeBuildingContext<T, TContext>(Func<Type, TContext> createContext)
            where TContext : ITypeBuildingContext
        {
            typeBuildingContexts[typeof(T)] = t => createContext(t);
            return this;
        }

        [NotNull]
        public static ICustomTypeGenerator Null => new NullCustomTypeGenerator();

        private readonly Dictionary<Type, Func<Type, string>> typeLocations = new Dictionary<Type, Func<Type, string>>();
        private readonly Dictionary<Type, Redirect> typeRedirects = new Dictionary<Type, Redirect>();
        private readonly Dictionary<Type, Func<Type, ITypeBuildingContext>> typeBuildingContexts = new Dictionary<Type, Func<Type, ITypeBuildingContext>>();
    }
}