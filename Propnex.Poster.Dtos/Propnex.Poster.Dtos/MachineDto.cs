using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Propnex.Poster.Dtos
{
    public class MachineDto: AuditedEntityDto<Guid>
    {
        public string Number { get; set; }

        public string Name { get; set; }

        public string RomateName { get; set; }

        public string RomatePassword { get; set; }

        public DateTime OnlineTime { get; set; }

        public bool IsEnable { get; set; }
    }
}
