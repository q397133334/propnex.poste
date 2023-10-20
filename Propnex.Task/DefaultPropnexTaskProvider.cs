using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Propnex
{
    public class DefaultPropnexTaskProvider : IPropnexTaskProvider
    {
        private PropnexTasks Tasks { get; set; }

        public DefaultPropnexTaskProvider()
        {
            Tasks = new PropnexTasks();
        }

        public PropnexTasks Get(string content)
        {

            Tasks = new PropnexTasks(content);
            return Tasks;
        }
    }
}
