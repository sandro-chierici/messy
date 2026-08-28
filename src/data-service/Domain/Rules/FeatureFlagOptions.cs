namespace DataService.Domain.Rules;

/// <summary>
/// Configuration with consistent default
/// </summary>
public class FeatureFlagOptions
{
    /// <summary>
    /// Database Configuration
    /// </summary>
    /// <param name="ConnectionTimeout"></param>
    public class DatabaseFeature()
    {
    }

    public DatabaseFeature Database { get; set; } = new DatabaseFeature { };

}
