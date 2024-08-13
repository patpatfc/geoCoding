namespace ConsumerService.EventProcessing
{
    public interface IEventProcessor
    {
        void ProcessEvent(string message);
    }
}