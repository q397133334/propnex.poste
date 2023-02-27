using AutoMapper;

namespace Propnex.Poster.WebServer.ObjectMapping;

public class PosterAutoMapperProfile : Profile
{
    public PosterAutoMapperProfile()
    {
        /* Create your AutoMapper object mappings here */

        CreateMap<Entities.PnTask, Dtos.PnTaskDto>();
        CreateMap<Dtos.CreateUpdatePnTaskDto, Entities.PnTask>();
        CreateMap<Entities.PnUser,Dtos.PnUserDto>().ReverseMap();
    }
}
