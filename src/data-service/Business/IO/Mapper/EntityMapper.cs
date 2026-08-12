using DataService.Business.IO.DataCommand;
using DataService.Business.IO.DataView;
using DataService.Business.Repository.Entity.Tenant;
using DataService.Business.Tools;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace DataService.Business.IO.Mapper;

public class EntityMapper(SwissKnife swissKnife)
{
    private readonly Dictionary<(Type Source, Type Destination), Delegate> _mappings = new();

    /// <summary>
    /// Creates a mapping between two types.
    /// </summary>
    public void CreateMapping<TSource, TDestination>()
        where TDestination : new()
    {
        // Build mapping function dynamically
        var sourceParam = Expression.Parameter(typeof(TSource), "src");
        var destVar = Expression.Variable(typeof(TDestination), "dest");

        var bindings = new List<MemberBinding>();

        foreach (var destProp in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!destProp.CanWrite) continue;

            var sourceProp = typeof(TSource).GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
            if (sourceProp == null || !sourceProp.CanRead) continue;

            // Special case ExtProps mapping
            if (string.Equals(destProp.Name, "ExtProps", StringComparison.OrdinalIgnoreCase))
            {
                switch (sourceProp.PropertyType)
                {
                    case Type sourceType when sourceType == typeof(string):
                        var sourceValue = Expression.Property(sourceParam, sourceProp);
                        var deserializeMethod = typeof(SwissKnife).GetMethod(nameof(SwissKnife.DeserializeExtProps), BindingFlags.Public | BindingFlags.Instance);
                        var deserializeCall = Expression.Call(Expression.Constant(swissKnife), deserializeMethod!, sourceValue);
                        bindings.Add(Expression.Bind(destProp, deserializeCall));
                        continue;
                    case Type sourceType when sourceType == typeof(Dictionary<string, object?>):
                        var sourceDictValue = Expression.Property(sourceParam, sourceProp);
                        var serializeMethod = typeof(SwissKnife).GetMethod(nameof(SwissKnife.SerializeExtProps), BindingFlags.Public | BindingFlags.Instance);
                        var serializeCall = Expression.Call(Expression.Constant(swissKnife), serializeMethod!, sourceDictValue);
                        bindings.Add(Expression.Bind(destProp, serializeCall));
                        continue;
                }
            }

            // Special case TenantId mapping from Guid to string
            if (sourceProp.PropertyType == typeof(Guid) && destProp.PropertyType == typeof(string))
            {
                var sourceValue = Expression.Property(sourceParam, sourceProp);
                var serializeMethod = typeof(Guid).GetMethod(nameof(Guid.ToString), BindingFlags.Public | BindingFlags.Instance);
                var serializeCall = Expression.Call(sourceValue, serializeMethod!);
                bindings.Add(Expression.Bind(destProp, serializeCall));
                continue;
            }

            // All other cases where the property types are compatible
            if (destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
            {
                var sourceValue = Expression.Property(sourceParam, sourceProp);
                bindings.Add(Expression.Bind(destProp, sourceValue));
            }
        }

        var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
        var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

        _mappings[(typeof(TSource), typeof(TDestination))] = lambda.Compile();
    }

    /// <summary>
    /// Maps an object from source type to destination type.
    /// </summary>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (_mappings.TryGetValue((typeof(TSource), typeof(TDestination)), out var mapFunc))
        {
            return ((Func<TSource, TDestination>)mapFunc)(source);
        }

        throw new InvalidOperationException($"No mapping defined from {typeof(TSource).Name} to {typeof(TDestination).Name}");
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
            CreatedUTCDate = tenant.CreatedUTCDate,
            UpdatedUTCDate = tenant.UpdatedUTCDate,
            CreatedBy = tenant.CreatedBy,
            ExtProps = swissKnife.SerializeExtProps(tenant.ExtProps)
        };
}
