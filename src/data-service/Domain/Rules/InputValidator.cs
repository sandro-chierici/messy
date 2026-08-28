using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DataService.Domain.Rules;

/// <summary>
/// Singleton
/// </summary>
public class InputValidator()
{
    private const string SAFE_INPUT = @"\b(SELECT|FROM|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC|UNION|GRANT|REVOKE)\b";

    public OkOrError<string?> SanitizeId(string? id, bool nullable = false, int maxLen = 50)
    {
        if (id == null)
        {
            if (!nullable)
                return new OkOrError<string?>(false, null, "Id is null");
            else
                return new OkOrError<string?>(true);
        }

        if (id.Length > maxLen)
            return new OkOrError<string?>(false, id, $"Id is too long, max {maxLen} char");

        //prevent xss
        if (Regex.IsMatch(id, SAFE_INPUT, RegexOptions.IgnoreCase))
            return new OkOrError<string?>(false, null, $"Id seems not safe to elaborate (XSS scripting)");

        return new OkOrError<string?>(true, id);
    }
}
