namespace DataService.Domain.IO;

public record ApiV1Response(object? Data = null, bool Success = true, string? Message = null)
{
    public static ApiV1Response Failure(string message) => new ApiV1Response(Data: null, Success: false, Message: message);
    public static ApiV1Response Read(object data) => new ApiV1Response(Data: data, Success: true, Message: "Query executed successfully");
    public static ApiV1Response Created(object data) => new ApiV1Response(Data: data, Success: true, Message: "Resource created successfully");
    public static ApiV1Response Accepted(object data) => new ApiV1Response(Data: data, Success: true, Message: "Request accepted");
    public static ApiV1Response Warning(object data, string message) => new ApiV1Response(Data: data, Success: true, Message: message);
}
