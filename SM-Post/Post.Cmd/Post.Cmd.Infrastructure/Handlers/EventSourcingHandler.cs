using CQRS.Core.Domain;
using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using CQRS.Core.Producers;
using Post.Cmd.Domain.Aggregates;

namespace Post.Cmd.Infrastructure.Handlers
{
    public class EventSourcingHandler : IEventSourcingHandler<PostAggregate>
    {
        private readonly IEventStore _store;
        private readonly IEventProducer _producer;

        public EventSourcingHandler(IEventStore store, IEventProducer producer)
        {
            _store = store;
            _producer = producer;
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

        public async Task RepublishEventsAsync()
        {
            var aggregateIds = await _store.GetAggregateIdsAsync();

            if (aggregateIds is null || aggregateIds.Count() == 0)
                return;

            foreach(var aggregateId in aggregateIds) {
                var aggregate = await GetByIdAsync(aggregateId);
                if (aggregate is null || !aggregate.Active)
                    continue;

                var events = await _store.GetEventsAsync(aggregateId);    
                foreach (var @event in events) {
                    var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
                    await _producer.ProduceAsync(topic ,@event);
                }
            }
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