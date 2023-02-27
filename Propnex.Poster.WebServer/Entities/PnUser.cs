using Volo.Abp.Domain.Entities;

namespace Propnex.Poster.WebServer.Entities
{
    public class PnUser : Entity<Guid>
    {
        public string Account { get; set; }

        public string Password { get; set; }

        public string TokenJson { get; set; }

        public string LoginMessage { get; set; }

        public string UserType { get; set; } = "GURU";
    }
}
