namespace DataService.Domain.IO.Security;

public record SecurityGroupCreateDTO
{
    /// <summary>
    /// Group name, unique (case insensitive) inside the tenant.
    /// </summary>
    public string? Name { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Permission codes granted by the group (e.g., "USER_VIEW").
    /// </summary>
    public List<string>? PermissionCodes { get; set; }
}
