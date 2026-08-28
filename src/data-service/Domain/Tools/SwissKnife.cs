using System.Text.Json;

namespace DataService.Domain.Tools
{
    public class SwissKnife(ILogger<SwissKnife> logger)
    {
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
        };


        public Guid GenerateGuid() => Guid.CreateVersion7(DateTimeOffset.UtcNow);

        public Dictionary<string, object?>? DeserializeExtProps(string? extPropsJson)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    extPropsJson ?? "{}", 
                    jsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError("Failed to deserialize ExtProps JSON: {ExtPropsJson}", extPropsJson);
                throw new InvalidOperationException($"Failed to deserialize ExtProps JSON: {extPropsJson}", ex);
            }
        }

        public string? SerializeExtProps(Dictionary<string, object?>? extProps)
        {
            try
            {
                return JsonSerializer.Serialize(
                    extProps ?? new Dictionary<string, object?>(), 
                    jsonOptions);

            }
            catch (JsonException ex)
            {
                logger.LogError("Failed to serialize ExtProps dictionary: {ExtProps}", extProps);
                throw new InvalidOperationException($"Failed to serialize ExtProps dictionary: {extProps}", ex);
            }
        }
    }
}
