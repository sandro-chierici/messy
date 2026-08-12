using DataService.Business.IO.DataCommand;
using DataService.Business.IO.DataView;
using DataService.Business.Repository.Entity.Tenant;
using DataService.Business.Tools;
using System.Text.Json;

namespace DataService.Business.IO.Mapper;

public class EntityMapper(SwissKnife swissKnife)
{
    public TenantView MapTenantViewFrom(Tenant tenant)
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
            CreatedBy = tenant.CreatedBy,
            ExtProps = swissKnife.DeserializeExtProps(tenant.ExtProps)
        };

        return view;
    }  
}
