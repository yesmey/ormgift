using System;
using System.Linq;

namespace OrmGift.Generators
{
    internal static class DbTypeContextGenerator
    {
        public static string Generate(ModelType type)
        {
            return
$@"using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
namespace {type.Namespace}
{{
    public sealed class {type.TypeName}DataContext : IDisposable, IAsyncDisposable
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

        public ValueTask DisposeAsync()
        {{
            Dispose();
            return default;
        }}

        public async ValueTask<{type.TypeName}> GetAsync({type.Key.Type.FullName} key, CancellationToken cancellationToken = default)
        {{
            bool wasClosed = _connection.State == ConnectionState.Closed;

            try
            {{
                if (wasClosed) await _connection.OpenAsync(cancellationToken);
                using SqlCommand command = new SqlCommand(""SELECT * FROM {GetTableName(type)} WHERE {type.Key.Name} = @{type.Key.Name}"", _connection);
                command.Parameters.Add(new SqlParameter(""@{type.Key.Name}"", key));
                using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.SingleResult, cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {{
                    {GenerateReaders(type)}
                    return new {type.TypeName}({GenerateConstructorParameters(type)});
                }}

                return default;
            }}
            finally
            {{
                if (wasClosed) _connection.Close();
            }}
        }}

        public async ValueTask<int> InsertAsync({type.TypeName} item, CancellationToken cancellationToken = default)
        {{
            bool wasClosed = _connection.State == ConnectionState.Closed;

            try
            {{
                if (wasClosed) await _connection.OpenAsync(cancellationToken);
                using SqlCommand command = new SqlCommand(""INSERT INTO {GetTableName(type)}({string.Join(",", type.Fields.Select(f => f.Name))}) VALUES ({string.Join(",", type.Fields.Select(f => $"@{f.Name}"))})"", _connection);
                {GenerateDbParameters(type, "item")}
                return await command.ExecuteNonQueryAsync(cancellationToken);
            }}
            finally
            {{
                if (wasClosed) _connection.Close();
            }}
        }}

        public async ValueTask<List<{type.TypeName}>> GetAllAsync(CancellationToken cancellationToken = default)
        {{
            bool wasClosed = _connection.State == ConnectionState.Closed;

            try
            {{
                if (wasClosed) await _connection.OpenAsync(cancellationToken);
                using SqlCommand command = new SqlCommand(""SELECT * FROM {GetTableName(type)}"", _connection);
                using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);

                var buffer = new List<{type.TypeName}>();
                while (await reader.ReadAsync(cancellationToken))
                {{
                    {GenerateReaders(type)}
                    buffer.Add(new {type.TypeName}({GenerateConstructorParameters(type)}));
                }}

                return buffer;
            }}
            finally
            {{
                if (wasClosed) _connection.Close();
            }}
        }}
    }}
}}";
        }

        private static string GenerateDbParameters(ModelType type, string variableName)
        {
            return string.Join(" ", type.Fields.Select(f => $"command.Parameters.Add(new SqlParameter(\"@{f.Name}\", {variableName}.{f.Name}));"));
        }

        private static string GenerateConstructorParameters(ModelType type)
        {
            return string.Join(",", type.Fields.Select(f => $"{f.Name.ToLower()}___"));
        }

        private static string GenerateReaders(ModelType type)
        {
            return string.Join(" ", type.Fields.Select(GenerateFieldReader));
        }

        private static string GenerateFieldReader(ModelField field)
        {
            return $@"var {field.Name.ToLower()}___ = {GenerateReaderPerType(field)};";
        }

        private static string GetTableName(ModelType type)
        {
            return type.TableInfo.Name;
        }

        private static string GenerateReaderPerType(ModelField field)
        {
            var fieldName = $"\"{field.Name}\"";
            return field.Type.FullName switch
            {
                "System.Boolean" => $"reader.GetBoolean({fieldName})",
                "System.DateTime" => $"reader.GetDateTime({fieldName})",
                "System.Decimal" => $"reader.GetDecimal({fieldName})",
                "System.Double" => $"reader.GetDouble({fieldName})",
                "System.Single" => $"reader.GetSingle({fieldName})",
                "System.Guid" => $"reader.GetGuid({fieldName})",
                "System.Byte" => $"reader.GetByte({fieldName})",
                "System.SByte" => $"(SByte)reader.GetByte({fieldName})",
                "System.Int16" => $"reader.GetInt16({fieldName})",
                "System.UInt16" => $"(UInt16)reader.GetInt16({fieldName})",
                "System.Int32" => $"reader.GetInt32({fieldName})",
                "System.UInt32" => $"(UInt32)reader.GetInt32({fieldName})",
                "System.Int64" => $"reader.GetInt64({fieldName})",
                "System.UInt64" => $"(UInt64)reader.GetInt64({fieldName})",
                "System.String" => $"reader.GetString({fieldName})",
                "System.Object" => $"reader.GetValue({fieldName})",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
