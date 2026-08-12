using System.Text.Json;

namespace DataService.Business.Tools
{
    public class SwissKnife(ILogger<SwissKnife> logger)
    {
        public Guid GenerateGuid() => Guid.CreateVersion7(DateTimeOffset.UtcNow);

        public Dictionary<string, object?>? DeserializeExtProps(string? extPropsJson)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(extPropsJson ?? "{}");
            }
            catch (JsonException)
            {
                logger.LogError("Failed to deserialize ExtProps JSON: {ExtPropsJson}", extPropsJson);
                return new Dictionary<string, object?>();
            }
        }
        public string? SerializeExtProps(Dictionary<string, object?>? extProps)
        {
            try
            {
                return JsonSerializer.Serialize(extProps ?? new Dictionary<string, object?>());
            }
            catch (JsonException)
            {
                logger.LogError("Failed to serialize ExtProps dictionary: {ExtProps}", extProps);
                return "{}";
            }
        }
    }
}
