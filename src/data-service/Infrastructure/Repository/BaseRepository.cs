namespace DataService.Infrastructure.Repository;

/// <summary>
/// Base class of all repositories.
/// Array parameters are passed natively (string[], Guid[], int[]) and matched with
/// <c>= ANY(@Param)</c> / <c>&lt;&gt; ALL(@Param)</c>; never use <c>IN @Param</c> with Npgsql.
/// </summary>
public abstract class BaseRepository
{
}
