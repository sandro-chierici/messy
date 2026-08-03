using System.Data;

namespace DataService.Adapters;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
