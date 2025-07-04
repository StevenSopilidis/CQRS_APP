using System.Text.Json;
using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Events;
using Microsoft.Extensions.Options;
using Post.Query.Infrastructure.Converters;
using Post.Query.Infrastructure.Handlers;

namespace Post.Query.Infrastructure.Consumers
{
    public class EventConsumer : IEventConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly IEventHandler _eventHandler;

        public EventConsumer(
            IOptions<ConsumerConfig> config,
            IEventHandler eventHandler
        )
        {
            _config = config.Value;
            _eventHandler = eventHandler;
        }

        public void Consume(string topic)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();

            consumer.Subscribe(topic);
            while (true)
            {
                var consumeResult = consumer.Consume();
                if (consumeResult?.Message == null)
                    continue;
                
                var opts = new JsonSerializerOptions{
                    Converters= {new EventJsonConverter()}
                };
                var @event = JsonSerializer.Deserialize<BaseEvent>(
                    consumeResult.Message.Value, opts
                );

                // get correct handler for event received
                var handler = _eventHandler.GetType().GetMethod("On", new Type[]{@event.GetType()});
                if (handler is null)
                    throw new ArgumentNullException(
                        nameof(handler), 
                        "Could not find event handler method"
                    );

                handler.Invoke(_eventHandler, [@event]);
                consumer.Commit(consumeResult);
            }
        }
    }
}