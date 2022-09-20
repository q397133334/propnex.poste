using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru
{
    public class UnitValueModel<T>
    {
        public string Unit { get; set; }

        public T Value { get; set; }
    }
}
