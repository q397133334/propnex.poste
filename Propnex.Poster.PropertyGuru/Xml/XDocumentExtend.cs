using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Propnex.Poster.PropertyGuru.Xml
{
    public static class XDocumentExtend
    {
        public static int ElementInt(this XElement element, string name, int nullValue = 0)
        {
            if (element == null)
                return nullValue;
            var findElement = element.Element(name);
            if (findElement == null)
                return nullValue;
            try
            {

                return Convert.ToInt32(findElement.Value);
            }
            catch (Exception)
            {
                return nullValue;
            }
        }

        public static double ElementDouble(this XElement element, string name, double nullValue = 0)
        {
            if (element == null)
                return nullValue;
            var findElement = element.Element(name);
            if (findElement == null)
                return nullValue;
            try
            {

                return Convert.ToDouble(findElement.Value);
            }
            catch (Exception)
            {
                return nullValue;
            }
        }

        public static bool ElementBool(this XElement element, string name, bool nullValue = false)
        {
            if (element == null)
                return nullValue;
            var findElement = element.Element(name);
            if (findElement == null)
                return nullValue;
            try
            {
                return Convert.ToBoolean(findElement.Value);
            }
            catch (Exception)
            {
                return nullValue;
            }
        }

        public static string ElementString(this XElement element, string name, string nullValue = "")
        {
            if (element == null)
                return nullValue;
            var findElement = element.Element(name);
            if (findElement == null)
            {
                return nullValue;
            }
            return findElement.Value;
        }

        public static T GetElementValue<T>(this XElement element, string name, T nullValue = default)
        {
            try
            {
                if (element == null)
                    return nullValue;
                var findElement = element.Element(name);
                if (findElement == null)
                {
                    return nullValue;
                }
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(findElement.Value);
                }
                return nullValue;
            }
            catch (Exception)
            {
                return nullValue;
            }
        }

        public static XElement FindAttribute(this IEnumerable<XElement> elements, string aName, string value)
        {
            var query = from e in elements
                        where e.Attribute(aName).Value == value
                        select e;
            return query.FirstOrDefault();
        }

        public static T GetAttributeValue<T>(this XElement element, string name, T nullValue = default(T))
        {
            try
            {
                if (element == null)
                    return nullValue;
                var value = element.Attribute(name).Value;
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(value);
                }
                return nullValue;
            }
            catch (Exception)
            {
                return nullValue;
            }

        }
    }
}
