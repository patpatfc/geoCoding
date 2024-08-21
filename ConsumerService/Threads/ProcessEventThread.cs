using System.Collections.Concurrent;
using System.Text;
using ConsumerService.EventProcessing;
using ConsumerService.Models;
using RabbitMQ.Client;

namespace ConsumerService.Threads
{
    public class ProcessEventThread
    {

        private ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();
        private readonly IModel _channel;
        private readonly IEventProcessor _eventProcessor;
        int _threadId;
        private readonly AutoResetEvent _waitHandle;
        private bool loop = true;
        private readonly RequestLimitService _requestLimitService;

        public ProcessEventThread(IModel channel, IEventProcessor eventProcessor, RequestLimitService requestLimitService, int threadId)
        {
            _channel = channel;
            _eventProcessor = eventProcessor;
            _threadId = threadId;
            _waitHandle = new AutoResetEvent(false);
            _requestLimitService = requestLimitService;
        }

        public void ProcessEvent()
        {
            while (loop)
            {
                Console.WriteLine("--> ProcessEventThread: Processing Event from thread " + _threadId);
                
                Message message;
                queue.TryDequeue(out message);

                if (message != null)
                {
                    while (!_requestLimitService.RequestLimitAvailable(_threadId)) {
                        
                    }
                    var body = message.Body;
                    var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                    _eventProcessor.ProcessEvent(notificationMessage);
                    _channel.BasicAck(message.DeliveryTag, multiple: false);
                } else {                    
                    _waitHandle.WaitOne(300);
                }
            }
        }

        public int GetQueueLength()
        {
            return queue.Count;
        }

        public bool Queue(Message message)
        {
            try
            {
                queue.Enqueue(message);
                _waitHandle.Set();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void KillThread()
        {
            loop = false;
        }
    }
}