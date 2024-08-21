using System.Text;
using ProducerService.AysncDataServices;
using RabbitMQ.Client;

namespace ProducerService.AysncDataServices
{
    public class ProducerServicet : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IMessageBusClient _messageBusClient;
        int count = 0;

        public ProducerServicet(IMessageBusClient messageBusClient)
        {
            _messageBusClient = messageBusClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(PublishMessage, null, TimeSpan.Zero, TimeSpan.FromSeconds(.1));
            return Task.CompletedTask;
        }

        private void PublishMessage(object state)
        {

            Console.WriteLine($"--> Produced message {count++}");
            var platformPublishedDto = new Dtos.LocationPublishDto
            {
                // generate random double longitute (+180 to -180) and latitude (+90 to -90) with 6 decimals
                longitute = count, // Math.Round(new Random().NextDouble() * 360 - 180, 6),
                latitude = Math.Round(new Random().NextDouble() * 180 - 90, 6),
                Event = "Location_Published",
            };

            _messageBusClient.PublishNewLocation(platformPublishedDto);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _messageBusClient?.Dispose();
        }
    }
}