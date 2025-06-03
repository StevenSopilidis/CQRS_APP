using CQRS.Core.Domain;

namespace CQRS.Core.Handlers 
{
    // where T ---> concrete aggregate implementation
    public interface IEventSourcingHandler<T> 
    {
        Task SaveAsync(AggregateRoot aggregate);
        Task<T> GetByIdAsync(Guid aggregateId);
        Task RepublishEventsAsync();
    }
}