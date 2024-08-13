using AutoMapper;
using ProducerService.Dtos;
using ProducerService.Models;

namespace ProducerService.Profiles
{
    public class ProducerProfile : Profile
    {
        public ProducerProfile()
        {
            CreateMap<Location, LocationPublishDto>();
        }
    }
}