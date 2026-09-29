namespace DataService.Domain.Repository.Models;

/// <summary>
/// OFBiz UserLogin: credentials of a person party.
/// </summary>
public class UserLogin
{
    public int Id { get; set; }

    /// <summary>
    /// External Unique Identifier (GUID v7) of the login.
    /// </summary>
    public required Guid UserLoginId { get; set; }

    public int PartyPk { get; set; }

    /// <summary>
    /// Primary key of the owning tenant (always equal to the tenant of the party).
    /// </summary>
    public int TenantPk { get; set; }

    /// <summary>
    /// Login name, unique (case insensitive) inside the tenant.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Password hash. Never exposed outside the repository.
    /// </summary>
    public required string PasswordHash { get; set; }

    public bool Enabled { get; set; } = true;
    public bool IsSystem { get; set; }
    public bool RequirePasswordChange { get; set; }
    public int SuccessiveFailedLogins { get; set; }
    public DateTime? DisabledUTCDate { get; set; }
    public string? LastLocale { get; set; }
    public string? LastTimeZone { get; set; }
    public string? ExternalAuthId { get; set; }
    public DateTime? CreatedUTCDate { get; set; }
    public DateTime? UpdatedUTCDate { get; set; }
    public string? CreatedBy { get; set; }
}
