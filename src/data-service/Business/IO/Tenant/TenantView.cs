namespace DataService.Business.IO.Tenant;

public class TenantView
{
    /// <summary>
    /// External Unique Identifier for the tenant.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Display name of the tenant (plant or company name).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Short unique code used to identify the tenant (e.g., "PLANT-01").
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Official legal/registered company name.
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// VAT or Tax identification number.
    /// </summary>
    public string? TaxCode { get; set; }

    /// <summary>
    /// ISO 3166 country code (e.g., "IT", "DE", "US").
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// IANA timezone identifier (e.g., "Europe/Rome").
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Culture/locale code (e.g., "it-IT").
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// Industry sector for this tenant (e.g., "Automotive", "Food").
    /// </summary>
    public string? IndustryType { get; set; }

    /// <summary>
    /// Indicates whether this tenant is active and operational.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// License tier assigned to this tenant.
    /// </summary>
    public string? LicenseType { get; set; }

    /// <summary>
    /// License expiration date (UTC).
    /// </summary>
    public DateTime? LicenseExpiresAtUTC { get; set; }

    /// <summary>
    /// Maximum number of users allowed for this tenant.
    /// </summary>
    public int? MaxUsers { get; set; }

    /// <summary>
    /// Maximum number of machines/assets manageable by this tenant.
    /// </summary>
    public int? MaxMachines { get; set; }

    /// <summary>
    /// UTC date when the tenant was created.
    /// </summary>
    public DateTime? CreatedUTCDate { get; set; }

    /// <summary>
    /// UTC date of the last update.
    /// </summary>
    public DateTime? UpdatedUTCDate { get; set; }

    /// <summary>
    /// User who created this tenant record.
    /// </summary>
    public string? CreatedBy { get; set; }
    /// <summary>
    /// Custom fields for this tenant, stored as a JSON string. This allows for flexible extension of tenant information without altering the database schema.
    /// </summary>
    public Dictionary<string, object?>? ExtProps { get; set; }
}
