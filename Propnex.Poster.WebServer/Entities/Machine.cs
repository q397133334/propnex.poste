namespace Propnex.Poster.WebServer.Entities
{
    public class Machine : Volo.Abp.Domain.Entities.Auditing.AuditedEntity<Guid>
    {
        public string Number { get; set; }

        public string Name { get; set; }

        public string RomateName { get; set; }

        public string RomatePassword { get; set; }
    }
}
