namespace DataService.Domain.IO.User;

public class UserViewDTO
{
    /// <summary>
    /// External Unique Identifier of the user (the party GUID).
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// External Unique Identifier of the tenant the user belongs to.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// External Unique Identifier of the user login.
    /// </summary>
    public required string UserLoginId { get; set; }

    public string? Username { get; set; }
    public bool Enabled { get; set; }
    public bool IsSystem { get; set; }
    public bool RequirePasswordChange { get; set; }
    public string? LastLocale { get; set; }
    public string? LastTimeZone { get; set; }

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

    public string? ExternalId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedUTCDate { get; set; }
    public DateTime? UpdatedUTCDate { get; set; }
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Role type codes assigned to the user.
    /// </summary>
    public List<string> RoleCodes { get; set; } = [];

    /// <summary>
    /// External ids of the security groups currently assigned to the user.
    /// </summary>
    public List<string> SecurityGroupIds { get; set; } = [];

    public Dictionary<string, object?>? ExtProps { get; set; }
}
