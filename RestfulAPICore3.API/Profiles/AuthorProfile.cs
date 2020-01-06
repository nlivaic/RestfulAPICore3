using AutoMapper;
using API.Helpers;
using API.Entities;
using API.Models;

namespace API.Profiles
{
    public class AuthorProfile : Profile
    {
        public AuthorProfile()
        {
            CreateMap<Author, AuthorDto>()
                .ForMember(
                    dest => dest.Name,
                    options => options.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(
                    dest => dest.Age,
                    options => options.MapFrom(src => src.DateOfBirth.GetCurrentAge())
                );
            CreateMap<AuthorForCreationDto, Author>();
            CreateMap<Author, AuthorFullDto>();
        }
    }
}