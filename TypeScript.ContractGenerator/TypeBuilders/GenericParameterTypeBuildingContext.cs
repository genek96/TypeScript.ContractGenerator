using System.Reflection;

using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.CodeDom;

namespace SkbKontur.TypeScript.ContractGenerator.TypeBuilders
{
    public class GenericParameterTypeBuildingContext : ITypeBuildingContext
    {
        public GenericParameterTypeBuildingContext(ITypeInfo type)
        {
            this.type = type;
        }

        public bool IsDefinitionBuilt => true;

        public void Initialize(ITypeGenerator typeGenerator)
        {
        }

        public void BuildDefinition(ITypeGenerator typeGenerator)
        {
        }

        public TypeScriptType ReferenceFrom(TypeScriptUnit targetUnit, ITypeGenerator typeGenerator, ICustomAttributeProvider customAttributeProvider)
        {
            return new TypeScriptTypeReference(type.Name);
        }

        private readonly ITypeInfo type;
    }
}