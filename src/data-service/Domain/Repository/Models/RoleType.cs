namespace DataService.Domain.Repository.Models;

/// <summary>
/// OFBiz RoleType. Reference table: identified by its natural code, no external GUID.
/// </summary>
public class RoleType
{
    public int Id { get; set; }

    /// <summary>
    /// Natural code (e.g., "EMPLOYEE", "OPERATOR").
    /// </summary>
    public required string Code { get; set; }

    public int? ParentPk { get; set; }

    /// <summary>
    /// Code of the parent role type (read model, joined).
    /// </summary>
    public string? ParentCode { get; set; }

    public string? Description { get; set; }
}
