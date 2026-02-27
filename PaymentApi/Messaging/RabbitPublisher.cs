using RabbitMQ.Client;
using System.Text;

namespace Payment.Api.Messaging
{
    public class RabbitPublisher
    {
        private readonly IConnection _connection;

        public RabbitPublisher()
        {
            var factory =
                new ConnectionFactory()
                {
                    HostName = "rabbitmq"
                };

            _connection = factory.CreateConnection();
        }

        public void Publish(string message)
        {
            using var channel =
                _connection.CreateModel();

            channel.QueueDeclare(
                "payments",
                true,
                false,
                false);

            var body =
                Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                "",
                "payments",
                body: body);
        }
    }
}