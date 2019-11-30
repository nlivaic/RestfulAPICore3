using API.Entities;
using API.Models;
using AutoMapper;

namespace API.Profiles
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseDto>();
            CreateMap<CourseForCreationDto, Course>();
        }
    }
}