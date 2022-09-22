using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Propnex.Poster.PropertyGuru
{
    public static class DefaultTitles
    {
        public static List<string> Titles = new List<string>()
        {
            "Contact us today to discuss your needs",
            "Contact us now for more information",
            "Call now to enquire",
            "Call us now to ask questions",
            "Contact us today to make an inquiry",
            "Call us to make an enquiry",
            "Contact us today to make an enquiry",
            "Call us to make an inquiry"
        };

        public static object obj = new object();
        public static Random Random = new Random(obj.GetHashCode());

        public static string GetTitle()
        {
            return Titles[Random.Next(0, Titles.Count() - 1)];
        }
    }
}
