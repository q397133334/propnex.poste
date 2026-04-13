using Volo.Abp.Application.Dtos;

namespace Propnex.Poster.Dtos
{
    public class PnTaskListInput : PagedAndSortedResultRequestDto
    {
        public string NumberFilter { get; set; }
    }
}
