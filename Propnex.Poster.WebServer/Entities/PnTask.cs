using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Propnex.Poster.WebServer.Entities
{
    public class PnTask : AuditedAggregateRoot<Guid>
    {
        public string Number { get; set; }

        public string AccountId { get; set; }

        public string Password { get; set; }

        public string TargetPortal { get; set; }

        public string Country { get; set; }

        public string Source { get; set; }

        public Share.TaskStatus Status { get; set; } = Share.TaskStatus.Wait;

        public ICollection<PnTaskItem> PnTaskItems { get; set; }

    }

    public class PnTaskItem : Entity<Guid>
    {
        public Guid PnTaskId { get; set; }

        public string PnTaskNumber
        {
            get; set;
        }
        public string Number { get; set; }

        public Share.TaskStatus Status { get; set; }
    }
}
