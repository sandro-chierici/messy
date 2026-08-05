using System.Data;
using DataService.Business.Rules;

namespace DataService.Business.IO;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();

    Task<OkOrError<T>> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action);
}
