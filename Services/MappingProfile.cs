using AutoMapper;
using Models.CoreModels;
using Models.DataModels;
using Models.DataTransferObjects;

namespace Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        RegisterUserMappings();
    }

    private void RegisterUserMappings()
    {
        CreateMap<RegisterUserDto, UserDataModel>()
            .ForMember(src => src.Password, dest => dest.Ignore());
        
        CreateMap<UserDataModel, User>()
            .ForMember(src => src.Password, dest => dest.Ignore());
    }
}