using Propnex.Poster.Share;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Propnex.Poster.Dtos
{
    public class PnTaskDto : AuditedEntityDto<Guid>
    {
        public string Number { get; set; }

        public string AccountId { get; set; }

        public string Password { get; set; }

        public string TargetPortal { get; set; }

        public string Country { get; set; }

        public string Source { get; set; }

        public int RetryCount { get; set; } = 0;

        public TaskStatus Status { get; set; } = TaskStatus.Wait;
    }
}
