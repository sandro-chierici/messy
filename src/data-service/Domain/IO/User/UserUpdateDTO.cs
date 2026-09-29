namespace DataService.Domain.IO.User;

/// <summary>
/// Full replacement of the mutable user data. Roles and security groups have their own endpoints.
/// </summary>
public record UserUpdateDTO
{
    /// <summary>
    /// New password (min 8 chars). Null keeps the current one.
    /// </summary>
    public string? Password { get; set; }

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
    public bool Enabled { get; set; } = true;
    public bool RequirePasswordChange { get; set; }
    public string? LastLocale { get; set; }
    public string? LastTimeZone { get; set; }
    public Dictionary<string, object?>? ExtProps { get; set; }
}
