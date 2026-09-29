namespace DataService.Domain.Repository.Models;

/// <summary>
/// OFBiz SecurityGroup. A null <see cref="TenantPk"/> marks a built-in group visible to every tenant.
/// </summary>
public class SecurityGroup
{
    public int Id { get; set; }

    /// <summary>
    /// External Unique Identifier (GUID v7) of the group.
    /// </summary>
    public required Guid SecurityGroupId { get; set; }

    public int? TenantPk { get; set; }

    /// <summary>
    /// External id of the owning tenant (read model, joined). Null for built-in groups.
    /// </summary>
    public Guid? TenantId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}
