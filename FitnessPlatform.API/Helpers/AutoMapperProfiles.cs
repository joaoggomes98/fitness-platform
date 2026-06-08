using AutoMapper;
using FitnessPlatform.API.DTOs;
using FitnessPlatform.API.Models;

namespace FitnessPlatform.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Add AutoMapper mappings here.
            // Example:
            // CreateMap<User, UserDto>().ReverseMap();
            // CreateMap<Workout, WorkoutDto>().ReverseMap();
            // CreateMap<Exercise, ExerciseDto>().ReverseMap();

                CreateMap<AppUser, UserDto>()
.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
    