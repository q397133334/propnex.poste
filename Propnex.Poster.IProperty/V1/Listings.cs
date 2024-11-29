using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty.V1
{
    public class ListingsData
    {
        public Listings listings { get; set; }
    }

    public class Listings
    {
        public List<Listing> Data { get; set; }
    }


}
