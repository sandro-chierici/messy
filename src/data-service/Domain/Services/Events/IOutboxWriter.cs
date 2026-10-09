using System.Data;

namespace DataService.Domain.Services.Events;

/// <summary>
/// Stores an event in the outbox using the caller's connection and transaction,
/// so the event is committed or rolled back together with the business change.
/// </summary>
public interface IOutboxWriter
{
    Task AddAsync(IDbConnection connection, IDbTransaction transaction, EventBase @event);
}
