using Propnex.Poster.WebServer.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Propnex.Poster.WebServer.Services
{
    public interface IPnTaskAppService: ICrudAppService< //Defines CRUD methods
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting
            Dtos.CreateUpdatePnTaskDto> //Used
    {

    }

    public class PnTaskAppService : CrudAppService<
            Entities.PnTask, //The Book entity
            Dtos.PnTaskDto, //Used to show books
            Guid, //Primary key of the book entity
            PagedAndSortedResultRequestDto, //Used for paging/sorting
            Dtos.CreateUpdatePnTaskDto>, IPnTaskAppService
    {
        public PnTaskAppService(IRepository<PnTask, Guid> repository) : base(repository)
        {
        }
    }

}
