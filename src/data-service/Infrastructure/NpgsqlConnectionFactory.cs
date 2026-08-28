using System.Data;
using Npgsql;
using DataService.Domain.IO;
using DataService.Domain.Rules;
using Microsoft.Extensions.Options;

namespace DataService.Infrastructure;

/// <summary>
/// Singleton factory for creating database connections using the configured connection string.
/// For parameters 
/// see https://www.npgsql.org/doc/connection-string-parameters.html
/// </summary>
public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    private readonly FeatureFlagOptions _featureFlagOptions;

    public NpgsqlConnectionFactory(IConfiguration config, IOptions<FeatureFlagOptions> options)
    {
        _connectionString = config.GetConnectionString("DbConnection")!;
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Connection string 'DbConnection' is not configured.");
        _featureFlagOptions = options.Value;
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        try
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create database connection.", ex);
        }
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

