namespace DataService.Domain.IO.Security;

public class SecurityGroupViewDTO
{
    /// <summary>
    /// External Unique Identifier of the group.
    /// </summary>
    public required string SecurityGroupId { get; set; }

    /// <summary>
    /// External id of the owning tenant. Null for built-in groups.
    /// </summary>
    public string? TenantId { get; set; }

    public bool IsBuiltIn { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<string> PermissionCodes { get; set; } = [];
}
