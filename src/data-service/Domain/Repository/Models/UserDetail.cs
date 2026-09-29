namespace DataService.Domain.Repository.Models;

/// <summary>
/// Read model: a user as the join of Party, Person, UserLogin and Tenant.
/// Only external ids are carried; no internal primary keys.
/// </summary>
public class UserDetail
{
    public required Guid PartyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserLoginId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool IsSystem { get; set; }
    public bool RequirePasswordChange { get; set; }
    public string? LastLocale { get; set; }
    public string? LastTimeZone { get; set; }

    public string? ExternalId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedUTCDate { get; set; }
    public DateTime? UpdatedUTCDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? ExtProps { get; set; }

    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Nickname { get; set; }
    public string? PersonalTitle { get; set; }
    public string? Suffix { get; set; }
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Comments { get; set; }
}
