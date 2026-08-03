namespace DataService.Business.Rules;

public record OkOrError<T>(bool Ok, T? Value = default, string? Error = null)
{
    public static implicit operator bool(OkOrError<T> okOrError) => okOrError.Ok;
    public static implicit operator T?(OkOrError<T> okOrError) => okOrError.Value;
}
