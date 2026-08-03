namespace DataService.Business.IO.DataView;

public record TenantView
{
    public required string Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public DateTime? CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }

}
