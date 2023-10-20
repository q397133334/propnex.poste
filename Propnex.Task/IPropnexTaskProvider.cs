using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex
{
    public interface IPropnexTaskProvider
    {
        PropnexTasks Get(string content);
    }
}
