using System;

namespace Propnex.Poster.Dtos
{
    public class PnTaskLogDto
    {
        public Guid PntaskId { get; set; }
        public Guid MachineId { get; set; }
        public string Ip { get; set; }
        public string Message { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
