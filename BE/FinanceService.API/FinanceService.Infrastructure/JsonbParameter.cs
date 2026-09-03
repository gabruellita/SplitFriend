using System.Data;
using Dapper;
using NpgsqlTypes;

namespace FinanceService.Infrastructure;

/// <summary>
/// Ambaleaza un string JSON ca parametru Npgsql de tip jsonb (NpgsqlDbType.Jsonb),
/// ca Dapper sa-l trimita corect catre proceduri cu parametru JSONB.
/// </summary>
public sealed class JsonbParameter(string json) : SqlMapper.ICustomQueryParameter
{
    public void AddParameter(IDbCommand command, string name)
    {
        var p = (Npgsql.NpgsqlParameter)command.CreateParameter();
        p.ParameterName = name;
        p.NpgsqlDbType  = NpgsqlDbType.Jsonb;
        p.Value         = json;
        command.Parameters.Add(p);
    }
}
