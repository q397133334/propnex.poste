using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Propnex.Poster.WebServer.Entities
{
    public class PnTask : AuditedAggregateRoot<Guid>
    {
        public PnTask()
        {
            PnTaskItems=new List<PnTaskItem>();
        }

        public string Number { get; set; }

        public string ClientId { get; set; }

        public string AccountId { get; set; }

        public string Password { get; set; }

        public string TargetPortal { get; set; }

        public string Country { get; set; }

        public string Source { get; set; }

        public int RetryCount { get; set; } = 0;

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

        public virtual PnTask PnTask { get; set; }
    }

    public class PnTaskLog: Entity<Guid>
    {
        public Guid PntaskId { get; set; }

        public Guid MachineId { get; set; }

        public string Ip { get; set; }

        public string Message { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
