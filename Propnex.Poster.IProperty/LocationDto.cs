using System;
using System.Collections.Generic;
using System.Text;

namespace Propnex.Poster.IProperty
{
    public class LocationDto
    {
        public PlaceDto location { get; set; }

        public string id { get; set; }

        public object extension { get; set; } = new object();

        public int saleableAreaMeasurementCode { get; set; }
    }
}
