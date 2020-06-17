using System;
using System.Linq;

namespace OrmGift.Generators
{
    internal static class TypeGenerator
    {
        public static string Generate(ModelType type)
        {
            return
$@"using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using OrmGift.DataModeling;
namespace {type.Namespace}
{{
	{type.Accesibility} {type.ReadOnly} partial {type.TypeVariant} {type.TypeName}
	{{
		public {type.TypeName}({GenerateConstructor(type)})
		{{
			{GenerateConstructorSetters(type)}
		}}

		public override string ToString()
        {{
            return {GenerateToString(type)};
        }}

		public static {type.TypeName}DataContext CreateDataContext(SqlConnection cnn, bool keepAlive = false) => new {type.TypeName}DataContext(cnn, keepAlive);
	}}

	public sealed class {type.TypeName}DataContext : IDisposable
	{{
		private readonly SqlConnection _connection;
		private readonly bool _keepAlive;

		internal {type.TypeName}DataContext(SqlConnection connection, bool keepAlive)
		{{
			_connection = connection;
			_keepAlive = keepAlive;
		}}

		public void Dispose()
		{{
			if (!_keepAlive)
			{{
				_connection?.Dispose();
			}}
		}}

		public async Task<{type.TypeName}> GetAsync({type.Key.Type.FullName} {type.Key.Name}, CancellationToken cancellationToken = default)
		{{
			try
            {{
                await _connection.OpenAsync(cancellationToken);
                using SqlCommand command = new SqlCommand(""SELECT * FROM {type.TypeName} WHERE {type.Key.Name} = @{type.Key.Name}"", _connection);
				command.Parameters.Add(new SqlParameter(""@{type.Key.Name}"", {type.Key.Name}));
                using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.SingleResult, cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {{
					{GenerateReaders(type)}
                    return new {type.TypeName}({GenerateConstructorParameters(type)});
                }}

                while (await reader.NextResultAsync(cancellationToken))
                {{
                }}

                return default;
            }}
            catch
            {{
                return default;
            }}
		}}

		public async Task<List<{type.TypeName}>> GetAllAsync(CancellationToken cancellationToken = default)
        {{
            try
            {{
                await _connection.OpenAsync(cancellationToken);
                using SqlCommand command = new SqlCommand(""SELECT * FROM {type.TypeName}"", _connection);
                using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow, cancellationToken);

                var buffer = new List<{type.TypeName}>();
                while (await reader.ReadAsync(cancellationToken))
                {{
					{GenerateReaders(type)}
                    buffer.Add(new {type.TypeName}({GenerateConstructorParameters(type)}));
                }}

                while (await reader.NextResultAsync(cancellationToken))
                {{
                }}

                return buffer;
            }}
            catch
            {{
                return default;
            }}
        }}
	}}
}}";
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
            return string.Join(",", type.Fields.Select(f => $"{f.Type.FullName} _{f.Name.ToLower()}"));
        }

        private static string GenerateConstructorSetters(ModelType type)
        {
            return string.Join(";", type.Fields.Select(f => $"{f.Name} = _{f.Name.ToLower()}")) + ";";
        }

        private static string GenerateConstructorParameters(ModelType type)
        {
            return string.Join(",", type.Fields.Select(f => f.Name.ToLower()));
        }

        private static string GenerateReaders(ModelType type)
        {
            return string.Join("\n", type.Fields.Select(GenerateFieldReader));
        }

        private static string GenerateFieldReader(ModelField field)
        {
            return $@"var {field.Name.ToLower()} = {GenerateReaderPerType(field)};";
        }

        private static string GenerateReaderPerType(ModelField field)
        {
            var ordinalField = $"\"{field.Name}\"";
            var type = field.Type.FullName;
            switch (type)
            {
                case "System.Boolean":
                    return $"reader.GetBoolean({ordinalField})";
                case "System.Byte":
                    return $"reader.GetByte({ordinalField})";
                case "System.DateTime":
                    return $"reader.GetDateTime({ordinalField})";
                case "System.Decimal":
                    return $"reader.GetDecimal({ordinalField})";
                case "System.Double":
                    return $"reader.GetDouble({ordinalField})";
                case "System.Guid":
                    return $"reader.GetGuid({ordinalField})";
                case "System.Int16":
                    return $"reader.GetInt16({ordinalField})";
                case "System.Int32":
                    return $"reader.GetInt32({ordinalField})";
                case "System.Int64":
                    return $"reader.GetInt64({ordinalField})";
                case "System.SByte":
                    return $"reader.GetFieldValue<SByte>({ordinalField})";
                case "System.Single":
                    return $"reader.GetSingle({ordinalField})";
                case "System.String":
                    return $"reader.GetString({ordinalField})";
                case "System.UInt16":
                    return $"reader.GetFieldValue<UInt16>({ordinalField})";
                case "System.UInt32":
                    return $"reader.GetFieldValue<UInt32>({ordinalField})";
                case "System.UInt64":
                    return $"reader.GetFieldValue<UInt64>({ordinalField})";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
