using ProducerService.Dtos;

namespace ProducerService.AysncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewLocation(LocationPublishDto locationPublishDto);
        void Dispose();

    }
}