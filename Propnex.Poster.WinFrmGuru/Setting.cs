using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.Guru
{
    public class Setting
    {
        public static Guru.Setting setting = null;


        public static Setting Get()
        {
            if (setting == null)
            {
                setting = Abp.Dependency.IocManager.Instance.Resolve<ConfigurationJson<Setting>>().Value;
            }
            return setting;
        }

        public string AnyDesk { get; set; } = "";

        public string Id { get; set; } = "";
    }
}
