namespace DataService.Business.Rules;

public record OkOrError<T>(bool Ok, T? Result = default, string? Error = null)
{
    public static implicit operator bool(OkOrError<T> okOrError) => okOrError.Ok; 
}
