
using System.Net;
using Newtonsoft.Json; 
using AutoMapper;
using ConsumerService.Dtos;
using ConsumerService.Models;

namespace ConsumerService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.LocationPublished:
                    convertToLocation(message);
                    break;
                default:
                    break;
            }

        }

        private EventType DetermineEvent(string notificationMessaeg)
        {
            Console.WriteLine("--> Determining Event");

            var eventType = System.Text.Json.JsonSerializer.Deserialize<GenericEventDto>(notificationMessaeg);

            switch(eventType.Event)
            {
                case "Location_Published":
                    Console.WriteLine("--> Location Published event detected");
                    return EventType.LocationPublished;
                default:
                    Console.WriteLine("--> Could not determine event type");
                    return EventType.Undetermined;
            }
        }

        private void convertToLocation(string locationPublishedMessage)
        {
            Console.WriteLine("--> Processing Location Published Event");

            var locationPublishedDto = System.Text.Json.JsonSerializer.Deserialize<LocationPublishedDto>(locationPublishedMessage);

            try
            {
                var location = _mapper.Map<Location>(locationPublishedDto);

                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&key=apiKey", location.latitude, location.longitude); 

                var json = new WebClient().DownloadString(requestUri);
                GoogleGeocoderResponseDto results = JsonConvert.DeserializeObject<GoogleGeocoderResponseDto>(json);
                
                if (results.status == "OK")
                {
                    var address = results.results[0].formatted_address;
                    Console.WriteLine($"--> Location: {address}");
                }
                else
                {
                    Console.WriteLine("--> Unable to determine location");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Error while returning location coordiantes {ex.Message}");
            }
        }
    }

    enum EventType
    {
        LocationPublished,
        Undetermined
    }
}