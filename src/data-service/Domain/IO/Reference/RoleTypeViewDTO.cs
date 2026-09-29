namespace DataService.Domain.IO.Reference;

public class RoleTypeViewDTO
{
    public required string Code { get; set; }
    public string? ParentCode { get; set; }
    public string? Description { get; set; }
}
