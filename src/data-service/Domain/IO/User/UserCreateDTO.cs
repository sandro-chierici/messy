namespace DataService.Domain.IO.User;

public record UserCreateDTO
{
    /// <summary>
    /// Login name, unique inside the tenant (3-100 chars: letters, digits, . _ @ -).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Initial password (min 8 chars). Stored hashed, never returned.
    /// </summary>
    public string? Password { get; set; }

    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Nickname { get; set; }
    public string? PersonalTitle { get; set; }
    public string? Suffix { get; set; }

    /// <summary>
    /// Single letter gender code.
    /// </summary>
    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }
    public string? Comments { get; set; }

    /// <summary>
    /// Identifier of the user in an external system.
    /// </summary>
    public string? ExternalId { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Whether the user is enabled at creation.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Force a password change at first login.
    /// </summary>
    public bool RequirePasswordChange { get; set; }

    public string? LastLocale { get; set; }
    public string? LastTimeZone { get; set; }

    /// <summary>
    /// Role type codes (e.g., "OPERATOR").
    /// </summary>
    public List<string>? RoleCodes { get; set; }

    /// <summary>
    /// External ids (GUID) of the security groups to assign.
    /// </summary>
    public List<string>? SecurityGroupIds { get; set; }

    /// <summary>
    /// Additional properties for extensibility (e.g., custom fields).
    /// </summary>
    public Dictionary<string, object?>? ExtProps { get; set; }
}
