using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace IdentityService.Infrastructure;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class DatabaseConnectionFactory(IConfiguration config) : IDbConnectionFactory
{
    private readonly string _connectionString = ResolvePostgres(config);

    private static string ResolvePostgres(IConfiguration config)
    {
        var cs = config.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Connection string 'Postgres' lipsește sau este goală (seteaz-o prin user-secrets).");
        return cs;
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
