using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru
{
    public class QueryLocale
    {
        public string DisplayDescription { get; set; }

        public string DisplayText { get; set; }

        public string DisplayType { get; set; }

        public string ObjectId { get; set; }

        public string ObjectType { get; set; }
    }


    public class QueryProject
    {
        public List<Address> Addresses { get; set; }
    }

    public class Address
    {
        public string id { get; set; }
        public string external_id { get; set; }
    }

}
