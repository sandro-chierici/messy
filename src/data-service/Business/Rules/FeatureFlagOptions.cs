namespace DataService.Business.Rules;

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
        public int ConnectionTimeoutMillis { get; set; }
    }

    public DatabaseFeature Database { get; set; } = new DatabaseFeature { ConnectionTimeoutMillis = 20000 };

}
