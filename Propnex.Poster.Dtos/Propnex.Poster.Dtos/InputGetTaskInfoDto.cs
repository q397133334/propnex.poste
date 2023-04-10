using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.Dtos
{
    public class InputGetTaskInfoDto
    {
        public Guid MachineId { get; set; }

        public string TargetPortal { get; set; } = "GURU";

    }
}
