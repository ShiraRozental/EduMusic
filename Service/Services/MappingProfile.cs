using AutoMapper;
using Common.Dto;
using Repository.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<AdminRegisterDto, Admin>()
            .ForMember(dest => dest.ImageUrl,
                opt => opt.Ignore())
            .ForMember(dest => dest.Password,
                opt => opt.Ignore());

        CreateMap<Admin, AuthResponseDto>()
            .ForMember(dest => dest.Role,
                opt => opt.MapFrom(_ => "Admin"))
            .ForMember(dest => dest.Token,
                opt => opt.Ignore());

        CreateMap<Admin, AdminDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();

    }
}