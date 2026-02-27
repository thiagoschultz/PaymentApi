using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Messaging;

namespace Payment.Api.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {

        private readonly IServiceProvider _provider;
        private readonly RabbitPublisher _publisher;

        public OutboxProcessor(IServiceProvider provider)
        {
            _provider = provider;
            _publisher = new RabbitPublisher();
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {

                using var scope =
                    _provider.CreateScope();

                var context =
                    scope.ServiceProvider
                    .GetRequiredService<PaymentDbContext>();


                var events =
                    await context.OutboxEvents
                    .Where(x => x.ProcessedAt == null)
                    .ToListAsync();


                foreach (var e in events)
                {
                    _publisher.Publish(e.Payload);

                    e.ProcessedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();

                await Task.Delay(5000);
            }
        }
    }
}