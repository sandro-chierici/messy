namespace DataService.Domain.Repository.Models;

/// <summary>
/// OFBiz SecurityPermission. Reference table: identified by its natural code, no external GUID.
/// </summary>
public class SecurityPermission
{
    public int Id { get; set; }

    /// <summary>
    /// Natural code (e.g., "USER_VIEW").
    /// </summary>
    public required string Code { get; set; }

    public string? Description { get; set; }
}
