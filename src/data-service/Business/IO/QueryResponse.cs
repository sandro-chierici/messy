namespace DataService.Business.IO;

public class QueryResponse
{
    public int TotalCount { get; set; }
    public int ItemCount { get; set; }
    public object[] Items { get; set; } = Array.Empty<object>();

    public static QueryResponse Create(int totalCount = 0, params object[] items)
    {
        return new QueryResponse
        {
            TotalCount = totalCount > 0 ? totalCount : items.Length,
            ItemCount = items.Length,
            Items = items ?? Array.Empty<object>()
        };
    }
}
