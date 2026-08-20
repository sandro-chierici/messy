using DataService.Business.IO.DataCommand;
using DataService.Business.IO.DataView;
using DataService.Business.Repository.Entity.Tenant;
using DataService.Business.Tools;
using System.Reflection;

namespace DataService.Business.Mapper;

public class EntityMapper(SwissKnife swissKnife)
{
    private void StandardMapFunction(object source, object destination)
    {
        foreach (var destProp in destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destProp.CanWrite || !destProp.CanRead) continue;
            // Skip if the destination property already has a value
            if (destProp.GetValue(destination, null) != null) continue;

            var sourceProp = source.GetType().GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
            if (sourceProp == null || !sourceProp.CanRead) continue;

            // Special case ExtProps mapping
            if (string.Equals(destProp.Name, "ExtProps", StringComparison.OrdinalIgnoreCase))
            {
                switch (sourceProp.PropertyType)
                {
                    case Type sourceType when sourceType == typeof(string):
                        var deserializedValue = swissKnife.DeserializeExtProps((string?)sourceProp.GetValue(source));
                        destProp.SetValue(destination, deserializedValue);
                        continue;
                    case Type sourceType when sourceType == typeof(Dictionary<string, object?>):
                        var serializedValue = swissKnife.SerializeExtProps((Dictionary<string, object?>?)sourceProp.GetValue(source));
                        destProp.SetValue(destination, serializedValue);
                        continue;
                }
            }
            // Special case TenantId mapping from Guid to string
            if (sourceProp.PropertyType == typeof(Guid) && destProp.PropertyType == typeof(string))
            {
                var guidValue = (Guid)sourceProp.GetValue(source)!;
                destProp.SetValue(destination, guidValue.ToString());
                continue;
            }
            // All other cases where the property types are compatible
            if (destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
            {
                var value = sourceProp.GetValue(source);
                destProp.SetValue(destination, value);
            }
        }
    }

    /// <summary>
    /// Maps an object from source type to destination type.
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source) 
        where TDestination : new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        try
        {
            var destination = Activator.CreateInstance<TDestination>();
            StandardMapFunction(source, destination!);
            return destination;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} failed.", ex);
        }
    }

    /// <summary>
    /// Maps an object from source type to destination type.
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        try
        {
            StandardMapFunction(source, destination!);
            return destination;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} failed.", ex);
        }
    }

    public TenantView MapTenantViewFrom(Tenant tenant) =>
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

    public Tenant MapTenantFrom(TenantCommand tenant) =>
        new Tenant
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
