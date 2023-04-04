using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public static class IPropertyHelper
    {
        public static int PropertyTypeCode(string name)
        {
            switch (name)
            {
                case "Flat":
                    return 1;
                case "Apartment":
                    return 2;
                case "Condominium":
                    return 3;
                case "Serviced residence":
                    return 4;
                case "Terrace house":
                    return 5;
                case "Townhouse":
                    return 6;
                case "Semi-D house":
                    return 7;
                case "Bungalow":
                    return 8;
                case "Cluster house":
                    return 9;
                case "Bungalow land":
                    return 10;
                case "Residential land":
                    return 11;
                default: return -1;
            }
        }

        public static int ListingType(string name)
        {
            name = name.ToLower();
            if("buy"==name)
            {
                return 1;
            }
            if("rent"==name)
            {
                return 2;
            }
            return -1;
        }

        public static int CategoryType(string name) 
        { 
            name=name.ToLower();
            if("residential" == name)
            {
                return 1;
            }
            if("commercial"==name)
            {
                return 2;
            }
            return 1;
        }
    }
}
