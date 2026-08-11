namespace DataService.Business.Repository.Entity.Tenant;

/// <summary>
/// Customizzazioni della tabella Tenant per estendere le informazioni del tenant con campi aggiuntivi.
/// </summary>
public class TenantExt : IEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    public long Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public CustomFieldType Type { get; set; } = CustomFieldType.String;
    public string? Value { get; set; }
    public bool IsDeleted { get; set; }
}