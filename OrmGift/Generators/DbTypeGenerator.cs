using System.Linq;

namespace OrmGift.Generators
{
    internal static class DbTypeGenerator
    {
        public static string Generate(ModelType type)
        {
            return
$@"using System;
using Microsoft.Data.SqlClient;
using OrmGift.Utils;
namespace {type.Namespace}
{{
    {type.Accesibility} partial class {type.TypeName} : IEquatable<{type.TypeName}>
    {{
        public {type.TypeName}({GenerateConstructor(type)})
        {{
            {GenerateConstructorSetters(type)}
        }}

        public override string ToString()
        {{
            return {GenerateToString(type)};
        }}

        public override int GetHashCode()
        {{
            {GenerateHashCodes(type)}
        }}
        public bool Equals({type.TypeName} other)
        {{
            {GenerateEquals(type, "other")}
        }}
        public override bool Equals(object other) => other is {type.TypeName} tmp ? Equals(tmp) : false;

        public static bool operator ==({type.TypeName} left, {type.TypeName} right) => left.Equals(right);
        public static bool operator !=({type.TypeName} left, {type.TypeName} right) => !(left == right);

        public static {type.TypeName}DataContext CreateDataContext(SqlConnection cnn, bool keepAlive = false) => new {type.TypeName}DataContext(cnn, keepAlive);
    }}
}}";
        }

        private static string GenerateHashCodes(ModelType type)
        {
            return "var hash = HashCodeCombiner.Start();"
                + string.Join(" ", type.Fields.Select(f => $"hash.Add({f.Name});"))
                + "return hash.CombinedHash;";
        }

        private static string GenerateEquals(ModelType type, string otherParameter)
        {
            return "return " + string.Join(" && ", type.Fields.Select(f => $"{f.Name}.Equals({otherParameter}.{f.Name})")) + ";";
        }

        private static string GenerateToString(ModelType type)
        {
            return $"$\"{string.Join(", ", type.Fields.Select(GenerateToString))}\"";
        }

        private static string GenerateToString(ModelField field)
        {
            return $"{field.Name} = {{{field.Name}}}";
        }

        private static string GenerateConstructor(ModelType type)
        {
            return string.Join(",", type.Fields.Select(f => $"{f.Type.FullName} {f.Name.ToLower()}___"));
        }

        private static string GenerateConstructorSetters(ModelType type)
        {
            return string.Join(";", type.Fields.Select(f => $"{f.Name} = {f.Name.ToLower()}___")) + ";";
        }
    }
}
