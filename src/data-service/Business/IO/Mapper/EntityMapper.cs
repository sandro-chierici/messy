using DataService.Business.IO.DataView;
using DataService.Business.Repository.Entity.Tenant;

namespace DataService.Business.IO.Mapper;

public class EntityMapper
{
    public TenantView MapTenantViewFrom(Tenant tenant, IEnumerable<TenantExt>? exts = null)
    {
        var view = new TenantView
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
            CreatedBy = tenant.CreatedBy
        };

        if (exts != null)
            view.CustomProperties = exts
                .Where(ext => !ext.IsDeleted)
                .ToDictionary(ext => ext.Name, ext => ext.Value);

        return view;
    }   
}
