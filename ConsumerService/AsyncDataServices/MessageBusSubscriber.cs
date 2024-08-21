using System.Text;
using ConsumerService.EventProcessing;
using ConsumerService.Models;
using ConsumerService.Threads;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private readonly RequestLimitService _requestLimitService;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private List<ProcessEventThread> _processEventThreads = new List<ProcessEventThread>();
        private List<Thread> _threads = new List<Thread>();

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor, RequestLimitService requestLimitService)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
            _requestLimitService = requestLimitService;
            InitializeRabbitMQ();
            InitializeProcessEventThreads(3);
        }

        private void InitializeRabbitMQ() 
        {
            var factory = new ConnectionFactory() { HostName= _configuration["RabbitMQHost"], Port= int.Parse(_configuration["RabbitMQPort"])};

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            // Actively declare a server-named exclusive, autodelete, non-durable queue.
            _queueName = _channel.QueueDeclare("test", true, false, false).QueueName;
            _channel.QueueBind(queue: _queueName,
                exchange: "trigger",
                routingKey: "test");
                
            Console.WriteLine("--> Listening on the Message Bus...");

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        private void InitializeProcessEventThreads(int numOfThreads)
        {
            for (int i = 0; i < numOfThreads; i++)
            {
                ProcessEventThread processEventThread = new ProcessEventThread(_channel, _eventProcessor, _requestLimitService, i);
                Thread thread = new Thread(() => processEventThread.ProcessEvent());
                thread.Start();
                _processEventThreads.Add(processEventThread);
                _threads.Add(thread);
            }
            Console.WriteLine("--> Initialized Process Event Threads...");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (ModuleHandle, ea) =>
            {
                // Console.WriteLine("--> Event Received");

                int min = _processEventThreads[0].GetQueueLength();
                for (int i = 1; i < _processEventThreads.Count; i++)
                {
                    if (_processEventThreads[i].GetQueueLength() < _processEventThreads[min].GetQueueLength())
                        min = i;
                }
                _processEventThreads[min].Queue(new Message{Body=ea.Body, DeliveryTag=ea.DeliveryTag});
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _processEventThreads.ForEach(t => t.KillThread());
            _threads.ForEach(t => t.Abort());
            Console.WriteLine("--> Connection shutdown");

        }

        public override void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }

            base.Dispose();
        }
    }
}