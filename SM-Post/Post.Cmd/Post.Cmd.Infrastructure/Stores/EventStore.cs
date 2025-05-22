using CQRS.Core.Domain;
using CQRS.Core.Events;
using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Post.Cmd.Domain.Aggregates;
using ZstdSharp.Unsafe;

namespace Post.Cmd.Infrastructure.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IEventStoreRepository _repo;

        public EventStore(IEventStoreRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<BaseEvent>> GetEventsAsync(Guid aggregateId)
        {
            var eventStream = await _repo.FindByAggregateId(aggregateId);

            if (eventStream == null || !eventStream.Any())
                throw new AggregateNotFoundException("Incorrect post ID provided");

            return eventStream.OrderBy(e => e.Version).
                Select(e => e.EventData).
                ToList();
        }

        public async Task SaveEventsAsync(
            Guid aggregateId, 
            IEnumerable<BaseEvent> events, 
            int expectedVersion
        )
        {
            var eventStream = await _repo.FindByAggregateId(aggregateId);

            if (expectedVersion != -1 && eventStream[^1].Version != expectedVersion)
                throw new ConcurrencyException(); 

            var version = expectedVersion;

            foreach (var @event in events)
            {
                version++;
                @event.Version = version;
                var eventType = @event.GetType().Name;

                var eventModel = new EventModel{
                    TimeStamp = DateTime.Now,
                    AggregateIdentifier = aggregateId,
                    AggregateType = nameof(PostAggregate),
                    Version = version,
                    EventType = eventType,
                    EventData = @event
                };

                await _repo.SaveAsync(eventModel);
            }
        }
    }
}