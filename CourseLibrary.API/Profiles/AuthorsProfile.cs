using AutoMapper;
using CourseLibrary.API.Helpers;

namespace CourseLibrary.API.Profiles
{
  public class AuthorsProfile : Profile
  {
    public AuthorsProfile()
    {
      CreateMap<Entities.Author, Models.AuthorDto>()
        .ForMember(
          dst => dst.Name,
          opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
        .ForMember(
          dst => dst.Age,
          opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge()));

      CreateMap<Models.AuthorForCreationDto, Entities.Author>();
    }
  }
}
