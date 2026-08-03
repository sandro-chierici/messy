using System.ComponentModel.DataAnnotations;

namespace DataService.Business.Rules;

public class InputValidator()
{
    public OkOrError<string?> SanitizeId(string? id, bool nullable = false, int maxLen = 30)
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

        return new OkOrError<string?>(true, id);
    }
}
