using Castle.Core.Logging;
using CQRS.Core.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Post.Query.Infrastructure.Consumers
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IServiceProvider _provider;

        public ConsumerHostedService(
            ILogger<ConsumerHostedService> logger,
            IServiceProvider provider
        )
        {
            _logger = logger;    
            _provider = provider;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service running");

            using var scope = _provider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
            Task.Run(() => consumer.Consume(topic), cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Event consumer service stoped");

            return Task.CompletedTask;
        }
    }
}