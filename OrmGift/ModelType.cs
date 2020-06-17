using OrmGift.CodeAnalysis;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using OrmGift.DataModeling;

namespace OrmGift
{
    internal sealed class ModelType
    {
        public string Accesibility { get; }
        public string Namespace { get; }
        public string TypeName { get; }
        public string TypeVariant { get; }
        public string ReadOnly { get; }
        public ModelField Key { get; }
        public IReadOnlyCollection<ModelField> Fields { get; }

        public ModelType(INamedTypeSymbol type)
        {
            Accesibility = type.DeclaredAccessibility.ToString().ToLower();
            Namespace = type.GetNamespace();
            TypeName = type.Name;
            ReadOnly = type.IsReadOnly ? "readonly" : string.Empty;
            TypeVariant = type.TypeKind.ToString().ToLower(); // can only be class or struct :p

            var properties = type.GetMembers().OfType<IPropertySymbol>().Where(SyntaxHelper.IsAutoProperty).ToArray();

            Fields = properties.Select(field => new ModelField(field)).ToArray();
            Key = properties.Where(p => p.HasAttribute(typeof(DataKeyAttribute).FullName)).Select(field => new ModelField(field)).FirstOrDefault();
            if (Key == null)
            {
                Key = Fields.FirstOrDefault(x => string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase));
                if (Key == null)
                {
                    throw new Exception("FEEEL");
                }
            }
        }
    }


    internal sealed class ModelField
    {
        public string Name { get; }
        public ModelDataType Type { get; }

        public ModelField(IPropertySymbol property)
        {
            Name = property.Name;
            Type = new ModelDataType(property.Type);
        }
    }

    internal sealed class ModelDataType
    {
        public string FullName { get; }
        public bool Nullable { get; }

        public ModelDataType(ITypeSymbol symbol)
        {
            FullName = symbol.GetFullName();

            if (symbol is INamedTypeSymbol namedSymbol && FullName == typeof(Nullable).FullName)
            {
                FullName = namedSymbol.TypeArguments[0].GetFullName();
                Nullable = true;
            }
        }

        public override string ToString()
        {
            return Nullable
                ? $"{FullName}?"
                : FullName;
        }
    }
}
