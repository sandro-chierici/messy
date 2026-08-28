using System.Data;
using DataService.Domain.Rules;

namespace DataService.Domain.IO;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();

    Task<OkOrError<T>> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action);
}
