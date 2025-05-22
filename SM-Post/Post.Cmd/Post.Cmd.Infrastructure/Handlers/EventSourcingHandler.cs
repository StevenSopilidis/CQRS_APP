using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>
    {
        private readonly IEventStore _store;

        public EventSourcingHandler(IEventStore store)
        {
            _store = store;
        }

        public async Task<PostAggregate> GetByIdAsync(Guid aggregateId)
        {
            var aggregate = new PostAggregate();
            var events = await _store.GetEventsAsync(aggregateId);

            if (events == null || !events.Any())
                return aggregate;

            aggregate.ReplayEvents(events);
            aggregate.Version = events.Select(e => e.Version).Max();
            return aggregate;
        }

        public async Task SaveAsync(AggregateRoot aggregate)
        {
            await _store.SaveEventsAsync(
                aggregate.Id, 
                aggregate.GetUncommitedChanges(),
                aggregate.Version 
            );

            aggregate.MarkChangesAsCommited();
        }
    }
}