using System.Data;
using Npgsql;
using DataService.Business.IO;
using DataService.Business.Rules;

namespace DataService.Adapters;

/// <summary>
/// Singleton factory for creating database connections using the configured connection string.
/// </summary>
public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DbConnection")!;
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Connection string 'DbConnection' is not configured.");
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<OkOrError<T>> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action)
    {
        try
        {
            using var connection = await CreateConnectionAsync();
            var result = await action(connection);
            return result;
        }
        catch (Exception ex)
        {
            return new OkOrError<T>(false, Error: ex.Message);
        }
    }
}

