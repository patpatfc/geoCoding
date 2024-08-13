using AutoMapper;
using ConsumerService.Dtos;
using ConsumerService.Models;

namespace ConsumerService.Profiles
{
    public class ConsumerProfile : Profile
    {
        public ConsumerProfile()
        {
            CreateMap<LocationPublishedDto, Location>();
        }
    }
}