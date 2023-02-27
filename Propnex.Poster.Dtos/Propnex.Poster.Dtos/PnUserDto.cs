using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Propnex.Poster.Dtos
{
    public class PnUserDto : EntityDto<Guid>
    {
        public string Account { get; set; }

        public string Password { get; set; }

        public string TokenJson { get; set; }

        public string LoginMessage { get; set; }

        public string UserType { get; set; } = "GURU";
    }
}
