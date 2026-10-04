namespace DataService.Infrastructure.Repository;

public abstract class BaseRepository
{
    protected string ComposeArrayParameter<T>(IReadOnlyCollection<T> values)
        => values switch
        {
            IReadOnlyCollection<Guid> guidValues when guidValues.Count > 0 => $"({string.Join(",", guidValues.Select(v => $"'{v}'"))})::uuid[]",
            IReadOnlyCollection<Guid> guidValues when guidValues.Count == 0 => $"()::uuid[]",
            IReadOnlyCollection<string> stringValues => $"({string.Join(",", stringValues.Select(v => $"'{v}'"))})",
            IReadOnlyCollection<int> intValues => $"({string.Join(",", intValues)})",
            _ => throw new ArgumentException($"Unsupported type {typeof(T)} for array parameter composition.")
        };
}

