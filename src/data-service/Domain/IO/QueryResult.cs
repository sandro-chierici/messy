namespace DataService.Domain.IO;

public class QueryResult
{
    public int TotalCount { get; set; }
    public int ItemCount { get; set; }
    public object[] Items { get; set; } = Array.Empty<object>();

    public static QueryResult Create(int totalCount = 0, params object[] items)
    {
        return new QueryResult
        {
            TotalCount = totalCount > 0 ? totalCount : items.Length,
            ItemCount = items.Length,
            Items = items ?? Array.Empty<object>()
        };
    }
}
