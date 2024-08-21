using System.Text;
using System.Text.Json;
using ProducerService.Dtos;
using RabbitMQ.Client;

namespace ProducerService.AysncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], Port = int.Parse(_configuration["RabbitMQPort"])};

            try
            {
                _connection = factory.CreateConnection("rabbit");
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout, durable: true);
                // _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Direct, durable: true);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to RabbitMQ:");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Couldn't connect to RabbitMQ: {ex.Message}");
            }

        }

        public void PublishNewLocation(LocationPublishDto locationPublishDto) 
        {
            var message = JsonSerializer.Serialize(locationPublishDto);
            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ connection open, sending message...");
                SendMessage(message);
            }
            else 
                Console.WriteLine("--> RabbitMQ connection closed, not sending message...");
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "trigger", routingKey: "", basicProperties: null, body: body);
            Console.WriteLine($"--> Message sent: {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("Message bus disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) 
        {
            Console.WriteLine("--> RabbitMQ connection shutdown");
        }
    }
}