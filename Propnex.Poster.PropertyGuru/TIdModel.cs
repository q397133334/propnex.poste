using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru
{
    /// <summary>
    /// Id 字段，用于继承，方便查看
    /// D field, used for inheritance, easy to view
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IdModel<T>
    {
        public T Id { get; set; }
    }


}
