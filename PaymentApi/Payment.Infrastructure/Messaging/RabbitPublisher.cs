using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Payment.Infrastructure.Messaging
{
    public class RabbitPublisher
    {
        private readonly string _host;

        public RabbitPublisher(string host)
        {
            _host = host;
        }

        public void Publish(object message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _host
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "payment-queue",
                durable: false,
                exclusive: false,
                autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: "",
                routingKey: "payment-queue",
                body: body);
        }
    }
}