using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Task
{
    public interface IPropnexTaskProvider
    {
        PropnexTasks GetTasks(string content);
    }
}
