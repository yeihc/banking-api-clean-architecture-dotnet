using System;

namespace BankingApi.Domain.Events
{
    /// <summary>
    /// Base contract for Domain Events.
    ///
    /// Domain Events represent immutable facts that happened in the past.
    /// They are collected by aggregates (Pull Model) and processed by the application layer.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// When the event occurred (UTC).
        /// </summary>
        DateTime OccurredAt { get; }

        /// <summary>
        /// Aggregate identifier that emitted the event.
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Technical event name for logging/dispatching.
        /// </summary>
        string Type { get; }
    }
}