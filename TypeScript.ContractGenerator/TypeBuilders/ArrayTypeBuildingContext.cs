using System;
using System.Collections.Generic;

using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.CodeDom;
using SkbKontur.TypeScript.ContractGenerator.Extensions;
using SkbKontur.TypeScript.ContractGenerator.Internals;

namespace SkbKontur.TypeScript.ContractGenerator.TypeBuilders
{
    public class ArrayTypeBuildingContext : TypeBuildingContextBase
    {
        public ArrayTypeBuildingContext(ITypeInfo arrayType)
            : base(arrayType)
        {
        }

        public static bool Accept(ITypeInfo type)
        {
            return type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition().Equals(TypeInfo.From(typeof(List<>)));
        }

        protected override TypeScriptType ReferenceFromInternal(ITypeInfo type, TypeScriptUnit targetUnit, ITypeGenerator typeGenerator)
        {
            var attributeProvider = type.Member;
            var elementType = GetElementType(type);
            var itemType = typeGenerator.BuildAndImportType(targetUnit, elementType);
            var resultType = TypeScriptGeneratorHelpers.BuildTargetNullableTypeByOptions(
                itemType,
                CanItemBeNull(elementType, typeGenerator.Options, attributeProvider),
                typeGenerator.Options
                );
            return new TypeScriptArrayType(resultType);
        }

        private ITypeInfo GetElementType(ITypeInfo arrayType)
        {
            if (arrayType.IsArray)
                return arrayType.GetElementType() ?? throw new ArgumentNullException($"Array type's {arrayType.Name} element type is not defined");

            if (arrayType.IsGenericType && arrayType.GetGenericTypeDefinition().Equals(TypeInfo.From(typeof(List<>))))
                return arrayType.GetGenericArguments()[0];

            throw new ArgumentException("arrayType should be either Array or List<T>", nameof(arrayType));
        }

        private bool CanItemBeNull(ITypeInfo elementType, TypeScriptGenerationOptions options, IAttributeProvider? attributeProvider)
        {
            if (elementType.IsValueType || elementType.IsEnum || attributeProvider == null)
                return false;

            if (options.NullabilityMode == NullabilityMode.NullableReference)
                return TypeScriptGeneratorHelpers.NullableReferenceCanBeNull(attributeProvider, elementType, 1);

            return options.NullabilityMode == NullabilityMode.Pessimistic
                       ? !attributeProvider.IsNameDefined(AnnotationsNames.ItemNotNull)
                       : attributeProvider.IsNameDefined(AnnotationsNames.ItemCanBeNull);
        }
    }
}