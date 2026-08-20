using DataService.Business.IO.Tenant;
using DataService.Business.Repository.Entity.Tenant;
using DataService.Business.Tools;

namespace DataService.Business.Mapper.Tenant;

public class TenantMapper(
    SwissKnife swissKnife, 
    EntityMapper entityMapper) 
{
    private readonly EntityMapper _entityMapper = entityMapper;

    public TenantView MapTenantViewFrom(TenantModel tenant) =>
       new TenantView
       {
           TenantId = $"{tenant.TenantId}",
           Name = tenant.Name,
           Code = tenant.Code,
           LegalName = tenant.LegalName,
           TaxCode = tenant.TaxCode,
           Country = tenant.Country,
           TimeZone = tenant.TimeZone,
           Locale = tenant.Locale,
           IndustryType = tenant.IndustryType,
           IsActive = tenant.IsActive,
           LicenseType = tenant.LicenseType,
           LicenseExpiresAtUTC = tenant.LicenseExpiresAtUTC,
           MaxUsers = tenant.MaxUsers,
           MaxMachines = tenant.MaxMachines,
           CreatedUTCDate = tenant.CreatedUTCDate,
           UpdatedUTCDate = tenant.UpdatedUTCDate,
           CreatedBy = tenant.CreatedBy,
           ExtProps = swissKnife.DeserializeExtProps(tenant.ExtProps)
       };

    public TenantModel MapTenantFrom(TenantCommand tenant) =>
        new TenantModel
        {
            TenantId = swissKnife.GenerateGuid(),
            Name = tenant.Name,
            Code = tenant.Code,
            LegalName = tenant.LegalName,
            TaxCode = tenant.TaxCode,
            Country = tenant.Country,
            TimeZone = tenant.TimeZone,
            Locale = tenant.Locale,
            IndustryType = tenant.IndustryType,
            IsActive = tenant.IsActive,
            LicenseType = tenant.LicenseType,
            LicenseExpiresAtUTC = tenant.LicenseExpiresAtUTC,
            MaxUsers = tenant.MaxUsers,
            MaxMachines = tenant.MaxMachines,
            CreatedUTCDate = tenant.CreatedUTCDate ?? DateTime.UtcNow,
            UpdatedUTCDate = tenant.UpdatedUTCDate,
            CreatedBy = tenant.CreatedBy,
            ExtProps = swissKnife.SerializeExtProps(tenant.ExtProps)
        };
}
