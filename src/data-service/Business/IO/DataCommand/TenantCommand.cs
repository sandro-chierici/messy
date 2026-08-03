namespace DataService.Business.IO.DataCommand;

public record TenantCommand
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public DateTime? CreateDate { get; init; }
    public DateTime? UpdateDate { get; init; }

}
