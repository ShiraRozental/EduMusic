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

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullNameUser))
            .ReverseMap()
            .ForMember(dest => dest.FullNameUser, opt => opt.MapFrom(src => src.FullName));

        CreateMap<User, UserProvisioningDto>()
                .ReverseMap();

        CreateMap<Song, SongSearchResultDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.CategoryID,
                opt => opt.MapFrom(src => src.CategoryID));

        CreateMap<Tag, TagDto>()
            .ForMember(dest => dest.TagName, opt => opt.MapFrom(src => src.TagText)).ReverseMap();

        CreateMap<Category, CategoryDto>()
     .ForMember(dest => dest.CategoryID, opt => opt.MapFrom(src => src.CategoryID))
     .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
     .ForMember(dest => dest.ParentCategoryID, opt => opt.MapFrom(src => src.ParentCategoryID));
    }
}