using System;
using System.Linq;

using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.CodeDom;
using SkbKontur.TypeScript.ContractGenerator.Internals;

namespace SkbKontur.TypeScript.ContractGenerator
{
    public static class TypeScriptGeneratorHelpers
    {
        public static TypeScriptType BuildAndImportType(this ITypeGenerator typeGenerator, TypeScriptUnit typeScriptUnit, ITypeInfo type)
        {
            return typeGenerator.ResolveType(type).ReferenceFrom(type, typeScriptUnit, typeGenerator);
        }

        public static bool NullableReferenceCanBeNull(IAttributeProvider? attributeProvider, ITypeInfo type, int index)
        {
            var nullableBytes = GetNullableFlags(attributeProvider);
            if (nullableBytes.Length == 1 && nullableBytes[0] == 2 || nullableBytes.Length > index && nullableBytes[index] == 2)
                return !type.IsValueType;
            return false;
        }

        private static byte[] GetNullableFlags(IAttributeProvider? attributeProvider)
        {
            byte contextFlag = 0;
            if (attributeProvider is PropertyWrapper propertyInfo)
                contextFlag = GetNullableContextFlag(propertyInfo) ?? GetNullableContextFlag(TypeInfo.From(propertyInfo.Property.ReflectedType)) ?? 0;
            if (attributeProvider is MethodWrapper methodInfo)
                contextFlag = GetNullableContextFlag(methodInfo) ?? GetNullableContextFlag(TypeInfo.From(methodInfo.Method.ReflectedType)) ?? 0;
            return GetNullableFlagsInternal(attributeProvider) ?? new[] {contextFlag};
        }

        public static byte[]? GetNullableFlagsInternal(this IAttributeProvider? attributeProvider)
        {
            var nullableAttribute = attributeProvider?.GetAttributes(true).SingleOrDefault(a => a.AttributeType.Name == AnnotationsNames.Nullable);
            return nullableAttribute?.AttributeData["NullableFlags"] as byte[];
        }

        public static byte? GetNullableContextFlag(this IAttributeProvider? attributeProvider)
        {
            var nullableAttribute = attributeProvider?.GetAttributes(true).SingleOrDefault(a => a.AttributeType.Name == AnnotationsNames.NullableContext);
            return (byte?)nullableAttribute?.AttributeData["Flag"];
        }

        public static TypeScriptType BuildTargetNullableTypeByOptions(TypeScriptType innerType, bool isNullable, TypeScriptGenerationOptions options)
        {
            if (!(innerType is INullabilityWrapperType) && isNullable && options.NullabilityMode != NullabilityMode.None)
            {
                if (!options.UseGlobalNullable)
                    return new TypeScriptOrNullType(innerType);

                if (options.UseGlobalNullable)
                    return new TypeScriptNullableType(innerType);
            }

            return innerType;
        }

        public static int GetGenericArgumentsToSkip(ITypeInfo type)
        {
            if (type.IsArray)
                return 1 + GetGenericArgumentsToSkip(type.GetElementType());

            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(TypeInfo.From(typeof(Nullable<>))))
                return 0;

            if (!type.IsGenericType)
                return type.IsValueType ? 0 : 1;

            var count = 1;
            foreach (var argument in type.GetGenericArguments())
                count += GetGenericArgumentsToSkip(argument);

            return count;
        }
    }
}