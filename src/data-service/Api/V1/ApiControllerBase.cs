using DataService.Domain.IO;
using DataService.Domain.Rules;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1;

public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Maps a failed result to 404 when the error reports a missing resource, 400 otherwise.
    /// </summary>
    protected IActionResult Failure<T>(OkOrError<T> result, string fallbackMessage)
    {
        var message = result.Error ?? fallbackMessage;

        return message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(ApiV1Response.Failure(message))
            : BadRequest(ApiV1Response.Failure(message));
    }
}
