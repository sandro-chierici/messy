using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DataService.Domain.Rules;

/// <summary>
/// Singleton
/// </summary>
public partial class InputValidator()
{
    private const string SAFE_INPUT = @"\b(SELECT|FROM|INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|EXEC|UNION|GRANT|REVOKE)\b";

    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    [GeneratedRegex(@"^[A-Za-z0-9._@\-]{3,100}$")]
    private static partial Regex UsernameRegex();

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

    /// <summary>
    /// Validates an external id: it must be a non empty GUID.
    /// </summary>
    public OkOrError<Guid> SanitizeGuid(string? id)
    {
        var safeId = SanitizeId(id, maxLen: 36);
        if (!safeId)
            return new OkOrError<Guid>(false, Error: safeId.Error);

        if (!Guid.TryParse(id, out var guid) || guid == Guid.Empty)
            return new OkOrError<Guid>(false, Error: "Id is not a valid GUID");

        return guid;
    }

    public OkOrError<string> ValidateUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username) || !UsernameRegex().IsMatch(username))
            return new OkOrError<string>(false, Error: "Username must be 3-100 chars: letters, digits, '.', '_', '@', '-'");

        return username;
    }

    public OkOrError<string> ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
            return new OkOrError<string>(false, Error: $"Password must be {MinPasswordLength}-{MaxPasswordLength} chars long");

        return password;
    }
}
