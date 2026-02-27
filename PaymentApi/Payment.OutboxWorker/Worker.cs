using Payment.Infrastructure.Data;
using RabbitMQ.Client;
using System.Text;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {

            using var scope =
                _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                .GetRequiredService<PaymentDbContext>();

            var messages =
                context.Outbox
                .Where(x => !x.Processed)
                .ToList();


            var factory =
                new ConnectionFactory()
                {
                    HostName = "rabbitmq"
                };

            var connection =
                factory.CreateConnection();

            var channel =
                connection.CreateModel();

            foreach (var msg in messages)
            {

                var body =
                    Encoding.UTF8.GetBytes(msg.Payload);

                channel.BasicPublish(
                    "",
                    "paymentQueue",
                    body: body);

                msg.Processed = true;
            }

            await context.SaveChangesAsync();

            await Task.Delay(5000);
        }
    }
}