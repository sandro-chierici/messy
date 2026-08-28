using DataService.Domain.IO.Tenant;
using DataService.Domain.Repository.Models;
using DataService.Domain.Tools;

namespace DataService.Domain.Mapper.Tenant;

public class TenantMapper(
    SwissKnife swissKnife) 
{
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

    public TenantModel MapTenantFrom(TenantCommand tenant, Guid tenantId) =>
        new TenantModel
        {
            TenantId = tenantId,
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
